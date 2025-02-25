using ServerlessFacesAnalyzer.FuncApp.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.FuncApp.DurableFunctions.Dtos
{
    public class SendNotificationToEventGridDto
    {
        public OperationContext OperationContext { get; set; }
        public AnalyzeFaceFromStreamResponse AnalysisResult { get; set; }
    }
}
