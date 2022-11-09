using Microsoft.AspNetCore.Http;
using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions
{
    public class OperationContext
    {
        public OperationContext()
        {

        }

        public string OperationId { get; set; }
        public string Extension { get; set; }
        public string BlobFolder { get; set; }
        public string BlobName { get; set; }
        public string OriginalFileName { get; set; }

        public static OperationContext CreateContext(IFormFile file)
        {
            var context = new OperationContext();
            context.OperationId = Guid.NewGuid().ToString();
            context.OriginalFileName = file.FileName;
            var dateNow = DateTime.UtcNow;
            context.Extension = (new FileInfo(file.FileName)).Extension;
            context.BlobFolder = $"{dateNow:yyyy}\\{dateNow:MM}\\{dateNow:dd}\\{context.OperationId}";
            context.BlobName = $"{context.BlobFolder}\\{file.FileName}";
            return context;
        }

        public string GenerateResultFileName() => $"{BlobFolder}\\result.json";
        public string GenerateFaceFileName(int fileIndex,FaceInfo face) => $"{BlobFolder}\\face-{face.Id}{Extension}";

    }
}
