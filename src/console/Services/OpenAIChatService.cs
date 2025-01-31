using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.Net.Http.Headers;

namespace AzureOpenAI
{

public class OpenAIResponse
{
    public required Choice[] Choices { get; set; }
    public class Choice
    {
        public required Message Message { get; set; }
    }
    public class Message
    {
        public required string Role { get; set; }
        public required string Content { get; set; }
    }
}

public class OpenAIRequest
{
    public required string Model { get; set; }
    public required double Temperature { get; set; }
    public double Top_p { get; set; }
    public int Max_tokens { get; set; }

    public class Content {
        public required string Text { get; set; }
        public required string Type { get; set; }
    }

    public class Message
    {
        public required string Role { get; set; }
        public required Content[] Content { get; set; }
    }
    public required Message[] Messages { get; set; }
}

public class OpenAIChatService {
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
    private HttpClient _httpClient;

    // private readonly ChatCompletionsClient _client;

    public OpenAIChatService(IConfiguration configuration) {
        _apiKey = configuration["AzureOpenAI:ApiKey"] ?? "";
        _endpoint = configuration["AzureOpenAI:Endpoint"] ?? "";
        _model = configuration["AzureOpenAI:Model"] ?? "";

        _conversationHistory = new List<Tuple<Agent, string>>();

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["Azure:Subscription"]);
    }

    public string GetModel() {
        return _model;
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
        var messages = new List<OpenAIRequest.Message>();

        foreach (var (agent, message) in _conversationHistory) {
            var role = AgentDescription.get(agent);
            var contentObj = new OpenAIRequest.Content{
                        Type = "text",
                        Text = message
            };
            var messageObj = new OpenAIRequest.Message{
                Role = role,
                Content =
                [
                    contentObj
                ]
            };
            messages.Add(messageObj);
        }

        var payload = new OpenAIRequest{
            Model = _model,
            Messages = messages.ToArray(),
            Temperature = 0.7,
            Top_p = 0.95,
            Max_tokens = 100
        };

        var messageFromChat = string.Empty;

        try
        {
//            Console.WriteLine(_endpoint);
//            Console.WriteLine(payload);

            var response = await _httpClient.PostAsJsonAsync(_endpoint, payload);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

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