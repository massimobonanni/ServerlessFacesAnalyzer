using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class ExtractFaceFromImageActivity
    {
        private readonly ILogger<ExtractFaceFromImageActivity> logger;
        private readonly IImageProcessor imageProcessor;

        public ExtractFaceFromImageActivity(IImageProcessor imageProcessor,
           ILogger<ExtractFaceFromImageActivity> log)
        {
            logger = log;
            this.imageProcessor = imageProcessor;
        }

        [FunctionName(nameof(ExtractFaceFromImageActivity))]
        public async Task Run([ActivityTrigger] ExtractFaceFromImageDto context,
            [Blob("%DestinationContainer%", FileAccess.ReadWrite, Connection = "StorageConnectionString")] CloudBlobContainer containerClient)
        {
            var faceBlobName = context.OperationContext.GenerateFaceFileName(context.FaceIndex);
            var imageBlob= containerClient.GetBlockBlobReference(context.OperationContext.BlobName);
            var faceBlob = containerClient.GetBlockBlobReference(faceBlobName);

            using (var sourceStream = imageBlob.OpenRead())
            using (var faceStream=faceBlob.OpenWrite())
            {
                await imageProcessor.CropImageAsync(sourceStream, context.Face.Rectangle, faceStream);
            }
        }
    }
}
