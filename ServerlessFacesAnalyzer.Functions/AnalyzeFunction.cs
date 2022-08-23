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

namespace ServerlessFacesAnalyzer.Functions
{
    public class AnalyzeFunction
    {
        private readonly ILogger<AnalyzeFunction> logger;
        private readonly IFaceAnalyzer faceAnalyzer;
        private readonly IConfiguration configuration;

        public AnalyzeFunction(IFaceAnalyzer faceAnalyzer,
            IConfiguration configuration, ILogger<AnalyzeFunction> log)
        {
            this.logger = log;
            this.faceAnalyzer = faceAnalyzer;
            this.configuration = configuration;
        }

        [FunctionName("AnalyzeFaceFromStream")]
        [OpenApiOperation(operationId: "AnalyzeFaceFromStream")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> AnalyzeFaceFromStream(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "analyze")] HttpRequest req,
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer destinationContainer)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!req.Form.Files.Any())
                return new BadRequestResult();

            var operationId = Guid.NewGuid().ToString();
            var dateNow = DateTime.UtcNow;
            var file = req.Form.Files[0];
            var blobFolder = $"{dateNow:yyyy}\\{dateNow:MM}\\{dateNow:dd}\\{operationId}";
            var blobName = $"{blobFolder}\\{file.FileName}";

            await file.UploadToStorageAsync(blobName, destinationContainer);

            var faceresult = await file.AnalyzeAsync(this.faceAnalyzer);

            var response = new AnalyzeFaceFromStreamResponse()
            {
                OperationId = operationId,
                OriginalFileName = file.FileName,
                FileName = blobName,
                AnalysisResult = faceresult
            };

            var resultBlobName = $"{blobFolder}\\result.json";
            await destinationContainer.SerializeObjectToBlobAsync(resultBlobName, response);

            return new OkObjectResult(response);
        }

        
    }
}
