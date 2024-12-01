using AIDotChat;
using Azure.AI.OpenAI;
using System;
using System.Net.Http;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up configuration
        var app = new Application();

        Console.WindowWidth = 100;

        Console.WriteLine(app.GetGreetings());

        await app.run();
    }

}
