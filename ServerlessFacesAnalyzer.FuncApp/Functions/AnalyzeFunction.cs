using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using ServerlessFacesAnalyzer.Core.Models;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using System.Text.Json;
using System.Text;
using Azure.Storage.Sas;
using ServerlessFacesAnalyzer.FuncApp.Responses;

namespace ServerlessFacesAnalyzer.FuncApp.Functions
{
    public class AnalyzeFunction
    {
        private readonly ILogger<AnalyzeFunction> logger;
        private readonly IFaceAnalyzer faceAnalyzer;
        private readonly IConfiguration configuration;
        private readonly IImageProcessor imageProcessor;
        private readonly BlobServiceClient storageServiceClient;
        private readonly EventGridPublisherClient eventClient;

        public AnalyzeFunction(IFaceAnalyzer faceAnalyzer,
            IConfiguration configuration,
            IImageProcessor imageProcessor,
            IAzureClientFactory<BlobServiceClient> blobClientFactory, 
            IAzureClientFactory<EventGridPublisherClient> eventClientFactory,
            ILogger<AnalyzeFunction> log)
        {
            logger = log;
            this.faceAnalyzer = faceAnalyzer;
            this.imageProcessor = imageProcessor;
            this.configuration = configuration;
            storageServiceClient = blobClientFactory.CreateClient(Constants.BlobClientName);
            eventClient = eventClientFactory.CreateClient(Constants.EventGridClientName);
        }

        [Function(nameof(AnalyzeFaceFromStream))]
        public async Task<IActionResult> AnalyzeFaceFromStream(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "analyze")] HttpRequest req)
        {
            if (!req.IsValid())
                return new BadRequestResult();

            var file = req.Form.Files[0];
            var operationContext = OperationContext.CreateContext(file);

            // Upload original image on storage account
            var destinationContainerName = configuration.GetValue<string>("DestinationContainer");
            var blobContainerClient = storageServiceClient.GetBlobContainerClient(destinationContainerName);
            await file.UploadToStorageAsync(operationContext.BlobName, blobContainerClient);

            // Analyze image
            var faceresult = await file.AnalyzeAsync(faceAnalyzer);

            //Create response DTO
            var response = await CreateResponseAsync(operationContext, faceresult, blobContainerClient, file);

            // Send event using Event Grid Custom Topic
            var @event = new EventGridEvent(
                  subject: operationContext.BlobName,
                  eventType: "ImageAnalyzed",
                  dataVersion: "1.0",
                  data: response);

            await eventClient.SendEventAsync(@event);

            return new OkObjectResult(response);
        }


        private async Task<AnalyzeFaceFromStreamResponse> CreateResponseAsync(OperationContext operationContext,
            FaceAnalyzerResult faceresult, BlobContainerClient destinationContainer, IFormFile file)
        {
            //Create response DTO
            var response = new AnalyzeFaceFromStreamResponse()
            {
                OperationId = operationContext.OperationId,
                OriginalFileName = operationContext.OriginalFileName,
                FileName = operationContext.BlobName,
                AnalysisResult = faceresult,
                FaceBlobs = new List<FaceBlob>()
            };

            var resultBlobName = operationContext.GenerateResultFileName();

            // Serialize response to JSON and generate the stream
            var responseJson = JsonSerializer.Serialize(response);
            using (var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseJson)))
            {
                await destinationContainer.UploadBlobAsync(resultBlobName, responseStream);
            }

            // Elaborate faces
            for (int i = 0; i < faceresult.Faces.Count; i++)
            {
                var face = faceresult.Faces[i];
                var faceBlobName = operationContext.GenerateFaceFileName(i, face);
                var faceBlob = destinationContainer.GetBlobClient(faceBlobName);
                using (var sourceStream = file.OpenReadStream())
                using (var faceBlobStream = faceBlob.OpenWrite(true))
                {
                    // Extract face from original image
                    await imageProcessor.CropImageAsync(sourceStream, face.Rectangle, faceBlobStream);
                }
                var blobUri = faceBlob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now.AddDays(1));
                response.FaceBlobs.Add(new FaceBlob() { 
                     BlobUrl= blobUri.ToString(),
                     FaceId=face.Id
                });
            }

            return response;
        }
    }
}
