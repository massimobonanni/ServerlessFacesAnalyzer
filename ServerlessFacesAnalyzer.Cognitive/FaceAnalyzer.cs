using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Diagnostics;

namespace ServerlessFacesAnalyzer.Cognitive
{
    public class FaceAnalyzer : IFaceAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "FaceAnalyzer";
            public string ServiceEndpoint { get; set; }
            public string ServiceKey { get; set; }

            public List<VisualFeatureTypes?> Features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Faces
            };

            public List<Details?> Details = new List<Details?>()
            {
                Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.Details.Landmarks
            };

            public static Configuration Load(IConfiguration config)
            {
                var retVal = new Configuration();
                retVal.ServiceEndpoint = config[$"{ConfigRootName}:ServiceEndpoint"];
                retVal.ServiceKey = config[$"{ConfigRootName}:ServiceKey"];
                return retVal;
            }
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<FaceAnalyzer> logger;

        public FaceAnalyzer(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<FaceAnalyzer>();
        }

        private ComputerVisionClient CreateVisionClient(Configuration config)
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(config.ServiceKey);
            return new ComputerVisionClient(credentials)
            {
                Endpoint = config.ServiceEndpoint
            };
        }

        private FaceAnalyzerResult AnalyzeVisionResult(ImageAnalysis imageAnalysis, long elapsedMilliseconds, Configuration config)
        {
            var result = new FaceAnalyzerResult()
            {
                ElapsedTimeInMilliseconds = elapsedMilliseconds
            };

            foreach (var face in imageAnalysis.Faces)
            {
                result.Faces.Add(face.ToFaceInfo());
            }

            return result;
        }

        public async Task<FaceAnalyzerResult> AnalyzeAsync(Stream imageStream,
            CancellationToken cancellationToken = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var visionClient = CreateVisionClient(config);
                var stopWatch = Stopwatch.StartNew();
                var visionResponse = await visionClient.AnalyzeImageInStreamAsync(imageStream,
                    config.Features, config.Details, cancellationToken: cancellationToken);
                stopWatch.Stop();
                return AnalyzeVisionResult(visionResponse, stopWatch.ElapsedMilliseconds, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during face analysis");
                throw;
            }
        }

        public async Task<FaceAnalyzerResult> AnalyzeAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var visionClient = CreateVisionClient(config);
                var stopWatch = Stopwatch.StartNew();
                var visionResponse = await visionClient.AnalyzeImageAsync(imageUrl,
                    config.Features, config.Details, cancellationToken: cancellationToken);
                stopWatch.Stop();
                return AnalyzeVisionResult(visionResponse, stopWatch.ElapsedMilliseconds, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during face analysis");
                throw;
            }
        }
    }
}
