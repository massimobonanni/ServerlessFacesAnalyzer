using ServerlessFacesAnalyzer.Core.Models;
using ServerlessFacesAnalyzer.Functions.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos
{
    public class SaveFaceAnalysisResultDto
    {
        public OperationContext OperationContext { get;  set; }
        public AnalyzeFaceFromStreamResponse FaceResultResponse { get;  set; }
    }
}
