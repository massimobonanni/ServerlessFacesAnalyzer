using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Models;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;
using ServerlessFacesAnalyzer.Functions.Responses;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Orchestrators
{
    public class ImageAnalizerOrchestrator
    {
        private readonly ILogger<ImageAnalizerOrchestrator> logger;
        private readonly IConfiguration configuration;

        public ImageAnalizerOrchestrator(IConfiguration configuration,
            ILogger<ImageAnalizerOrchestrator> log)
        {
            logger = log;
            this.configuration = configuration;
        }

        [FunctionName(nameof(ImageAnalizerOrchestrator))]
        public async Task<AnalyzeFaceFromStreamResponse> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var operationContext = context.GetInput<OperationContext>();

            var faceresult = await context.CallActivityAsync<FaceAnalyzerResult>(nameof(AnalyzeImageActivity), operationContext);

            var response = new AnalyzeFaceFromStreamResponse()
            {
                OperationId = operationContext.OperationId,
                OriginalFileName = operationContext.OriginalFileName,
                FileName = operationContext.BlobName,
                AnalysisResult = faceresult
            };

            await context.CallActivityAsync(nameof(SaveFaceAnalysisResultActivity),
                new SaveFaceAnalysisResultDto()
                {
                    OperationContext = operationContext,
                    FaceResultResponse = response
                });

            if (faceresult.Faces.Any())
            {
                var tasks = new Task<FaceBlob>[faceresult.Faces.Count];
                for (int i = 0; i < faceresult.Faces.Count; i++)
                {
                    tasks[i] = context.CallActivityAsync<FaceBlob>(nameof(ExtractFaceFromImageActivity),
                        new ExtractFaceFromImageDto()
                        {
                            OperationContext = operationContext,
                            Face = faceresult.Faces[i],
                            FaceIndex = i
                        });
                }

                await Task.WhenAll(tasks);

                response.FaceBlobs = tasks.Select(t => t.Result).ToList();
            }

            await context.CallActivityAsync(nameof(SendNotificationToEventGridActivity),
                new SendNotificationToEventGridDto()
                {
                    OperationContext = operationContext,
                    AnalysisResult = response
                });

            return response;
        }

    }
}