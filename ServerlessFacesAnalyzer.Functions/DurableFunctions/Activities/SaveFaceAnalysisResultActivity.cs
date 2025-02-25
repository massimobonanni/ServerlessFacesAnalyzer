using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class SaveFaceAnalysisResultActivity
    {
        private readonly ILogger<SaveFaceAnalysisResultActivity> logger;
        private readonly BlobServiceClient storageServiceClient;
        private readonly IConfiguration configuration;

        public SaveFaceAnalysisResultActivity(IConfiguration configuration,
             IAzureClientFactory<BlobServiceClient> blobClientFactory, 
             ILogger<SaveFaceAnalysisResultActivity> log)
        {
            logger = log;
            this.configuration = configuration;
            this.storageServiceClient = blobClientFactory.CreateClient(Constants.BlobClientName);
        }

        [Function(nameof(SaveFaceAnalysisResultActivity))]
        public async Task Run([ActivityTrigger] SaveFaceAnalysisResultDto context)
        {
            var resultBlobName = context.OperationContext.GenerateResultFileName();

            var destinationContainerName = configuration.GetValue<string>("DestinationContainer");
            var blobContainerClient = storageServiceClient.GetBlobContainerClient(destinationContainerName);
            var blobClient = blobContainerClient.GetBlobClient(resultBlobName);

            await blobClient.UploadAsync(JsonConvert.SerializeObject(context.FaceResultResponse, Formatting.Indented));
        }
    }
}
