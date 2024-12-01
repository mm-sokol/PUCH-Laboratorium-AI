import os
import requests
import base64

# Configuration
API_KEY = "YOUR_API_KEY"
IMAGE_PATH = "YOUR_IMAGE_PATH"
encoded_image = base64.b64encode(open(IMAGE_PATH, 'rb').read()).decode('ascii')
headers = {
    "Content-Type": "application/json",
    "api-key": API_KEY,
}

# Payload for the request
payload = {
  "messages": [
    {
      "role": "system",
      "content": [
        {
          "type": "text",
          "text": "You are a software developer, knowledgeable in your craft."
        }
      ]
    },
    {
      "role": "user",
      "content": [
        {
          "type": "text",
          "text": "I'd like to send a request in C# to azure cognitiveservices. How should the payload look like?"
        }
      ]
    },
    {
      "role": "assistant",
      "content": [
        {
          "type": "text",
          "text": "To send a request to Azure Cognitive Services, you first need to determine which specific service and API you want to use as Azure Cognitive Services encompasses several APIs such as Computer Vision, Text Analytics, Face API, etc. Each of these services may require different payload structures depending on the endpoint.\n\nFor demonstration purposes, let's consider a common use case: sending an image to the Computer Vision API to analyze visual features. Below is an example of how you can structure your C# code and payload for this purpose.\n\n### Prerequisites\n- An Azure subscription\n- A Cognitive Services resource created in your Azure portal\n- The endpoint URL and an access key from your Cognitive Services resource\n\n### Step 1: Install the necessary NuGet package\nFor sending HTTP requests, you can use `HttpClient`. Ensure you have the `System.Net.Http` namespace available, or install it via NuGet if necessary.\n\n### Step 2: Constructing the HTTP request\n\nHere's a basic example of how you might set up your C# code:\n\n```csharp\nusing System;\nusing System.IO;\nusing System.Net.Http;\nusing System.Net.Http.Headers;\nusing System.Threading.Tasks;\n\nclass Program\n{\n    static async Task Main(string[] args)\n    {\n        string subscriptionKey = \"your_subscription_key_here\";\n        string endpoint = \"https://your_resource_name.cognitiveservices.azure.com/\";\n        string uriBase = endpoint + \"vision/v3.2/analyze\";\n\n        string imageFilePath = @\"path_to_your_image.jpg\";\n        string visualFeatures = \"Categories,Description,Color\";\n\n        HttpClient client = new HttpClient();\n\n        // Request headers.\n        client.DefaultRequestHeaders.Add(\"Ocp-Apim-Subscription-Key\", subscriptionKey);\n\n        // Assemble the URI for the REST API method.\n        string uri = $\"{uriBase}?visualFeatures={visualFeatures}\";\n\n        HttpResponseMessage response;\n        // Read the contents of the specified local image into a byte array.\n        byte[] byteData = GetImageAsByteArray(imageFilePath);\n\n        using (ByteArrayContent content = new ByteArrayContent(byteData))\n        {\n            // Set the content type to \"application/octet-stream\".\n            content.Headers.ContentType = new MediaTypeHeaderValue(\"application/octet-stream\");\n\n            // Asynchronously call the REST API method.\n            response = await client.PostAsync(uri, content);\n        }\n\n        // Asynchronously get the JSON response.\n        string contentString = await response.Content.ReadAsStringAsync();\n        Console.WriteLine(\"\\nResponse:\\n\");\n        Console.WriteLine(JsonPrettyPrint(contentString));\n    }\n\n    static byte[] GetImageAsByteArray(string imageFilePath)\n    {\n        // Read the file as a byte array.\n        return File.ReadAllBytes(imageFilePath);\n    }\n\n    static string JsonPrettyPrint(string json)\n    {\n        if (string.IsNullOrEmpty(json))\n            return string.Empty;\n\n        json = json.Replace(Environment.NewLine, \"\").Replace(\"\\t\", \"\");\n\n        string INDENT_STRING = \"    \";\n        var indent = 0;\n        var quoted = false;\n        var sb = new System.Text.StringBuilder();\n        for (var i = 0; i < json.Length; i++)\n        {\n            var ch = json[i];\n            switch (ch)\n            {\n                case '{':\n                case '[':\n                    sb.Append(ch);\n                    if (!quoted)\n                    {\n                        sb.AppendLine();\n                        Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));\n                    }\n                    break;\n                case '}':\n                case ']':\n                    if (!quoted)\n                    {\n                        sb.AppendLine();\n                        Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));\n                    }\n                    sb.Append(ch);\n                    break;\n                case '\"':\n                    sb.Append(ch);\n                    bool escaped = false;\n                    var index = i;\n                    while (index > 0 && json[--index] == '\\\\')\n                        escaped = !escaped;\n                    if (!escaped)\n                        quoted = !quoted;\n                    break;\n                case ',':\n                   "
        }
      ]
    }
  ],
  "temperature": 0.7,
  "top_p": 0.95,
  "max_tokens": 800
}

ENDPOINT = "https://01133-m45kcji6-swedencentral.openai.azure.com/openai/deployments/gpt-4-deployment-1/chat/completions?api-version=2024-02-15-preview"

# Send request
try:
    response = requests.post(ENDPOINT, headers=headers, json=payload)
    response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
except requests.RequestException as e:
    raise SystemExit(f"Failed to make the request. Error: {e}")

# Handle the response as needed (e.g., print or process)
print(response.json())