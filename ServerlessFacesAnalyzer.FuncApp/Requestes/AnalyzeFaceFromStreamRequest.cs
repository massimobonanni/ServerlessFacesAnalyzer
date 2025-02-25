using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.FuncApp.Requestes
{
    public class AnalyzeFaceFromStreamRequest
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
    }
}
