using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;
using ServerlessFacesAnalyzer.Functions.Responses;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class ExtractFaceFromImageActivity
    {
        private readonly ILogger<ExtractFaceFromImageActivity> logger;
        private readonly BlobServiceClient storageServiceClient;
        private readonly IImageProcessor imageProcessor;
        private readonly IConfiguration configuration;

        public ExtractFaceFromImageActivity(IImageProcessor imageProcessor,
            IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory,
            ILogger<ExtractFaceFromImageActivity> log)
        {
            logger = log;
            this.imageProcessor = imageProcessor;
            this.configuration = configuration;
            this.storageServiceClient = blobClientFactory.CreateClient(Constants.BlobClientName);
        }

        [Function(nameof(ExtractFaceFromImageActivity))]
        public async Task<FaceBlob> Run([ActivityTrigger] ExtractFaceFromImageDto context)
        {
            var faceBlobName = context.OperationContext.GenerateFaceFileName(context.FaceIndex, context.Face);

            var destinationContainerName = configuration.GetValue<string>("DestinationContainer");
            var blobContainerClient = storageServiceClient.GetBlobContainerClient(destinationContainerName);
            var imageBlobClient = blobContainerClient.GetBlobClient(context.OperationContext.BlobName);
            var faceBlobClient = blobContainerClient.GetBlobClient(faceBlobName);

            using (var sourceStream = imageBlobClient.OpenRead())
            using (var faceStream = faceBlobClient.OpenWrite(true))
            {
                await imageProcessor.CropImageAsync(sourceStream, context.Face.Rectangle, faceStream);
            }

            var blobUri = faceBlobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddDays(1));

            return new FaceBlob()
            {
                FaceId = context.Face.Id,
                BlobUrl = blobUri.ToString()
            };
        }
    }
}
