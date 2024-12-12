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
                else
                {
                    Console.WriteLine(prediction);
                }
                return prediction;
            }
        }

        public async Task<ImagePrediction> PredictOneUrl(string url)
        {

            var client = getClient(Mode.Url);

            using (Stream stream = await GetImageStreamFromUrlAsync(url))
            {
                var prediction = client.ClassifyImage(
                    new Guid(this._projectId),
                    this._publishedName,
                    stream
                );
                return prediction;
            }
        }

        static private async Task<Stream> GetImageStreamFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
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