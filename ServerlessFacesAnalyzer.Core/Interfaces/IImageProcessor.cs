using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Interfaces
{
    public interface IImageProcessor
    {
        Task CropImageAsync(Stream sourceImage, FaceRectangle rectangle, Stream outputImage);
    }
}
