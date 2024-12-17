
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Core;
using Azure;

using Microsoft.Extensions.Configuration;

namespace AzureDocumentAI
{
  enum ExtractionMode {
   Jpg, Folder, None
  }

  class ExtractionModeDescription {
    public static string get(ExtractionMode mode) {
      return mode switch
      {
        ExtractionMode.Jpg => "jpg",
        ExtractionMode.Folder => "folder",
        ExtractionMode.None => "",
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

    public AzureCDIReceiptService(IConfiguration configuration) {
      _apiKey = configuration["AzureDocumentAI:ApiKey"] ?? "";
      _endpoint = configuration["AzureDocumentAI:Endpoint"] ?? "";
      _modelId = configuration["AzureDocumentAI:Model"] ?? "";

      var credential = new AzureKeyCredential(_apiKey);
      var uri = new Uri(_endpoint);

      _client = new DocumentAnalysisClient(uri, credential);
    }


    public async Task ExtractOneReceipt(string filename) {

    }

    public async Task ExtractManyReceipts(string dirname) {

    }
  }
}