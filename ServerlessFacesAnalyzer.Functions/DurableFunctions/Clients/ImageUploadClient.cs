using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Orchestrators;
using ServerlessFacesAnalyzer.Functions.Responses;
using System.Text.Json;


namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Clients
{
    public class ImageUploadClient
    {
        private readonly ILogger<ImageUploadClient> logger;
        private readonly IConfiguration configuration;
        private readonly BlobServiceClient storageServiceClient;

        public ImageUploadClient(IConfiguration configuration,
            IAzureClientFactory<BlobServiceClient> blobClientFactory,
            ILogger<ImageUploadClient> log)
        {
            logger = log;
            this.configuration = configuration;
            this.storageServiceClient = blobClientFactory.CreateClient(Constants.BlobClientName);
        }

        [Function(nameof(ImageUploadClient))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "durable/analyze")] HttpRequest req,
            [DurableClient] DurableTaskClient client)
        {
            if (!req.IsValid())
                return new BadRequestResult();

            var file = req.Form.Files[0];
            var operationContext = OperationContext.CreateContext(file);

            var destinationContainerName = configuration.GetValue<string>("DestinationContainer");
            var blobContainerClient = storageServiceClient.GetBlobContainerClient(destinationContainerName);
            await file.UploadToStorageAsync(operationContext.BlobName, blobContainerClient);

            var orchestratorName = new TaskName(nameof(ImageAnalizerOrchestrator));
            var StartOrchestrationOption = new StartOrchestrationOptions(operationContext.OperationId);
            await client.ScheduleNewOrchestrationInstanceAsync(orchestratorName,
                 operationContext, StartOrchestrationOption);

            var orcestrationMetadata = await client.WaitForInstanceCompletionAsync(operationContext.OperationId, true);

            AnalyzeFaceFromStreamResponse response = null;
            if (orcestrationMetadata.IsCompleted)
            {
                response = JsonSerializer.Deserialize<AnalyzeFaceFromStreamResponse>(orcestrationMetadata.SerializedOutput);
            }

            return new OkObjectResult(response); ;
        }
    }
}
