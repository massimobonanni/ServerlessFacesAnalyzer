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
    }
}
