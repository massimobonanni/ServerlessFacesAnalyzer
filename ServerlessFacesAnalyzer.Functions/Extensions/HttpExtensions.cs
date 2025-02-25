using Azure.Storage.Blobs;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpExtensions
    {
        public static async Task UploadToStorageAsync(this IFormFile file,
            string blobName,
            BlobContainerClient blobContainer,
            CancellationToken token = default)
        {
            var blob = blobContainer.GetBlobClient(blobName);
            using (var sourceStream = file.OpenReadStream())
            {
                await blob.UploadAsync(sourceStream, token);
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

        public static bool IsValid(this HttpRequest req)
        {
            if (!req.ContentType.StartsWith("multipart/form-data"))
                return false;
            if (req.Form == null || req.Form.Files == null || !req.Form.Files.Any())
                return false;
            return true;
        }
    }
}
