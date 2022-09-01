using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class AnalyzeImageActivity
    {
        private readonly ILogger<AnalyzeImageActivity> logger;
        private readonly IFaceAnalyzer faceAnalyzer;

        public AnalyzeImageActivity(IFaceAnalyzer faceAnalyzer,
           ILogger<AnalyzeImageActivity> log)
        {
            logger = log;
            this.faceAnalyzer = faceAnalyzer;
        }

        [FunctionName(nameof(AnalyzeImageActivity))]
        public async Task<FaceAnalyzerResult> Run([ActivityTrigger] OperationContext context,
            [Blob("%DestinationContainer%", FileAccess.Read, Connection = "StorageConnectionString")] CloudBlobContainer containerClient)
        {
            var blobReference = containerClient.GetBlockBlobReference(context.BlobName);

            FaceAnalyzerResult result;
            using (var stream = await blobReference.OpenReadAsync())
            {
                result = await this.faceAnalyzer.AnalyzeAsync(stream);
            }

            return result;
        }
    }
}
