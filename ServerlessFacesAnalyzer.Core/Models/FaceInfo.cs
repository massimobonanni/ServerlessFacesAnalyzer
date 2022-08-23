using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Models
{
    public class FaceInfo
    {
        public int Age { get; set; }
        public string Gender { get; set; }
        public FaceRectangle Rectangle { get; set; }
    }
}
