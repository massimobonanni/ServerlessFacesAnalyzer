using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class SaveFaceAnalysisResultActivity
    {
        private readonly ILogger<SaveFaceAnalysisResultActivity> logger;

        public SaveFaceAnalysisResultActivity(ILogger<SaveFaceAnalysisResultActivity> log)
        {
            logger = log;
        }

        [FunctionName(nameof(SaveFaceAnalysisResultActivity))]
        public async Task Run([ActivityTrigger] SaveFaceAnalysisResultDto context,
            [Blob("%DestinationContainer%", FileAccess.Read, Connection = "StorageConnectionString")] CloudBlobContainer containerClient)
        {
            var resultBlobName = context.OperationContext.GenerateResultFileName();

            var resultBlob = containerClient.GetBlockBlobReference(resultBlobName);
            await resultBlob.UploadTextAsync(JsonConvert.SerializeObject(context.FaceResultResponse, Formatting.Indented));
        }
    }
}
