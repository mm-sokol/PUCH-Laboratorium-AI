using System;
using System.Text;
using System.Threading.Tasks;

using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;

// using OpenAI;
// using OpenAI.Models;
// using OpenAI.Completions;

using Microsoft.Extensions.Configuration;
// using OpenAI.RealtimeConversation;

using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;
using System.Collections;

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

    private int _maxTokens { get; set; }
    private float _temperature { get; set; }
    private float _topP { get; set; }

    public OpenAIPdfService(IConfiguration configuration)
    {
      // _apiKey = configuration["OpenAI:ApiKey"] ?? "";
      // _client = new OpenAIClient(_apiKey);

      _apiKey = configuration["AzureOpenAI:ApiKey"] ?? "";
      _endpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
      _model = configuration["AzureOpenAI:Model"] ?? "";

      _client = new AzureOpenAIClient(new Uri(_endpoint), new System.ClientModel.ApiKeyCredential(_apiKey));
      _chat = _client.GetChatClient(_model);

      _maxTokens = 200;
      _temperature = 0.5f;
      _topP = 0.95f;

    }

    public void Summarize(string textSource, string destination, SummaryMode mode, bool verbose)
    {
      switch (mode)
      {
        case SummaryMode.File:
          SummarizeOne(textSource, destination, verbose);
          break;
        case SummaryMode.Folder:
          SummarizeMany(textSource, destination, verbose);
          break;
        default:
          Console.WriteLine("SummaryMode currently unimplemented");
          break;
      }
    }

    private static string ExtractFromPdf(string filePath)
    {
      StringBuilder text = new StringBuilder();
      // List<ChatMessageContentPart> content = [];

      using (PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly))
      {
        foreach (var page in document.Pages)
        {
          // content.Add(ChatMessageContentPart.CreateTextPart(page.Contents.ToString()));
          text.AppendLine(page.Contents.ToString());
        }
      }
      // return content;
      return text.ToString();
    }

    // private async Task<string> SummarizePdf(string sourcePath)
    private string SummarizePdf(string sourcePath)
    {
      // List<ChatMessageContentPart> content = ExtractFromPdf(filePath);
      string text = ExtractFromPdf(sourcePath);
      string prompt = $"Could you summarize this text for me: \n\n{text}";

      var systemMessage = ChatMessage.CreateSystemMessage("You are an assistant tasked with summarizing text from PDF files.");

      var userMessage = ChatMessage.CreateUserMessage(prompt);

      List<ChatMessage> messages = [];
      messages.Add(systemMessage);
      messages.Add(userMessage);

      ChatCompletion summary = _chat.CompleteChat(messages);

      return summary.Content[0].Text;
    }

    private void SummarizeOne(string sourcePath, string destPath, bool verbose)
    {
      try
      {
        string? destDir = Path.GetDirectoryName(destPath);

        if (destDir == null)
          throw new Exception($"Invalid destination directory: {destPath}");

        string? destName = Path.GetFileName(destPath);
        if (string.IsNullOrEmpty(destName))
          throw new Exception($"Invelid destination filename: {destPath}");

        string? destExt = Path.GetExtension(destPath);

        if (string.IsNullOrEmpty(destExt) || !destExt.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
          destPath = Path.Combine(destDir, destName + ".pdf");

        Directory.CreateDirectory(destDir);
        string text = SummarizePdf(sourcePath);

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

    private void SummarizeMany(string sourceDir, string destDir, bool verbose)
    {
      Directory.CreateDirectory(destDir);
      string[] pdfFiles = Directory.GetFiles(sourceDir, "*.pdf");

      foreach (string filePath in pdfFiles)
      {
        try
        {
          SummarizeOne(filePath, filePath.Replace(sourceDir, destDir), verbose);
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