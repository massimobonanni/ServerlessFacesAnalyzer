using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.FuncApp.DurableFunctions.Dtos
{
    public class ExtractFaceFromImageDto
    {
        public OperationContext OperationContext { get; set; }
        public FaceInfo Face { get; set; }
        public int FaceIndex { get; set; }
    }
}
