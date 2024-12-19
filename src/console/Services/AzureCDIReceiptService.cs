
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Core;
using Azure;

using Microsoft.Extensions.Configuration;
using AIDotChat;
using System.Collections;
using System.Text.Json;
using Newtonsoft.Json;

// using DocumentFormat.OpenXml.Spreadsheet;
// using ClosedXML.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
using DocumentFormat.OpenXml.Math;

namespace AzureDocumentAI
{
  enum ExtractionMode
  {
    Jpg, Folder, None
  }

  class ExtractionModeDescription
  {
    public static string get(ExtractionMode mode)
    {
      return mode switch
      {
        ExtractionMode.Jpg => "jpg",
        ExtractionMode.Folder => "folder",
        ExtractionMode.None => "",
        _ => "unknown"
      };
    }
  }

  enum SaveMode
  {
    Xlsx, Json, None
  }

  class SaveModeDescription
  {
    public static string get(SaveMode mode)
    {
      return mode switch
      {
        SaveMode.Xlsx => ".xlsx",
        SaveMode.Json => ".json",
        SaveMode.None => "",
        _ => "unknown"
      };
    }
  }
  class AzureCDIReceiptService
  {
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _modelId;
    private DocumentAnalysisClient _client;

    public AzureCDIReceiptService(IConfiguration configuration)
    {
      _apiKey = configuration["AzureDocumentAI:ApiKey"] ?? "";
      _endpoint = configuration["AzureDocumentAI:Endpoint"] ?? "";
      _modelId = configuration["AzureDocumentAI:Model"] ?? "";

      var credential = new AzureKeyCredential(_apiKey);
      var uri = new Uri(_endpoint);

      _client = new DocumentAnalysisClient(uri, credential);
    }


    public async Task ExtractOne(string sourcePath, string destPath, SaveMode mode, bool verbose)
    {
      try
      {
        AnalyzeResult result = await ExtractReceipt(sourcePath);

        if (verbose)
        {
          foreach (AnalyzedDocument document in result.Documents)
          {
            Console.WriteLine($"Document of type: {document.DocumentType}");
            foreach (KeyValuePair<string, DocumentField> field in document.Fields)
            {
              string fieldName = field.Key;
              DocumentField fieldValue = field.Value;
              Console.WriteLine($"field: {fieldName}");
              Console.WriteLine($"  - value: {fieldValue.Content}");
              Console.WriteLine($"  - confidence: {fieldValue.Confidence * 100} %");

            }
          }
        }
        else
        {
          Console.WriteLine($"Extracted receipt form {sourcePath} to {destPath}");
        }

        // if (mode == SaveMode.Xlsx && ValidateDestFilename(ref destPath, mode))
        // {
        //   SaveReceiptAsXlsx(result, destPath);
        // }
        Console.WriteLine($"Save mode: {SaveModeDescription.get(mode)}");
        if (mode == SaveMode.Json && ValidateDestFilename(ref destPath, mode))
        {
          SaveReceiptAsJson(result, destPath);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to process receipt: {sourcePath}, {ex.Message}");
      }
    }

    bool ValidateDestFilename(ref string destPath, SaveMode mode)
    {
      if (string.IsNullOrWhiteSpace(destPath))
      {
        throw new ArgumentException("Destination file cannot be null or empty.");
      }
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

      if (string.IsNullOrEmpty(destExt) || !destExt.Equals(requiredExt, StringComparison.OrdinalIgnoreCase))
      {
        string fileName = Path.GetFileNameWithoutExtension(destPath);
        destPath = Path.Combine(destDir, $"{fileName}{requiredExt}");
      }

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

      if (!Directory.Exists(destDir))
      {
        Directory.CreateDirectory(destDir);
      }
      return true;
    }

    // public async Task ExtractMany(string dirname)
    // {

    // }

    private async Task<AnalyzeResult> ExtractReceipt(string imageFile)
    {
      if (!File.Exists(imageFile))
        throw new ArgumentException($"Path {imageFile} if not valid.");

      using (var imageStream = new FileStream(imageFile, FileMode.Open))
      {
        var operation = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _modelId,
            imageStream
        );
        if (operation == null)
          throw new Exception("Error in ExtractReceipt: Analyze operation is null");
        return operation.Value;
      }
    }


    // private void SaveReceiptAsXlsx(AnalyzeResult result, string filename)
    // {
    //   Excel.Application excelApp = new Excel.Application();
    //   excelApp.Visible = false;

    //   Excel.Workbook workbook;

    //   if (!File.Exists(filename))
    //   {
    //     // Create a new workbook
    //     workbook = excelApp.Workbooks.Add();
    //     // Save the new workbook
    //     workbook.SaveAs(filename);
    //   }
    //   else
    //   {
    //     workbook = excelApp.Workbooks.Open(filename);
    //   }

    //   bool sheetExists = false;
    //   foreach (Excel.Worksheet workSheet in workbook.Sheets)
    //   {
    //     if (workSheet.Name == "Receipts")
    //     {
    //       sheetExists = true;
    //       break;
    //     }
    //   }

    //   Excel.Worksheet sheet;
    //   if (!sheetExists)
    //   {
    //     sheet = (Excel.Worksheet)workbook.Sheets.Add();
    //     sheet.Name = "Receipts";
    //   }
    //   else
    //   {
    //     sheet = (Excel.Worksheet)workbook.Sheets["Receipts"];
    //   }

    //   Excel.Range usedRange = sheet.UsedRange;
    //   int nextRow = usedRange.Rows.Count;

    //   foreach (AnalyzedDocument document in result.Documents)
    //   {
    //     // Column count might change
    //     Excel.Range intermediateUsedRange = sheet.UsedRange;
    //     int columnCount = usedRange.Columns.Count;

    //     foreach (KeyValuePair<string, DocumentField> field in document.Fields)
    //     {

    //       int columnIndex = -1;

    //       for (int i = 1; i < columnCount; i++)
    //       {
    //         Excel.Range header = (Excel.Range)usedRange.Cells[1, i];
    //         if (header != null && header.Value2 != null && string.Equals(header.Value2?.ToString().ToLower(), field.Key))
    //         {
    //           columnIndex = i;
    //         }
    //       }

    //       if (columnIndex == -1)
    //       {
    //         Excel.Range newHeader = (Excel.Range)sheet.Cells[1, columnCount];
    //         newHeader.Font.Bold = true;
    //         newHeader.Font.Size = 14;
    //         newHeader.HorizontalAlignment = XlHAlign.xlHAlignCenter;
    //         newHeader.VerticalAlignment = XlVAlign.xlVAlignBottom;
    //         newHeader.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightCoral);
    //         newHeader.Value2 = field.Key;
    //         columnIndex = columnCount;
    //       }

    //       Excel.Range cell = (Excel.Range)sheet.Cells[nextRow, columnIndex];
    //       if (cell != null)
    //       {
    //         cell.Value2 = field.Value.Content;
    //       }
    //       nextRow += 1;
    //     }
    //     workbook.SaveAs(filename);
    //   }
    //   workbook.Close();
    //   excelApp.Quit();
    // }

    private void SaveReceiptAsJson(AnalyzeResult result, string filename)
    {
      string json = JsonConvert.SerializeObject(result, Formatting.Indented);
      try
      {
        File.WriteAllText(filename, json);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while writing json file: {ex.Message}");
      }
      Console.WriteLine($"Saved json file: {filename}");
    }

  }
}