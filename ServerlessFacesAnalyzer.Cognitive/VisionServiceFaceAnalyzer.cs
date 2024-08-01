using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using System.Diagnostics;
using Azure;
using Azure.AI.Vision.Common;
using Azure.AI.Vision.ImageAnalysis;
using System.IO;

namespace ServerlessFacesAnalyzer.Cognitive
{
    public class VisionServiceFaceAnalyzer : IFaceAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "VisionServiceFaceAnalyzer";
            public string ServiceEndpoint { get; set; }
            public string ServiceKey { get; set; }

            public int ConfidenceThreshold { get; set; } = 80;

            public VisualFeatures Features = VisualFeatures.People;

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

        public VisionServiceFaceAnalyzer(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<VisionServiceFaceAnalyzer>();
        }

        private ImageAnalysisClient CreateImageAnalysisClient(Configuration config)
        {
            var credentials = new AzureKeyCredential(config.ServiceKey);
            return new ImageAnalysisClient(new Uri(config.ServiceEndpoint), credentials);
        }

        private FaceAnalyzerResult AnalyzeVisionResult(Response<ImageAnalysisResult> imageAnalysis, long elapsedMilliseconds, Configuration config)
        {
            var result = new FaceAnalyzerResult()
            {
                ElapsedTimeInMilliseconds = elapsedMilliseconds,
            };

            if (imageAnalysis.Value.People != null && imageAnalysis.Value.People.Values.Any())
            {
                foreach (var people in imageAnalysis.Value.People.Values)
                {
                    if (people.Confidence * 100 >= config.ConfidenceThreshold)
                        result.Faces.Add(people.ToFaceInfo());
                }
            }
            return result;
        }

        public async Task<FaceAnalyzerResult> AnalyzeAsync(Stream imageStream,
            CancellationToken cancellationToken = default)
        {
            var config = Configuration.Load(configuration);

            try
            {
                var imageClient = CreateImageAnalysisClient(config);
                var stopWatch = Stopwatch.StartNew();
                var visionResponse = await imageClient.AnalyzeAsync(BinaryData.FromStream(imageStream),
                    config.Features, cancellationToken: cancellationToken);
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
                var imageClient = CreateImageAnalysisClient(config);
                var stopWatch = Stopwatch.StartNew();
                var visionResponse = await imageClient.AnalyzeAsync(new Uri(imageUrl),
                    config.Features, cancellationToken: cancellationToken);
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
