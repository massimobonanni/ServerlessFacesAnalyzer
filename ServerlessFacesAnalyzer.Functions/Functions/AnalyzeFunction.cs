using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using System.Linq;
using System;
using ServerlessFacesAnalyzer.Functions.Responses;
using ServerlessFacesAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using ServerlessFacesAnalyzer.Core.Models;
using ServerlessFacesAnalyzer.Functions.Requestes;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Azure.Messaging.EventGrid;

namespace ServerlessFacesAnalyzer.Functions.Functions
{
    public class AnalyzeFunction
    {
        private readonly ILogger<AnalyzeFunction> logger;
        private readonly IFaceAnalyzer faceAnalyzer;
        private readonly IConfiguration configuration;
        private readonly IImageProcessor imageProcessor;

        public AnalyzeFunction(IFaceAnalyzer faceAnalyzer,
            IConfiguration configuration,
            IImageProcessor imageProcessor,
            ILogger<AnalyzeFunction> log)
        {
            logger = log;
            this.faceAnalyzer = faceAnalyzer;
            this.imageProcessor = imageProcessor;
            this.configuration = configuration;
        }

        [FunctionName("AnalyzeFaceFromStream")]
        [OpenApiOperation(operationId: "AnalyzeFaceFromStream", Description = "This API allows you to upload an image to analize.")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("multipart/form-data", typeof(AnalyzeFaceFromStreamRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AnalyzeFaceFromStreamResponse), Description = "The result of the analisis on the uploaded image")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "There aren't any files sent with the request")]
        public async Task<IActionResult> AnalyzeFaceFromStream(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "analyze")] HttpRequest req,
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer destinationContainer,
            [EventGrid(TopicEndpointUri = "TopicEndpoint", TopicKeySetting = "TopicKey")] IAsyncCollector<EventGridEvent> eventCollector)
        {
            if (!req.IsValid())
                return new BadRequestResult();

            var file = req.Form.Files[0];
            var operationContext = OperationContext.CreateContext(file);

            // Upload original image on storage account
            await file.UploadToStorageAsync(operationContext.BlobName, destinationContainer);
            // Analyze image
            var faceresult = await file.AnalyzeAsync(faceAnalyzer);

            var response = new AnalyzeFaceFromStreamResponse()
            {
                OperationId = operationContext.OperationId,
                OriginalFileName = operationContext.OriginalFileName,
                FileName = operationContext.BlobName,
                AnalysisResult = faceresult
            };

            var resultBlobName = operationContext.GenerateResultFileName();
            await destinationContainer.SerializeObjectToBlobAsync(resultBlobName, response);

            // Elaborate faces
            for (int i = 0; i < faceresult.Faces.Count; i++)
            {
                var face = faceresult.Faces[i];
                var faceBlobName = operationContext.GenerateFaceFileName(i);
                var faceBlob = destinationContainer.GetBlockBlobReference(faceBlobName);
                using (var sourceStream = file.OpenReadStream())
                using (var faceBlobStream = faceBlob.OpenWrite())
                {
                    // Extract face from original image
                    await imageProcessor.CropImageAsync(sourceStream, face.Rectangle, faceBlobStream);
                }
            }

            // Send event using Event Grid Custom Topic
            var @event = new EventGridEvent(
                  subject: operationContext.BlobName,
                  eventType: "ImageAnalyzed",
                  dataVersion: "1.0",
                  data: response);

            await eventCollector.AddAsync(@event);

            return new OkObjectResult(response);
        }


    }
}
