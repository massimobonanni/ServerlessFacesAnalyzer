using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions.Responses
{
    public class AnalyzeFaceFromStreamResponse
    {
        public string OperationId { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public FaceAnalyzerResult AnalysisResult { get; set; }
    }
}
