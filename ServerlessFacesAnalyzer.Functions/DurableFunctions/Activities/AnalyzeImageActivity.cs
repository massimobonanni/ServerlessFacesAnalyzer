using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class AnalyzeImageActivity
    {
        private readonly ILogger<AnalyzeImageActivity> logger;
        private readonly BlobServiceClient storageServiceClient;
        private readonly IFaceAnalyzer faceAnalyzer;
        private readonly IConfiguration configuration;

        public AnalyzeImageActivity(IFaceAnalyzer faceAnalyzer,
            IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory,
            ILogger<AnalyzeImageActivity> log)
        {
            logger = log;
            this.configuration = configuration;
            this.faceAnalyzer = faceAnalyzer;
            this.storageServiceClient = blobClientFactory.CreateClient(Constants.BlobClientName);
        }

        [Function(nameof(AnalyzeImageActivity))]
        public async Task<FaceAnalyzerResult> Run([ActivityTrigger] OperationContext context)
        {
            var destinationContainerName = configuration.GetValue<string>("DestinationContainer");
            var blobContainerClient = storageServiceClient.GetBlobContainerClient(destinationContainerName);
            var blobClient = blobContainerClient.GetBlobClient(context.BlobName);

            FaceAnalyzerResult result;
            using (var stream = await blobClient.OpenReadAsync())
            {
                result = await this.faceAnalyzer.AnalyzeAsync(stream);
            }

            return result;
        }
    }
}
