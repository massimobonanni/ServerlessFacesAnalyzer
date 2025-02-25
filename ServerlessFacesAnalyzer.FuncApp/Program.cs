using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerlessFacesAnalyzer.Cognitive;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.FuncApp;
using ServerlessFacesAnalyzer.ImageProcessing;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddLogging();
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();
builder.Services.AddAzureClients(b =>
{
    b.AddBlobServiceClient(builder.Configuration["StorageConnectionString"])
         .WithName(Constants.BlobClientName);
    b.AddEventGridPublisherClient(new Uri(builder.Configuration["TopicEndpoint"]),
                                    new AzureKeyCredential(builder.Configuration["TopicKey"]))
       .WithName(Constants.EventGridClientName);
});

var faceAnalyzerImplementation = builder.Configuration["FaceAnalyzerImplementation"];
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


builder.Build().Run();
