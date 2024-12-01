using Azure.AI.OpenAI;
using Azure.Core;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;

using System.ClientModel;
using System.Reflection.Metadata;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AIDotChat
{

public class OpenAIResponse
{
    public Choice[] Choices { get; set; }
    public class Choice
    {
        public Message Message { get; set; }
    }
    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}

public class OpenAIService {

//    private readonly string _apiKey = GetEnvironmentVariable("AZURE_OPENAI_KEY");
//    private readonly string _endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
//    private readonly string _model = GetEnvironmentVariable("AZURE_OPENAI_MODEL");

    private enum Agent {
        User, Assistant, System
    }
    private static class AgentDescription {
        public static string get(Agent agent) {
            return agent switch {
                Agent.User => "user",
                Agent.Assistant => "assistant",
                Agent.System => "system",
                _ => "unknown"
            };
        }
    }

    private readonly string _apiKey;
    private readonly string _endpoint;
    private string _model {get; set;}
    private List<Tuple<Agent, string>> _conversationHistory;
//    private ChatCompletionsClient _client;
    private HttpClient _httpClient;

    public OpenAIService(IConfiguration configuration) {
        _apiKey = configuration["OpenAI:ApiKey"];
        _endpoint = configuration["OpenAI:Endpoint"];
        _model = configuration["OpenAI:Model"];

        _conversationHistory = new List<Tuple<Agent, string>>();

//        _client = new ChatCompletionsClient(
//            new Uri(_endpoint),
//            new AzureKeyCredential(System.Environment.GetEnvironmentVariable(_apiKey)),
//            new ChatCompletionsClientOptions());

//        _client = new OpenAIClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));

        _httpClient = new HttpClient();
//        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["Azure:Subscription"]);
        Console.WriteLine($"Bearer {_apiKey}");

    }

    public string GetModel() {
        return this._model;
    }

    private void AddMessage(Agent agent, string message) {
        _conversationHistory.Add(new Tuple<Agent, string>(agent, message));
    }

    public void AddContext(string message) {
        _conversationHistory.Add(new Tuple<Agent, string>(Agent.System, message));
    }

    public void ClearHistory() {
        _conversationHistory.Clear();
    }

    public string GetConversationHistory() {
        var history = string.Empty;
        foreach (Tuple<Agent, string> entry in _conversationHistory) {
            history += $"{AgentDescription.get(entry.Item1).ToUpper()}: {entry.Item2}\n";
        }
        return history;
    }

    public async Task<string> GetChatResponseAsync(string prompt) {

        AddMessage(Agent.User, prompt);
        var messages = new List<object>();
        foreach (var (agent, message) in _conversationHistory) {
            var role = AgentDescription.get(agent);
            var messageObj = new {
                role,
                content = new[]
                {
                    new {
                        type = "text",
                        text = message
                    }
                }
            };
            messages.Add(messageObj);
        }

        var payload = new {
            model = _model,
            messages = messages,
            temperature = 0.7,
            top_p = 0.95,
            max_tokens = 100
        };

        var messageFromChat = string.Empty;

        try
        {
//            Console.WriteLine(_endpoint);
//            Console.WriteLine(payload);

            // Send POST request
            var response = await _httpClient.PostAsJsonAsync(_endpoint, payload);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Parse response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                // Print the response content
                if (responseObject?.Choices != null && responseObject.Choices.Length > 0)
                {
                    AddMessage(Agent.Assistant, responseObject.Choices[0].Message.Content);
                    messageFromChat += responseObject.Choices[0].Message.Content;
                }
                else
                {
                    Console.WriteLine("No response from OpenAI.");
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Details: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return messageFromChat;
    }
}
}