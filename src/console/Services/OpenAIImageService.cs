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
    public required string Prompt;
    public required string Size;
    public required int N;
    public required string Quality;
    public required string Style;
  }

  class Data
  {

  }
  class ImageData : Data
  {
    public required string Url { get; init; }

    public required string RevisedPrompt { get; init; }
  }

  class ErrorData : Data
  {
    public required string Code { get; init; }
    public required string Message { get; init; }
  }

  class ImageResponse
  {
    public required int Created { get; init; }
    public required Data[] Data;
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
      _model = config["AzureOpenAIImage:ImageModel"] ?? "";
      _httpClient = new HttpClient();
      _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
      _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
      _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config["Azure:Subscription"]);

      // default values
      _request = new ImageReguest
      {
        Prompt = "",
        Size = "1024 x 1024",
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
      Console.BackgroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"DALL-E: Choose the size options by navigating 🔽  and 🔼  keys, press \u001b[32mEnter\u001b[0m to select");
      Console.BackgroundColor = ConsoleColor.Black;

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
        2 => "1792 x 1792",
        1 => "1024 x 1792",
        _ => "1024 x 1024"
      };
    }

    public void PromptForQuality()
    {
      // hd, Standard
      Console.BackgroundColor = ConsoleColor.Magenta;
      Console.WriteLine($"DALL-E: Choose the quality options by navigating 🔽  and 🔼  keys, press \u001b[32mEnter\u001b[0m to select");
      Console.BackgroundColor = ConsoleColor.Magenta;

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
      _request.Size = option switch
      {
        1 => "hd",
        _ => "standard"
      };
    }

    public void PromptForStyle()
    {
      // vivid, natural
      Console.BackgroundColor = ConsoleColor.Yellow;
      Console.WriteLine($"DALL-E: Choose the style options by navigating 🔽  and 🔼  keys, press \u001b[32mEnter\u001b[0m to select");
      Console.BackgroundColor = ConsoleColor.Yellow;


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
      _request.Size = option switch
      {
        1 => "natural",
        _ => "vivid"
      };
    }

    public void PromptForPrompt()
    {
      Console.WriteLine($"{_model}: Could you describe you desired image? (press double Enter after you're finished)");
      StringBuilder userInput = new StringBuilder();
      string line = string.Empty;

      do
      {
        Console.WriteLine($"{_user}: ");
        line = Console.ReadLine() ?? "";
        userInput.Append(line);
      } while (!string.IsNullOrWhiteSpace(line));

      _request.Prompt = userInput.ToString();
    }

    public void PromptForNumber()
    {
      Console.Write($"{_model.ToUpper()}: How many images?");
      bool invalidInput = true;
      int number = 0;
      while (invalidInput)
      {

        Console.WriteLine($"{_user}: ");
        string input = Console.ReadLine() ?? "";

        if (int.TryParse(input, out number))
        {
          invalidInput = false;
        }
        else
        {
          Console.Write($"{_model.ToUpper()}: This is not a valid number. Sorry.");
        }
      }

      if (number > 0)
        _request.N = number;
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
        await GenerateToFile(imageResponse, gMode, destDir);
      }
      else if (gMode == GenerationMode.Url)
      {
        GenerateUrl(imageResponse);
      }
    }

    private void GenerateUrl(ImageResponse imageResponse)
    {
      Console.WriteLine($"{_model}: Created {imageResponse.Created}");
      for (int i = 0; i < imageResponse.Data.Length; i++)
      {
        var dataItem = imageResponse.Data[i];
        if (dataItem is ImageData imgData)
        {
          Console.WriteLine($"Image {i}: {imgData.RevisedPrompt}");
          Console.WriteLine($"Url: {imgData.Url}");
        }
        else if (dataItem is ErrorData errorData)
        {
          Console.WriteLine($"Image {i}: generation resulted in error: {errorData.Code}");
          Console.WriteLine($"Error message: {errorData.Message}");
        }
      }
    }

    private async Task GenerateToFile(ImageResponse imageResponse, GenerationMode gMode, string destDir)
    {
      if (!ValidateDirectory(destDir))
      {
        return;
      }

      try
      {
        for (int i = 0; i < imageResponse.Data.Length; i++)
        {
          var dataItem = imageResponse.Data[i];
          string filename = $"{_model}-image-{imageResponse.Created}-{i}{GenerationModeDescription.get(gMode)}";
          string path = Path.Join(destDir, filename);
          if (dataItem is ImageData imageData)
          {

            HttpResponseMessage response = await _httpClient.GetAsync(imageData.Url);
            response.EnsureSuccessStatusCode();
            byte[] image = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(path, image);
            Console.WriteLine($"Image downloaded and saved successfully in {filename}");

          }
          else if (dataItem is ErrorData errorData)
          {
            Console.WriteLine($"Image {i}.  generation resulted in error: {errorData.Code}");
            Console.WriteLine($"Error message: {errorData.Message}");
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
      if (string.IsNullOrWhiteSpace(destDir))
      {
        Console.WriteLine("Validation error: directory name is null or whitespace");
        return false;
      }
      if (Path.GetInvalidPathChars().Any(x => destDir.Contains(x)))
      {
        Console.WriteLine("Validation error: directory name contains illegal characters");
        return false;
      }
      if (!Directory.Exists(destDir))
      {
        Directory.CreateDirectory(destDir);
      }
      return true;
    }

  }
}