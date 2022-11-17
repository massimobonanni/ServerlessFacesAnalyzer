using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Models
{
    public class FaceAnalyzerResult
    {
        public long ElapsedTimeInMilliseconds { get; set; }

        public int NumberOfFaces => Faces.Count;

        public List<FaceInfo> Faces { get; set; }=new List<FaceInfo>();
    }
}
