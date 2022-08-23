using Newtonsoft.Json;
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
}
