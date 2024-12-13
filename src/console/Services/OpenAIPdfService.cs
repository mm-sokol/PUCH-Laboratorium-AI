using System;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Json;
using System.Text.Json;

using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;

using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
// using Azure.AI.DocumentAnalysis;

using Azure.Core;

// using OpenAI;
// using OpenAI.Models;
// using OpenAI.Completions;

using Microsoft.Extensions.Configuration;
// using OpenAI.RealtimeConversation;

using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;
using System.Collections;
using Microsoft.AspNetCore.Identity;

namespace AIDotChat
{
  enum SummaryMode
  {
    Url,
    File,
    Folder,
    None
  }

  class SummaryModeDescription
  {
    public static string get(SummaryMode mode)
    {
      return mode switch
      {
        SummaryMode.Url => "url",
        SummaryMode.File => "file",
        SummaryMode.Folder => "folder",
        SummaryMode.None => "",
        _ => "unknown"
      };
    }
  }

  class OpenAIPdfService
  {
    private readonly string _apiKey;
    private readonly string _endpoint;

    private readonly string _model;
    private AzureOpenAIClient _client;
    private ChatClient _chat;

    private HttpClient _httpClient;

    private int _maxTokens { get; set; }
    private float _temperature { get; set; }
    private float _topP { get; set; }

    private readonly string _docApiKey;
    private readonly string _docEndpoint;

    private DocumentAnalysisClient _docClient;

    public OpenAIPdfService(IConfiguration configuration)
    {
      // _apiKey = configuration["OpenAI:ApiKey"] ?? "";
      // _client = new OpenAIClient(_apiKey);

      _apiKey = configuration["AzureOpenAI:ApiKey"] ?? "";
      _endpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
      _model = configuration["AzureOpenAI:Model"] ?? "";

      _client = new AzureOpenAIClient(new Uri(_endpoint), new System.ClientModel.ApiKeyCredential(_apiKey));
      _chat = _client.GetChatClient(_model);

      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
      _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["Azure:Subscription"]);

      _docApiKey = configuration["AzureDocumentAI:ApiKey"] ?? "";
      _docEndpoint = configuration["AzureDocumentAI:Endpoint"] ?? "";
      _docClient = new DocumentAnalysisClient(new Uri(_docEndpoint), new AzureKeyCredential(_docApiKey));

      _maxTokens = 200;
      _temperature = 0.5f;
      _topP = 0.95f;

    }

    public async Task Summarize(string textSource, string destination, SummaryMode mode, bool verbose)
    {
      switch (mode)
      {
        case SummaryMode.File:
          await SummarizeOne(textSource, destination, verbose);
          break;
        case SummaryMode.Folder:
          await SummarizeMany(textSource, destination, verbose);
          break;
        default:
          Console.WriteLine("SummaryMode currently unimplemented");
          break;
      }
    }

    private async Task<string> ExtractFromPdf(string filePath)
    {
      StringBuilder text = new StringBuilder();

      using (FileStream fs = File.OpenRead(filePath))
      {
        // Call the AnalyzeDocument method
        AnalyzeDocumentOperation operation = await _docClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fs);

        // Wait for the operation to complete
        AnalyzeResult result = await operation.WaitForCompletionAsync();

        // Output the extracted text
        Console.WriteLine("Extracting text:");
        foreach (var page in result.Pages)
        {
          Console.WriteLine($"Page {page.PageNumber}");
          foreach (var line in page.Lines)
          {
            // Console.WriteLine(line.Content);
            text.Append(line.Content.ToString());
          }
        }
      }
      return text.ToString();
    }

    // private async Task<string> SummarizePdf(string sourcePath)
    private async Task<string> SummarizePdf(string sourcePath)
    {
      try
      {// List<ChatMessageContentPart> content = ExtractFromPdf(filePath);
        Console.WriteLine("Here");
        string text = await ExtractFromPdf(sourcePath);
        Console.WriteLine("Alse here");
        string prompt = $"Could you summarize this text for me: \n\n{text}";

        // var systemMessage = ChatMessage.CreateSystemMessage("You are an assistant tasked with summarizing text from PDF files.");
        var systemMessage = new OpenAIRequest.Message
        {
          Role = "system",
          Content = [ new OpenAIRequest.Content{
            Text = "You are an assistant tasked with summarizing text from PDF files.",
            Type = "text"
          }]
        };

        // var userMessage = ChatMessage.CreateUserMessage(prompt);
        var userMessage = new OpenAIRequest.Message
        {
          Role = "user",
          Content = [ new OpenAIRequest.Content{
            Text = prompt,
            Type = "text"
          }]
        };

        List<OpenAIRequest.Message> messages = [];
        // List<ChatMessage> messages = [];
        messages.Add(systemMessage);
        messages.Add(userMessage);

        Console.WriteLine("And here");
        // ChatCompletion summary = _chat.CompleteChat(messages);

        var payload = new OpenAIRequest
        {
          Model = _model,
          Messages = messages.ToArray(),
          Temperature = _temperature,
          Top_p = _topP,
          Max_tokens = _maxTokens
        };

        var summary = await _httpClient.PostAsJsonAsync(_endpoint, payload);

        if (!summary.IsSuccessStatusCode)
        {
          var errorContent = await summary.Content.ReadAsStringAsync();
          throw new Exception($"Error in SummarizePdf: http status code {summary.StatusCode}, message: {errorContent}");
        }

        var responseBody = await summary.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (responseObject?.Choices != null && responseObject.Choices.Length > 0)
        {
          return responseObject.Choices[0].Message.Content;
        }
        else
        {
          throw new Exception($"No response from OpenAI.");
        }
      }
      catch (Exception ex)
      {
        throw new Exception($"SummarizePdf exception: {ex.Message}");
      }
    }

    private async Task SummarizeOne(string sourcePath, string destPath, bool verbose)
    {
      try
      {
        string? destDir = Path.GetDirectoryName(destPath);

        if (string.IsNullOrEmpty(destDir))
          throw new Exception($"Invalid destination directory: {destPath}");

        string? destName = Path.GetFileName(destPath);
        if (string.IsNullOrEmpty(destName))
          throw new Exception($"Invelid destination filename: {destPath}");

        string? destExt = Path.GetExtension(destPath);

        if (string.IsNullOrEmpty(destExt) || !destExt.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
          destPath = Path.Combine(destDir, destName + ".pdf");

        Console.WriteLine($"Dest name: {destName}");
        Console.WriteLine($"Dest ext: {destExt}");
        Console.WriteLine($"Dest dir: {destDir}");
        Console.WriteLine($"Dest path: {destPath}");
        Console.WriteLine($"--------------------------------?");
        if (destPath.Contains("\0"))
        {
          Console.WriteLine("File path contains an invalid null character.");

        }
        if (sourcePath.Contains("\0"))
        {
          Console.WriteLine("In file path contains an invalid null character.");
        }



        Directory.CreateDirectory(destDir);
        string text = await SummarizePdf(sourcePath);

        if (verbose)
        {
          Console.WriteLine(":--------------------------------------------------------:");
          Console.WriteLine($" Summarizing: {sourcePath}");
          Console.WriteLine(":--------------------------------------------------------:");
          Console.WriteLine(text);
          Console.WriteLine($" Summary in: {destPath}");
        }
        else
        {
          Console.WriteLine($"Summary of {sourcePath} in: {destPath}");
        }

        SaveTextAsPdf(text, destPath);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to process file: {sourcePath}, {ex.Message}");
      }
    }

    private async Task SummarizeMany(string sourceDir, string destDir, bool verbose)
    {
      Directory.CreateDirectory(destDir);
      string[] pdfFiles = Directory.GetFiles(sourceDir, "*.pdf");

      foreach (string filePath in pdfFiles)
      {
        try
        {
          await SummarizeOne(filePath, filePath.Replace(sourceDir, destDir), verbose);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to process {filePath}: {ex.Message}");
        }
      }

    }

    private void SaveTextAsPdf(string text, string destFilePath)
    {
      PdfDocument document = new PdfDocument();
      var page = document.AddPage();

      XGraphics gfx = XGraphics.FromPdfPage(page);
      XFont font = new XFont("Verdana", 12, XFontStyle.Regular);
      gfx.DrawString(text, font, XBrushes.Black, new XRect(20, 20, page.Width - 40, page.Height - 40), XStringFormats.TopLeft);

      // Save the document to the specified path
      document.Save(destFilePath);
    }


  }
}