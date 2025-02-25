using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFacesAnalyzer.Functions.DurableFunctions.Dtos;

namespace ServerlessFacesAnalyzer.Functions.DurableFunctions.Activities
{
    public class SendNotificationToEventGridActivity
    {
        private readonly ILogger<SendNotificationToEventGridActivity> logger;
        private readonly EventGridPublisherClient eventClient;

        public SendNotificationToEventGridActivity(IAzureClientFactory<EventGridPublisherClient> eventClientFactory,
            ILogger<SendNotificationToEventGridActivity> log)
        {
            logger = log;
            this.eventClient = eventClientFactory.CreateClient(Constants.EventGridClientName);
        }

        [Function(nameof(SendNotificationToEventGridActivity))]
        public async Task Run([ActivityTrigger] SendNotificationToEventGridDto context)
        {
            var @event = new EventGridEvent(
              subject: context.OperationContext.BlobName,
              eventType: "ImageAnalyzed",
              dataVersion: "1.0",
              data: context.AnalysisResult);

            await this.eventClient.SendEventAsync(@event);

            logger.LogInformation("Event sended to custom topic", JsonConvert.SerializeObject(@event));
        }
    }
}
