using AIDotChat;
using AzureCVision;
using Microsoft.Extensions.Configuration;

namespace AIDotChat
{
    class Application
    {
        private string _username = "User";
        private string _assistant = "GPT-4";
        private OpenAIService _service;

        public Application()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            // Create OpenAI service
            _service = new OpenAIService(configuration);
        }

        public string GetGreetings()
        {
            var model = _service.GetModel();
            var greetings = string.Empty;
            greetings += ":--------------------------------------------------------:\n";
            greetings += $"               D O T  C H A T  {model}      \n";
            greetings += ":--------------------------------------------------------:\n";
            greetings += " Here are some usefull commands:\n ";
            greetings += " \\user <username> - to register your username\n";
            greetings += " \\system <text> - to provide context for the AI assistant\n ";
            greetings += " \\save <filename> - to save your chat history in a file\n ";
            greetings += " \\clear - to clear the chat history\n ";
            greetings += " \\exit - for leaving the chat\n ";
            greetings += " ...\n ";
            return greetings;
        }

        private void WriteToFile(string filename, string text)
        {
            try
            {
                // Open or create the file (overwrites if the file exists)
                using (StreamWriter writer = new StreamWriter(filename, append: false))
                {
                    writer.WriteLine(text);
                }
                Console.WriteLine("Content written to the file successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task run()
        {
            while (true)
            {
                Console.Write(this._username + ": ");
                string userInput = Console.ReadLine() ?? string.Empty;
                string[] words = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 0)
                {
                    string command = words[0].ToLower();  // Case-insensitive check
                    switch (command)
                    {
                        case "\\exit":
                            Console.WriteLine("Exiting the chat.");
                            return; // Exit the program
                        case "\\user":
                            Console.WriteLine("Setting username");
                            if (words.Length > 1)
                            {
                                this._username = words[1];
                            }
                            else
                            {
                                Console.WriteLine("No username provided after the \\user command.");
                            }
                            break;
                        case "\\save":
                            Console.WriteLine("Saving your data...");
                            if (words.Length > 1)
                            {
                                var filename = words[1];
                                WriteToFile(filename, _service.GetConversationHistory());
                            }
                            else
                            {
                                Console.WriteLine("No filename provided after the \\save command.");
                            }
                            break;
                        case "\\system":
                            Console.WriteLine("Providing context to AI assistant.");
                            if (words.Length > 1)
                            {
                                // Get the text after the command (\system)
                                string systemMessage = string.Join(" ", words, 1, words.Length - 1);
                                _service.AddContext(systemMessage);
                            }
                            else
                            {
                                Console.WriteLine("No message provided after the \\system command.");
                            }
                            break;
                        case "\\clear":
                            Console.WriteLine("Clearing conversation history.");
                            _service.ClearHistory();
                            break;
                        case "\\test":
                            Console.WriteLine("Testing custom vision.");
                            await AzureCVService.TestOne();
                            break;
                        default:
                            // Console.WriteLine("We are in the default");
                            var response = await _service.GetChatResponseAsync(userInput);
                            Console.WriteLine(this._assistant + ": " + response);
                            break;
                    }
                }
            }
        }
    }
}