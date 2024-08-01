using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
    public class Startup : FunctionsStartup
    {
        public IConfiguration Config { get; private set; }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);
            Config = builder.ConfigurationBuilder.AddEnvironmentVariables().Build();

        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var faceAnalyzerImplementation = Config.GetValue<string>("FaceAnalyzerImplementation");
            if (string.IsNullOrWhiteSpace(faceAnalyzerImplementation))
                faceAnalyzerImplementation = "vision";
    
            switch (faceAnalyzerImplementation.ToLower())
            {
                case "face":
                    // Face Service
                    builder.Services.AddScoped<IFaceAnalyzer, FaceServiceFaceAnalyzer>();
                    break;
                case "vision":
                default:
                    // Vision Service 4.0
                    builder.Services.AddScoped<IFaceAnalyzer, VisionServiceFaceAnalyzer>();
                    break;
            }
            builder.Services.AddScoped<IImageProcessor, ImageProcessor>();
        }
    }
}
