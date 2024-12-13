using AIDotChat;
using AzureCVision;
using Microsoft.Extensions.Configuration;
using Sprache;
using System.Text.RegularExpressions;

namespace AIDotChat
{
    class Application
    {
        private string _username = "User";
        private string _assistant = "GPT-4";
        private OpenAIService _service;
        private AzureCVService _visionService;
        private OpenAIPdfService _summaryService;

        public Application()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            // Create OpenAI service
            _service = new OpenAIService(configuration);
            // Create Azure Custom Vision service
            _visionService = new AzureCVService(configuration);
            // Create service for summaries
            _summaryService = new OpenAIPdfService(configuration);
        }

        public string GetGreetings()
        {
            var model = _service.GetModel();
            var greetings = string.Empty;
            greetings += ":--------------------------------------------------------:\n";
            greetings += $"               D O T  C H A T  {model}      \n";
            greetings += ":--------------------------------------------------------:\n";
            greetings += " Here are some usefull commands:\n";
            greetings += " \\user <username> - to register your username\n";
            greetings += " \\system <text> - to provide context for the AI assistant\n";
            greetings += " \\save <filename> - to save your chat history in a file\n";
            greetings += " \\clear - to clear the chat history\n";
            greetings += " \\exit - for leaving the chat\n\n";

            greetings += " \\vision [options] - predicts weather from given image with Azure Custom Vision\n";
            greetings += " \\vision img \"<path to img>\"\n";
            greetings += " \\vision url \"<url with img>\"\n\n";

            greetings += " \\summarize [options] - creates summaries of pdf files with OpenAI\n";
            greetings += " \\summarize pdf \"<in filename>\" to \"<out filename>\"\n";
            greetings += " \\summarize dir \"<source path>\" to \"dest path>\"\n";
            greetings += " \\summarize ... -v|--verbose - outputs summary to screen\n";
            greetings += " ...\n";
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

        private bool ValidateVisionCommand(string userInput, out Mode mode, out string imgSource)
        {
            imgSource = string.Empty;
            mode = Mode.None;
            string pattern = @"^\\vision\s(img|url)\s\""(.*\.(jpg|jpeg|png|gif|bmp)|(https?|ftp):\/\/([^\s\/$.?#].[^\s]*))\""$";
            Match match = Regex.Match(userInput, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (match.Groups[1].Value == "img")
                    mode = Mode.File;
                else if (match.Groups[1].Value == "url")
                    mode = Mode.Url;
                imgSource = match.Groups[2].Value;

                Console.WriteLine($"Requested image classification from {ModeDescription.get(mode)}: {imgSource}");
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidateSummaryCommand(string userInput, out SummaryMode mode, out string pdfSource, out string pdfDest, out bool verbose) {
            pdfSource = string.Empty;
            pdfDest = string.Empty;
            mode = SummaryMode.None;
            verbose = false;

            string pattern = @"^\\summarize\s+(pdf|dir)\s+(-v|\--verbose)?\s*""([^""]+)""\s+to\s+""([^""]+)""\s*(-v|\--verbose)?$";
            Match match = Regex.Match(userInput, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (match.Groups[1].Value == "pdf")
                    mode = SummaryMode.File;
                else if (match.Groups[1].Value == "dir")
                    mode = SummaryMode.Folder;
                else
                    return false;

                pdfSource = match.Groups[3].Value;
                Console.WriteLine($"Source {pdfSource}");
                pdfDest = match.Groups[4].Value;
                Console.WriteLine($"Dest {pdfDest}");

                if (match.Groups[2].Value != null || match.Groups[5].Value != null)
                    verbose = true;


                Console.WriteLine($"Requested pdf summary from {SummaryModeDescription.get(mode)}: {pdfSource} to {pdfDest}");
                return true;
            }
            else
            {
                return false;
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
                        case "\\vision":
                            try
                            {
                                if (words.Length < 3)
                                {
                                    Console.WriteLine("Not enough arguments provided.");
                                    break;
                                }
                                if (ValidateVisionCommand(userInput, out Mode vMode, out string imgSource))
                                {
                                    try
                                    {
                                        var prediction = await _visionService.PredictOne(imgSource, vMode);

                                        Console.WriteLine(":------------------ Predicting weather ------------------:");
                                        foreach (var label in prediction.Predictions)
                                        {
                                            Console.WriteLine($"- {label.TagName}: {label.Probability * 100:F2} %");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Command was invalid.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        case "\\summarize":
                            if (words.Length < 5) {
                                Console.WriteLine("Not enough arguments provided");
                                break;
                            }
                            if (! ValidateSummaryCommand(userInput, out SummaryMode sMode, out string fileSource, out string fileDest, out bool verbose)) {
                                Console.WriteLine("Command validation failed");
                                break;
                            }
                            try {
                               await _summaryService.Summarize(fileSource, fileDest, sMode, verbose);
                            } catch (Exception ex) {
                                Console.WriteLine($"Error occured: {ex.Message}");
                            }
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