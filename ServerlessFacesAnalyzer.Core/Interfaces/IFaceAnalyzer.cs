using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Core.Interfaces
{
    public interface IFaceAnalyzer
    {
        Task<FaceAnalyzerResult> AnalyzeAsync(Stream imageStream,CancellationToken cancellationToken=default);
        Task<FaceAnalyzerResult> AnalyzeAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
