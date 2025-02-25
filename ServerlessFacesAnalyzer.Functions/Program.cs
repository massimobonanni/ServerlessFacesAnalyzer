using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerlessFacesAnalyzer.Cognitive;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Functions;
using ServerlessFacesAnalyzer.ImageProcessing;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext,services) =>
    {
        services.AddLogging();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(hostContext.Configuration["StorageConnectionString"])
                .WithName(Constants.BlobClientName);
            builder.AddEventGridPublisherClient(new Uri(hostContext.Configuration["TopicEndpoint"]),
                    new AzureKeyCredential(hostContext.Configuration["TopicKey"]))
                .WithName(Constants.EventGridClientName);
        });

        var faceAnalyzerImplementation = hostContext.Configuration["FaceAnalyzerImplementation"];
        if (string.IsNullOrWhiteSpace(faceAnalyzerImplementation))
            faceAnalyzerImplementation = "vision";

        switch (faceAnalyzerImplementation.ToLower())
        {
            case "face":
                // Face Service
                services.AddScoped<IFaceAnalyzer, FaceServiceFaceAnalyzer>();
                break;
            case "vision":
            default:
                // Vision Service 4.0
                services.AddScoped<IFaceAnalyzer, VisionServiceFaceAnalyzer>();
                break;
        }
        services.AddScoped<IImageProcessor, ImageProcessor>();
    })
    .Build();

host.Run();