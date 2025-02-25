using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessFacesAnalyzer.FuncApp.DurableFunctions.Dtos;

namespace ServerlessFacesAnalyzer.FuncApp.DurableFunctions.Activities
{
    public class SendNotificationToEventGridActivity
    {
        private readonly ILogger<SendNotificationToEventGridActivity> logger;
        private readonly EventGridPublisherClient eventClient;

        public SendNotificationToEventGridActivity(IAzureClientFactory<EventGridPublisherClient> eventClientFactory,
            ILogger<SendNotificationToEventGridActivity> log)
        {
            logger = log;
            eventClient = eventClientFactory.CreateClient(Constants.EventGridClientName);
        }

        [Function(nameof(SendNotificationToEventGridActivity))]
        public async Task Run([ActivityTrigger] SendNotificationToEventGridDto context)
        {
            var @event = new EventGridEvent(
              subject: context.OperationContext.BlobName,
              eventType: "ImageAnalyzed",
              dataVersion: "1.0",
              data: context.AnalysisResult);

            await eventClient.SendEventAsync(@event);

            logger.LogInformation("Event sended to custom topic : {0}", JsonConvert.SerializeObject(@event));
        }
    }
}
