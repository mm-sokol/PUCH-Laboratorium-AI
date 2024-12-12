using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;

// using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
// using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;

namespace AzureCVision
{
    class AzureCVService
    {
        private readonly string _filePredictionKey;
        private readonly string _filePredictionEndpoint;

        private readonly string _urlPredictionKey;
        private readonly string _urlPredictionEndpoint;

        private readonly string _projectId;
        private readonly string _resourceId;
        private string _publishedName;

        //        private string _testImageDirectory;
        //        private string _predictionImageDirectory;

        private enum Mode
        {
            File,
            Url
        };

        public AzureCVService(IConfiguration configuration)
        {
            _resourceId = configuration["AzureCustomVision:ResourceId"] ?? "";
            _projectId = configuration["AzureCustomVision:ProjectId"] ?? "";

            _publishedName = configuration["AzureCustomVision:Prediction:PublishedName"] ?? "";

            _filePredictionKey = configuration["AzureCustomVision:Prediction:file:ApiKey"] ?? "";
            _filePredictionEndpoint = configuration["AzureCustomVision:Prediction:file:Endpoint"] ?? "";

            _urlPredictionKey = configuration["AzureCustomVision:Prediction:url:ApiKey"] ?? "";
            _urlPredictionEndpoint = configuration["AzureCustomVision:Prediction:url:Endpoint"] ?? "";


            // Console.WriteLine($"Key: {_filePredictionKey}");
            // Console.WriteLine($"Endpoint: {_filePredictionEndpoint}");
        }


        //        public async Task<Dictionary<string, double>> PredictMany(string imageDirectory) {
        //
        //
        //
        //        }



        public async Task<ImagePrediction> PredictOneFile(string imageFile)
        {
            if (!File.Exists(imageFile))
            {
                throw new ArgumentException($"Path {imageFile} if not valid.");
            }
            var client = getClient(Mode.File);
            if (client == null)
            {
                throw new Exception("No client for Azure Custom Vision.");
            }
            Console.WriteLine($"Client type: {client.GetType()}");

            using (var imageStream = new FileStream(imageFile, FileMode.Open))
            {
                Console.WriteLine("here");
                var prediction = await client.ClassifyImageAsync(
                    new Guid(this._projectId),
                    this._publishedName,
                    imageStream
                );
                Console.WriteLine("also here");
                if (prediction == null)
                {
                    throw new Exception("Error in PredictOneFile: Prediction is null");
                }

                return prediction;
            }
        }

        public async Task<ImagePrediction> PredictOneUrl(string url)
        {

            await IsValidImageUrlAsync(url);
            var client = getClient(Mode.Url);
            var prediction = await client.ClassifyImageUrlAsync(
                new Guid(this._projectId),
                this._publishedName,
                new ImageUrl(url)
            );
            return prediction;
        }

        static private async Task IsValidImageUrlAsync(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException($"Url provided is ill-formed: {url}");
            }
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    // Send a HEAD request to avoid downloading the full content
                    HttpResponseMessage response = await httpClient.SendAsync(
                        new HttpRequestMessage(HttpMethod.Head, url));

                    // Check if the response is successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Check if the content type is an image
                        if (response.Content == null) {
                            throw new Exception("no content recieved while testing");
                        } 
                        string contentType = response.Content.Headers.ContentType?.MediaType ?? "";

                        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Url does not point to image");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Url validation exception occured: {ex.Message}");
            }
        }

        private CustomVisionPredictionClient getClient(Mode mode)
        {
            switch (mode)
            {
                case Mode.File:
                    return new CustomVisionPredictionClient(new
                    Microsoft.Azure.CognitiveServices.
                    Vision.CustomVision.Prediction.
                    ApiKeyServiceClientCredentials(this._filePredictionKey))
                    {
                        Endpoint = this._filePredictionEndpoint
                    };
                case Mode.Url:
                    return new CustomVisionPredictionClient(new
                    Microsoft.Azure.CognitiveServices.
                    Vision.CustomVision.Prediction.
                    ApiKeyServiceClientCredentials(this._urlPredictionKey))
                    {
                        Endpoint = this._urlPredictionEndpoint
                    };
                default:
                    throw new ArgumentException($"Mode {mode} is not recognised");
            }
        }

        public static async Task TestOne()
        {
            try
            {// your code here
                var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");

                var configuration = builder.Build();

                var service = new AzureCVService(configuration);

                Console.WriteLine("Simple test for prediction");
                var prediction = await service.PredictOneFile("../../resources/split/train/dew/2208.jpg");

                Console.WriteLine("--- Predicting Weather ---");
                foreach (var label in prediction.Predictions)
                {
                    Console.WriteLine($"- Probability for {label.TagName}: {label.Probability * 100}%");
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Argument null exception: " + ex.Message);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("HTTP request exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General exception: " + ex.Message);
            }
        }
    }
}