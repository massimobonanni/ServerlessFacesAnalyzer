using Newtonsoft.Json;
using ServerlessFacesAnalyzer.Functions.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Storage.Blob
{
    public static class CloudBlobContainerExtensions
    {
        public static async Task SerializeObjectToBlobAsync(this CloudBlobContainer destinationContainer,
            string blobName,
            object obj,
            CancellationToken cancellationToken = default)
        {

            var resultBlob = destinationContainer.GetBlockBlobReference(blobName);
            await resultBlob.UploadTextAsync(JsonConvert.SerializeObject(obj, Formatting.Indented), cancellationToken);
        }
    }

    public static class CloudBlockBlobExtensions
    {
        public static string GetSasUrl(this CloudBlockBlob blob, SharedAccessBlobPolicy policy)
        {
            return $"{blob.Uri}{blob.GetSharedAccessSignature(policy)}"; 
        }
    }
}
