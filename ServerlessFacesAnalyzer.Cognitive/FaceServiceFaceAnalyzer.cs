using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using System.Diagnostics;

namespace ServerlessFacesAnalyzer.Cognitive
{
    public class FaceServiceFaceAnalyzer : IFaceAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "FaceServiceFaceAnalyzer";
            public required string ServiceEndpoint { get; set; }
            public required string ServiceKey { get; set; }

            public static Configuration Load(IConfiguration config)
            {
                var serviceEndpoint = config[$"{ConfigRootName}:ServiceEndpoint"];
                var serviceKey = config[$"{ConfigRootName}:ServiceKey"];
                if (serviceEndpoint == null || serviceKey == null)
                {
                    throw new InvalidOperationException("ServiceEndpoint and ServiceKey must be provided in the configuration.");
                }
                return new Configuration()
                {
                    ServiceEndpoint = serviceEndpoint ,
                    ServiceKey = serviceKey 
                };
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
                return AnalyzeFaceResult(response, stopWatch.ElapsedMilliseconds, config);
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
                var faceClient = CreateFaceClient(config);
                var stopWatch = Stopwatch.StartNew();
                var response = await faceClient.Face.DetectWithUrlAsync(imageUrl, returnFaceId: false);
                stopWatch.Stop();
                return AnalyzeFaceResult(response, stopWatch.ElapsedMilliseconds, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during face analysis");
                throw;
            }
        }

        private FaceAnalyzerResult AnalyzeFaceResult(IList<DetectedFace> response, long elapsedMilliseconds, Configuration config)
        {
            var result = new FaceAnalyzerResult()
            {
                ElapsedTimeInMilliseconds = elapsedMilliseconds,
            };

            if (response != null && response.Any())
            {
                foreach (var face in response)
                {
                    result.Faces.Add(face.ToFaceInfo());
                }
            }
            return result;
        }


    }
}
