using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions
{
    internal class OperationContext
    {
        private OperationContext()
        {

        }

        public string OperationId { get; private set; }
        public string Extension { get; private set; }
        public string BlobFolder { get; private set; }
        public string BlobName { get; private set; }

        public static OperationContext CreateContext(IFormFile file)
        {
            var context = new OperationContext();
            context.OperationId = Guid.NewGuid().ToString();
            var dateNow = DateTime.UtcNow;
            context.Extension = (new FileInfo(file.FileName)).Extension;
            context.BlobFolder = $"{dateNow:yyyy}\\{dateNow:MM}\\{dateNow:dd}\\{context.OperationId}";
            context.BlobName = $"{context.BlobFolder}\\{file.FileName}";
            return context;
        }

        public string GenerateResultFileName()=> $"{BlobFolder}\\result.json";
        public string GenerateFaceFileName(int fileIndex) => $"{BlobFolder}\\face{fileIndex + 1}{Extension}";

    }
}
