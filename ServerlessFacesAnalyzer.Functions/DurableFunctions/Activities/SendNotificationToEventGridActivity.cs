using Azure.Messaging.EventGrid;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFacesAnalyzer.Core.Interfaces;
using ServerlessFacesAnalyzer.Core.Models;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class SendNotificationToEventGridActivity
    {
        private readonly ILogger<SendNotificationToEventGridActivity> logger;

        public SendNotificationToEventGridActivity(ILogger<SendNotificationToEventGridActivity> log)
        {
            logger = log;
        }

        [FunctionName(nameof(SendNotificationToEventGridActivity))]
        public async Task Run([ActivityTrigger] SendNotificationToEventGridDto context,
            [EventGrid(TopicEndpointUri = "TopicEndpoint", TopicKeySetting = "TopicKey")] IAsyncCollector<EventGridEvent> eventCollector)
        {
            var @event = new EventGridEvent(
              subject: context.OperationContext.BlobName,
              eventType: "ImageAnalyzed",
              dataVersion: "1.0",
              data: context.AnalysisResult);

            await eventCollector.AddAsync(@event);

            logger.LogInformation("Event sended to custom topic", JsonConvert.SerializeObject(@event));
        }
    }
}
