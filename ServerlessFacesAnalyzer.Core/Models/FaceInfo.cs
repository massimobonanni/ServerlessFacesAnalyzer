using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Models
{
    public class FaceInfo
    {
        public string? Id { get; set; }
        public FaceRectangle? Rectangle { get; set; }
    }
}
