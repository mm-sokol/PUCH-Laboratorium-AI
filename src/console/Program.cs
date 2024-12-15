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

        var origWidth = Console.WindowWidth;
        var origHeight = Console.WindowHeight;
        Console.WindowWidth = 100;

        Console.WriteLine(app.GetGreetings());

        await app.run();


        Console.SetWindowSize(origWidth, origHeight);
    }

}
