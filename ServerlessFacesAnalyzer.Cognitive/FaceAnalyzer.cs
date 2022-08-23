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

namespace ServerlessFacesAnalyzer.Cognitive
{
    public class FaceAnalyzer : IFaceAnalyzer
    {
        public class Configuration
        {
            const string ConfigRootName = "FaceAnalyzer";
            public string ServiceEndpoint { get; set; }
            public string ServiceKey { get; set; }
            public int AgeThreshold { get; set; }

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
                retVal.AgeThreshold = config.GetValue<int>($"{ConfigRootName}:AgeThreshold");
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

        private FaceAnalyzerResult AnalyzeVisionResult(ImageAnalysis imageAnalysis, Configuration config)
        {
            var result = new FaceAnalyzerResult();

            foreach (var face in imageAnalysis.Faces)
            {
                if (face.Age >= config.AgeThreshold)
                {
                    var faceResult = new FaceInfo()
                    {
                        Age = face.Age,
                        Gender = face.Gender?.ToString(),
                        Rectangle = new Core.Models.FaceRectangle()
                        {
                           Left=face.FaceRectangle.Left,
                           Top=face.FaceRectangle.Top,
                           Width=face.FaceRectangle.Width,
                           Height=face.FaceRectangle.Height
                        }
                    };

                    result.Faces.Add(faceResult);
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
                var visionClient = CreateVisionClient(config);
                var visionResponse = await visionClient.AnalyzeImageInStreamAsync(imageStream,
                    config.Features, config.Details, cancellationToken: cancellationToken);

                return AnalyzeVisionResult(visionResponse, config);
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
                var visionResponse = await visionClient.AnalyzeImageAsync(imageUrl,
                    config.Features, config.Details, cancellationToken: cancellationToken);

                return AnalyzeVisionResult(visionResponse, config);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during face analysis");
                throw;
            }
        }
    }
}
