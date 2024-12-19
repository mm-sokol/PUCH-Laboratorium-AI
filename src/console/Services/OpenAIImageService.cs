using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Net.Http.Headers;

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

  class ImageResopnse
  {
    public required int Created { get; init; }
    public required Data[] Data;
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

    public void GetPrompt(string userInput) {
      _request.Prompt = userInput;
    }

    public void PromptForNumber() {
      Console.Write($"{_model.ToUpper()}: How many images?");
      bool invalidInput = true;
      int number = 0;
      while (invalidInput) {
        Console.WriteLine($"{_user}: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out number)) {
          invalidInput = false;
        } else {
          Console.Write($"{_model.ToUpper()}: This is not a valid number. Sorry.");
        }
      }
      _request.N = number;
    }

    public async Task GenerateToFile(string filename)
    {

    }

  }
}