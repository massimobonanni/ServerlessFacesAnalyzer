using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using ServerlessFacesAnalyzer.Functions.Responses;
using Microsoft.OpenApi.Models;
using ServerlessFacesAnalyzer.Functions.Requestes;
using System.Net;
using System.Linq;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Orchestrators;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Clients
{
    public class ImageUploadClient
    {
        private readonly ILogger<ImageUploadClient> logger;
        private readonly IConfiguration configuration;

        public ImageUploadClient(IConfiguration configuration,
            ILogger<ImageUploadClient> log)
        {
            logger = log;
            this.configuration = configuration;
        }

        [FunctionName("ImageUploadClient")]
        [OpenApiOperation(operationId: "AnalyzeFaceFromStreamDurable", Description = "This API allows you to start the orchestration to analyze an image.")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody("multipart/form-data", typeof(AnalyzeFaceFromStreamRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AnalyzeFaceFromStreamResponse), Description = "The result of the analisis on the uploaded image")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "There aren't any files sent with the request")]

        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "durable/analyze")] HttpRequest req,
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer destinationContainer,
            [DurableClient] IDurableClient client)
        {
            if (!req.IsValid())
                return new BadRequestResult();
            
            var file = req.Form.Files[0];
            var operationContext = OperationContext.CreateContext(file);

            await file.UploadToStorageAsync(operationContext.BlobName, destinationContainer);

            await client.StartNewAsync(nameof(ImageAnalizerOrchestrator), 
                operationContext.OperationId, operationContext);

            var result=await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, 
                operationContext.OperationId, TimeSpan.FromSeconds(5));

            return result;
        }
    }
}
