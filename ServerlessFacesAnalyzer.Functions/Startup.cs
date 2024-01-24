using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServerlessFacesAnalyzer.Cognitive;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Functions;
using ServerlessFacesAnalyzer.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]

namespace ServerlessFacesAnalyzer.Functions
{
    public class Startup: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IFaceAnalyzer, VisionServiceFaceAnalyzer>();
            builder.Services.AddScoped<IImageProcessor, ImageProcessor>();
        }
    }
}
