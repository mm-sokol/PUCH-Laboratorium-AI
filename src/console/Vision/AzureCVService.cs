using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

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

        private enum Mode {
            File,
            Url
        };

        public AzureCVService(IConfiguration configuration) {
            _resourceId = configuration["AzureCustomVision:ResourceId"] ?? "";
            _projectId = configuration["AzureCustomVision:ProjectId"] ?? "";

            _publishedName = configuration["AzureCustomVision:Prediction:PublishedName"] ?? "";

            _filePredictionKey = configuration["AzureCustomVision:Prediction:file:ApiKey"] ?? "";
            _filePredictionEndpoint = configuration["AzureCustomVision:Prediction:file:Endpoint"] ?? "";

            _urlPredictionKey = configuration["AzureCustomVision:Prediction:url:ApiKey"] ?? "";
            _urlPredictionEndpoint = configuration["AzureCustomVision:Prediction:url:Endpoint"] ?? "";
        }


//        public async Task<Dictionary<string, double>> PredictMany(string imageDirectory) {
//
//
//
//        }



        public async Task<ImagePrediction> PredictOneFile(string imageFile) {

            var client = getClient(Mode.File);

            using (var imageStream = new FileStream(imageFile, FileMode.Open)) {
                var prediction = await client.ClassifyImageAsync(
                    new Guid(this._projectId),
                    this._publishedName,
                    imageStream
                );
                return prediction;
            }
        }

        public async Task<ImagePrediction> PredictOneUrl(string url) {

            var client = getClient(Mode.Url);

            var prediction = await client.ClassifyImageAsync(
                new Guid(this._projectId),
                this._publishedName,
                url
            );
            return prediction;
        }

        private CustomVisionPredictionClient getClient(Mode mode) {
            switch (mode) {
                case Mode.File:
                    return new CustomVisionPredictionClient() {
                                           Endpoint = this._filePredictionEndpoint,
                                           ApiKey = this._filePredictionKey
                                       };
                case Mode.Url:
                    return new CustomVisionPredictionClient() {
                                           Endpoint = this._filePredictionEndpoint,
                                           ApiKey = this._filePredictionKey
                                       };
                default:
                    throw new ArgumentException($"Mode {mode} is not recognised");
            }
        }

        public static Task TestOne(string[] args)
        {
            // your code here
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            var service = new AzureCVService(configuration);

            Console.WriteLine("Simple test for prediction");
            var prediction = await service.PredictOneFile("../../resources/split/train/dew/2208.jpg");

            Console.WriteLine("--- Predicting Weather ---");
            foreach(var label in prediction.Predictions) {
                Console.WriteLine($"- Probability for {label.TagName}: {label.Probability * 100}%");
            }
        }
    }
}