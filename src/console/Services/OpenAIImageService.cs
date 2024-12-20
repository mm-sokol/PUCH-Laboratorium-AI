using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Net.Http.Headers;
using Microsoft.Identity.Client;
using System.Text;

namespace AzureOpenAI
{

  class ImageReguest
  {
    public required string Prompt {get; set;}
    public required string Size {get; set;}
    public required int N {get; set;}
    public required string Quality {get; set;}
    public required string Style {get; set;}
  }

  class ImageData
  {
    public string? Url { get; set; }

    public string? Revised_Prompt { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
  }

  class ImageResponse
  {
    public required int Created { get; set; }
    public required List<ImageData> Data { get; set; } = new List<ImageData>();
  }

  enum GenerationMode
  {
    Jpg, Url, None
  }

  class GenerationModeDescription
  {
    public static string get(GenerationMode mode)
    {
      return mode switch
      {
        GenerationMode.Jpg => ".jpg",
        GenerationMode.Url => "url",
        GenerationMode.None => "",
        _ => "unknown"
      };
    }
  }

  class OpenAIImageService
  {
    private readonly string _apiKey;
    private readonly string _endpoint;
    private string _model { get; set; }
    private HttpClient _httpClient;

    private ImageReguest _request;
    private string _user;

    public OpenAIImageService(IConfiguration config)
    {
      _apiKey = config["AzureOpenAIImage:ApiKey"] ?? "";
      _endpoint = config["AzureOpenAIImage:Endpoint"] ?? "";
      _model = config["AzureOpenAIImage:Model"] ?? "";
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
      _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config["Azure:Subscription"]);

      // default values
      _request = new ImageReguest
      {
        Prompt = "",
        Size = "1024x1024",
        N = 1,
        Quality = "standard",
        Style = "vivid"
      };
    }

    public string GetModel()
    {
      return _model;
    }

    public void SetUser(string username)
    {
      _user = username;
    }

    private string GetColorOrSpace(int chosen, int line)
    {

      if (chosen == line)
        return "✅ \u001b[32m";
      else
        return "   ";
    }

    public void PromptForSize()
    {
      // 1024 x 1024, 1024 x 1792, 1792 x 1792
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"{_model.ToUpper()}: Choose the size options by navigating 🔽  and 🔼  keys, press Enter to select");
      Console.ForegroundColor = ConsoleColor.White;

      ConsoleKeyInfo key;
      int option = 0;
      bool selectionComplete = false;
      (int left, int top) = Console.GetCursorPosition();

      while (!selectionComplete)
      {
        Console.SetCursorPosition(left, top);

        Console.WriteLine($"{GetColorOrSpace(option, 0)}1024 x 1024\u001b[0m");
        Console.WriteLine($"{GetColorOrSpace(option, 1)}1024 x 1792\u001b[0m");
        Console.WriteLine($"{GetColorOrSpace(option, 2)}1792 x 1792\u001b[0m");

        key = Console.ReadKey(true);
        switch (key.Key)
        {
          case ConsoleKey.DownArrow:
            option = option++ % 3;
            break;

          case ConsoleKey.UpArrow:
            option = option-- % 3;
            break;

          case ConsoleKey.Enter:
            selectionComplete = true;
            break;
        }
      }
      Console.WriteLine($"Selected option {option}");
      _request.Size = option switch
      {
        2 => "1792x1792",
        1 => "1024x1792",
        _ => "1024x1024"
      };
    }

    public void PromptForQuality()
    {
      // hd, Standard
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine($"{_model.ToUpper()}: Choose the quality options by navigating 🔽  and 🔼  keys, press Enter to select");
      Console.ForegroundColor = ConsoleColor.Magenta;

      ConsoleKeyInfo key;
      int option = 0;
      bool selectionComplete = false;
      (int left, int top) = Console.GetCursorPosition();

      while (!selectionComplete)
      {
        Console.SetCursorPosition(left, top);

        Console.WriteLine($"{GetColorOrSpace(option, 0)}Standard\u001b[0m");
        Console.WriteLine($"{GetColorOrSpace(option, 1)}HD\u001b[0m");


        key = Console.ReadKey(true);
        switch (key.Key)
        {
          case ConsoleKey.DownArrow:
            option = option++ % 2;
            break;

          case ConsoleKey.UpArrow:
            option = option-- % 2;
            break;

          case ConsoleKey.Enter:
            selectionComplete = true;
            break;
        }
      }
      Console.WriteLine($"Selected option {option}");
      _request.Quality = option switch
      {
        1 => "hd",
        _ => "standard"
      };
    }

    public void PromptForStyle()
    {
      // vivid, natural
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"{_model.ToUpper()}: Choose the style options by navigating 🔽  and 🔼  keys, press Enter to select");
      Console.ForegroundColor = ConsoleColor.Yellow;


      ConsoleKeyInfo key;
      int option = 0;
      bool selectionComplete = false;
      (int left, int top) = Console.GetCursorPosition();

      while (!selectionComplete)
      {
        Console.SetCursorPosition(left, top);

        Console.WriteLine($"{GetColorOrSpace(option, 0)}Vivid\u001b[0m");
        Console.WriteLine($"{GetColorOrSpace(option, 1)}Natural\u001b[0m");


        key = Console.ReadKey(true);
        switch (key.Key)
        {
          case ConsoleKey.DownArrow:
            option = option++ % 2;
            break;

          case ConsoleKey.UpArrow:
            option = option-- % 2;
            break;

          case ConsoleKey.Enter:
            selectionComplete = true;
            break;
        }
      }
      Console.WriteLine($"Selected option {option}");
      _request.Style = option switch
      {
        0 => "vivid",
        _ => "natural"
      };
    }

    public void PromptForPrompt()
    {
      Console.WriteLine($"{_model.ToUpper()}: Could you describe you desired image? (press double Enter after you're finished)");
      StringBuilder userInput = new StringBuilder();
      string line = string.Empty;

      do
      {
        Console.Write($"{_user}: ");
        line = Console.ReadLine() ?? "";
        userInput.Append(line);
      } while (!string.IsNullOrWhiteSpace(line));

      _request.Prompt = userInput.ToString();
    }


    private async Task<ImageResponse?> MakeRequest()
    {
      if (string.IsNullOrWhiteSpace(_request.Prompt))
      {
        return null;
      }
      if (_request.N < 0 || _request.N > 3)
      {
        return null;
      }
      try
      {
        var response = await _httpClient.PostAsJsonAsync(_endpoint, _request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseBody.ToString());
        var responseObject = JsonSerializer.Deserialize<ImageResponse>(responseBody, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        if (!response.IsSuccessStatusCode)
        {
          Console.WriteLine($"Http error: {response.StatusCode}");
          Console.WriteLine($"Details: {responseObject}");
        }

        return responseObject;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
      return null;
    }

    public async Task Generate(GenerationMode gMode, string destDir = "")
    {
      ImageResponse? imageResponse = await MakeRequest();
      if (imageResponse == null)
      {
        return;
      }
      if (gMode == GenerationMode.Jpg)
      {
        Console.WriteLine($"Destination: {destDir}");
        await GenerateToFile(imageResponse, gMode, destDir);
      }
      else if (gMode == GenerationMode.Url)
      {
        GenerateUrl(imageResponse);
      }
    }

    private void GenerateUrl(ImageResponse imageResponse)
    {
      Console.WriteLine($"{_model.ToUpper()}: Created {imageResponse.Created}");
      for (int i = 0; i < imageResponse.Data.Count; i++)
      {
        var dataItem = imageResponse.Data[i];
        if (dataItem.Url != null)
        {
          Console.WriteLine($"Image {i}: {dataItem.Revised_Prompt}");
          Console.WriteLine($"Url: {dataItem.Url}");
        }
        else if (dataItem.Code != null)
        {
          Console.WriteLine($"Image {i}: generation resulted in error: {dataItem.Code}");
          Console.WriteLine($"Error message: {dataItem.Message}");
        }
      }
    }

    private async Task GenerateToFile(ImageResponse imageResponse, GenerationMode gMode, string destDir)
    {
      Console.WriteLine($"Destiantion GenerateToFile: {destDir}");
      if (!ValidateDirectory(destDir))
      {
        return;
      }

      try
      {
        for (int i = 0; i < imageResponse.Data.Count; i++)
        {
          var dataItem = imageResponse.Data[i];
          string filename = $"{_model.ToUpper()}-image-{imageResponse.Created}{GenerationModeDescription.get(gMode)}";
          Console.WriteLine($"Filename: {filename}");
          string path = Path.Join(destDir, filename);
          Console.WriteLine($"Destiantion: {path}");
          if (dataItem.Url != null)
          {

            HttpResponseMessage response = await _httpClient.GetAsync(dataItem.Url);
            response.EnsureSuccessStatusCode();
            byte[] image = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(path, image);
            Console.WriteLine($"Image downloaded and saved successfully in {filename}");

          }
          else if (dataItem.Code != null)
          {
            Console.WriteLine($"Image {i}.  generation resulted in error: {dataItem.Code}");
            Console.WriteLine($"Error message: {dataItem.Message}");
          }

        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error occured: {ex.Message}");
      }
    }

    private bool ValidateDirectory(string destDir)
    {
      Console.WriteLine($"ValidateDirectory Destination 1: {destDir}");
      if (string.IsNullOrWhiteSpace(destDir))
      {
        Console.WriteLine("Validation error: directory name is null or whitespace");
        return false;
      }
      Console.WriteLine($"ValidateDirectory Destination 2: {destDir}");
      char[] invalidChars = Path.GetInvalidPathChars();
      if (destDir.IndexOfAny(invalidChars) >= 0)
      {
        Console.WriteLine("Validation error: directory name contains illegal characters");
        return false;
      }
      Console.WriteLine($"ValidateDirectory Destination 3: {destDir}");
      if (!Directory.Exists(destDir))
      {
        Console.WriteLine("Directory does not exist.");
        try {
          Directory.CreateDirectory(destDir);
        } catch (Exception e) {
          
          Console.WriteLine($"Path Validation exception: {e.Message}");
          throw new Exception(e.Message);
        }
      }
      Console.WriteLine($"ValidateDirectory Destination 4: {destDir}");
      return true;
    }

  }
}