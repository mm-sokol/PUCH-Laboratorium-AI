using AIDotChat;
using Azure.AI.OpenAI;
using System;

class Program
{
    // static async Task Main(string[] args)
    // {
    //     // Set up configuration
    //     var builder = new ConfigurationBuilder()
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .AddJsonFile("appsettings.json");
    //     var configuration = builder.Build();

    //     // Create OpenAI service
    //     var openAIService = new OpenAIService(configuration);

    //     // Example prompt to send to OpenAI
    //     string prompt = "Hello, can you tell me a joke?";

    //     // Get chat response
    //     string response = await openAIService.GetChatResponseAsync(prompt);

    //     // Output response
    //     Console.WriteLine(response);
    // }


    static void Main(string[] args)
    {
        // Build the configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Read settings from the configuration
        var apiKey = configuration["OpenAI:ApiKey"];
        var endpoint = configuration["OpenAI:Endpoint"];

        // Output the values (for demonstration)
        Console.WriteLine($"API Key: {apiKey}");
        Console.WriteLine($"Endpoint: {endpoint}");
    }

}
