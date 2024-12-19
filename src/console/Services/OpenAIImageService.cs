using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Net.Http.Headers;

namespace AzureOpenAI
{

  class ImageReguest {
    
  }

  class ImageResopnse {

  }

  class OpenAIImageService
  {
    private readonly string _apiKey;
    private readonly string _endpoint;
    private string _model { get; set; }
    private HttpClient _httpClient;

    public OpenAIImageService(IConfiguration config) {
      _apiKey = config["AzureOpenAI:ApiKey"] ?? "";
      _endpoint = config["AzureOpenAI:Endpoint"] ?? "";
      _model = config["AzureOpenAI:ImageModel"] ?? "";
      _httpClient = new HttpClient();
    }

  }
}