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

using Microsoft.Extensions.Configuration;
// using OpenAI.RealtimeConversation;


namespace AzureOpenAI
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


  enum SaveMode
  {
    Pdf,
    Md,
    Txt,
    None
  }

  class SaveModeDescription
  {
    public static string get(SaveMode mode)
    {
      return mode switch
      {
        SaveMode.Pdf => ".pdf",
        SaveMode.Md => ".md",
        SaveMode.Txt => ".txt",
        SaveMode.None => "",
        _ => "unknown"
      };
    }
  }

  class OpenAIPdfService
  {
    private readonly string _apiKey;
    private readonly string _endpoint;

    private readonly string _model;

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

      // _client = new AzureOpenAIClient(new Uri(_endpoint), new System.ClientModel.ApiKeyCredential(_apiKey));
      // _chat = _client.GetChatClient(_model);

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
        Console.WriteLine("Extracting text...");
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
        string text = await ExtractFromPdf(sourcePath);
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
        string text = await SummarizePdf(sourcePath);
        if (ValidateDestFilename(ref destPath, SaveMode.Md))
        {
          SaveTextAsMd(text, destPath);
        }
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

    private void SaveTextAsMd(string text, string destFilePath)
    {
      try
      {
        File.WriteAllText(destFilePath, text);
        // Console.WriteLine($"Markdown file saved to {destFilePath}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred: {ex.Message}");
      }
    }

    private bool ValidateDestFilename(ref string destPath, SaveMode mode)
    {
      if (string.IsNullOrWhiteSpace(destPath))
      {
        throw new ArgumentException("Destination path cannot be null or empty.");
      }

      // Resolve relative path to absolute path
      destPath = Path.GetFullPath(destPath);

      string? destDir = Path.GetDirectoryName(destPath);
      if (string.IsNullOrEmpty(destDir))
      {
        throw new ArgumentException($"Invalid destination directory: {destPath}");
      }

      string? destName = Path.GetFileName(destPath);
      if (string.IsNullOrEmpty(destName))
      {
        throw new ArgumentException($"Invalid destination filename: {destPath}");
      }

      string? destExt = Path.GetExtension(destPath);
      string requiredExt = SaveModeDescription.get(mode);

      // Ensure the file has the required extension
      if (string.IsNullOrEmpty(destExt) || !destExt.Equals(requiredExt, StringComparison.OrdinalIgnoreCase))
      {
        string fileName = Path.GetFileNameWithoutExtension(destPath);
        destPath = Path.Combine(destDir, $"{fileName}{requiredExt}");
      }

      // Validate the resolved path structure
      char[] invalidPathChars = Path.GetInvalidPathChars();
      if (destPath.IndexOfAny(invalidPathChars) != -1)
      {
        throw new ArgumentException($"Destination path contains invalid characters: {destPath}");
      }

      char[] invalidFileChars = Path.GetInvalidFileNameChars();
      if (destName.IndexOfAny(invalidFileChars) != -1)
      {
        throw new ArgumentException($"Destination filename contains invalid characters: {destPath}");
      }
      // Ensure the directory exists or create it
      if (!Directory.Exists(destDir))
      {
        Directory.CreateDirectory(destDir);
      }

      return true;
    }
  }
}
