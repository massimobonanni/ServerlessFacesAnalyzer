using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models
{
    public static class FaceDescriptionExtensions
    {
        public static FaceInfo ToFaceInfo(this FaceDescription source)
        {
            return new FaceInfo()
            {
                Rectangle = source.ToFaceRectangle()
            };
        }

        public static ServerlessFacesAnalyzer.Core.Models.FaceRectangle ToFaceRectangle(this FaceDescription source)
        {
            return new ServerlessFacesAnalyzer.Core.Models.FaceRectangle()
            {
                Left = source.FaceRectangle.Left,
                Top = source.FaceRectangle.Top,
                Width = source.FaceRectangle.Width,
                Height = source.FaceRectangle.Height
            };
        }
    }
}
