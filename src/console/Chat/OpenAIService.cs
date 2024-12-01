using Azure.AI.OpenAI;
using Azure.Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace AIDotChat;

public class OpenAIService {

    private readonly string _apiKey;
    private readonly string _endpoint;

    public OpenAIService(IConfiguration configuration) {
        _apiKey = configuration["OpenAI:ApiKey"];
        _endpoint = configuration["OpenAI:Endpoint"];
    }

    public async Task<string> getChatResponseAsync(string prompt) {
        var client = new OpenAIClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
        var result = await client.GetCompletionsAsync("gpt-3.5-turbo", new CompletionsOptions { Prompt = prompt, MaxTokens = 100 });
        return result.Value.Choices[0].Text;
    }

}