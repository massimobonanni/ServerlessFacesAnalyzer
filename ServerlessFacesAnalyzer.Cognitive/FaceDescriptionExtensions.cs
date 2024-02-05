using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using ServerlessFacesAnalyzer.Core.Models;

namespace Azure.AI.Vision.ImageAnalysis
{
    public static class FaceDescriptionExtensions
    {
        public static FaceInfo ToFaceInfo(this DetectedPerson source)
        {
            return new FaceInfo()
            {
                Id = Guid.NewGuid().ToString(),
                Rectangle = source.ToFaceRectangle()
            };
        }

        public static FaceInfo ToFaceInfo(this DetectedFace source)
        {
            return new FaceInfo()
            {
                Id = Guid.NewGuid().ToString(),
                Rectangle = source.ToFaceRectangle()
            };
        }

        public static ServerlessFacesAnalyzer.Core.Models.FaceRectangle ToFaceRectangle(this DetectedPerson source)
        {
            return new ServerlessFacesAnalyzer.Core.Models.FaceRectangle()
            {
                Left = source.BoundingBox.X,
                Top = source.BoundingBox.Y,
                Width = source.BoundingBox.Width,
                Height = source.BoundingBox.Height
            };
        }

        public static ServerlessFacesAnalyzer.Core.Models.FaceRectangle ToFaceRectangle(this DetectedFace source)
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
