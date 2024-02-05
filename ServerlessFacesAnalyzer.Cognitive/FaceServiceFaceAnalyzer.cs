using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Cognitive
{
    public class FaceServiceFaceAnalyzer : IFaceAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "FaceServiceFaceAnalyzer";
            public string ServiceEndpoint { get; set; }
            public string ServiceKey { get; set; }

            public int ConfidenceThreshold { get; set; } = 80;

            public static Configuration Load(IConfiguration config)
            {
                var retVal = new Configuration();
                retVal.ServiceEndpoint = config[$"{ConfigRootName}:ServiceEndpoint"];
                retVal.ServiceKey = config[$"{ConfigRootName}:ServiceKey"];
                if (config[$"{ConfigRootName}:ConfidenceThreshold"] != null)
                    if (int.TryParse(config[$"{ConfigRootName}:ConfidenceThreshold"], out var threshold))
                        retVal.ConfidenceThreshold = threshold;
                return retVal;
            }
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<VisionServiceFaceAnalyzer> logger;

        public FaceServiceFaceAnalyzer(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<VisionServiceFaceAnalyzer>();
        }

        private IFaceClient CreateFaceClient(Configuration config)
        {
            var credentials = new ApiKeyServiceClientCredentials(config.ServiceKey);
            return new FaceClient(credentials) { Endpoint = config.ServiceEndpoint };
        }

        // https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/quickstarts-sdk/identity-client-library?tabs=windows%2Cvisual-studio&pivots=programming-language-csharp

        public async Task<FaceAnalyzerResult> AnalyzeAsync(Stream imageStream, CancellationToken cancellationToken = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var faceClient = CreateFaceClient(config);
                var stopWatch = Stopwatch.StartNew();
                var response = await faceClient.Face.DetectWithStreamAsync(imageStream, returnFaceId: false);
                
                stopWatch.Stop();
                return AnalyzeVisionResult(visionResponse, stopWatch.ElapsedMilliseconds, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during face analysis");
                throw;
            }
        }

        public Task<FaceAnalyzerResult> AnalyzeAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
