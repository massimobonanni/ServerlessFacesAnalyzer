using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpExtensions
    {
        public static async Task UploadToStorageAsync(this IFormFile file, 
            string blobName,
            CloudBlobContainer blobContainer,
            CancellationToken token=default)
        {
            var blob = blobContainer.GetBlockBlobReference(blobName);
            using (var sourceStream = file.OpenReadStream())
            {
                await blob.UploadFromStreamAsync(sourceStream, token);
            }
        }

        public static async Task<FaceAnalyzerResult> AnalyzeAsync(this IFormFile file,
            IFaceAnalyzer faceAnalyzer,
            CancellationToken token = default)
        {
            FaceAnalyzerResult faceresult;
            using (var sourceStream = file.OpenReadStream())
            {
                faceresult = await faceAnalyzer.AnalyzeAsync(sourceStream);
            }
            return faceresult;
        }
    }
}
