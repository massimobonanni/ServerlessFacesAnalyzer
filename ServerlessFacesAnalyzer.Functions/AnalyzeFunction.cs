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

namespace ServerlessFacesAnalyzer.Functions
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
            this.logger = log;
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
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer destinationContainer)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!req.ContentType.StartsWith("multipart/form-data"))
                return new BadRequestResult();
            if (req.Form == null || req.Form.Files == null || !req.Form.Files.Any())
                return new BadRequestResult();

            var operationId = Guid.NewGuid().ToString();
            var dateNow = DateTime.UtcNow;
            var file = req.Form.Files[0];
            var extension = (new FileInfo(file.FileName)).Extension;
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

            for (int i = 0; i < faceresult.Faces.Count; i++)
            {
                var face = faceresult.Faces[i];
                var faceBlobName = $"{blobFolder}\\face{i + 1}{extension}";
                var faceBlob = destinationContainer.GetBlockBlobReference(faceBlobName);
                using (var sourceStream = file.OpenReadStream())
                using (var faceBlobStream = faceBlob.OpenWrite())
                {
                    await this.imageProcessor.CropImageAsync(sourceStream, face.Rectangle, faceBlobStream);
                }
            }

            return new OkObjectResult(response);
        }


    }
}