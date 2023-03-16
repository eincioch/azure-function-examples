using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;

namespace IotHubTriggerExample
{
    public class Functions
    {
        [FunctionName("EventHubOutput")]
        [return: EventHub("EventHubInstance", Connection = "EventHubConnectionString")]
        public static string EventHubOutput([TimerTrigger("0 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            var eventMessage = $"Event executed at: {DateTime.Now}";
            Console.WriteLine(eventMessage);
            return eventMessage;
        }

        [FunctionName("IotHubTriggerExample")]
        public void IotHubTriggerExample([EventHubTrigger("messages/events", Connection = "EventHubConnectionString")]EventData message, ILogger log)
        {
            Console.WriteLine($"IoT hub message processed: {Encoding.UTF8.GetString(message.Body.Array)}");
        }
    }
}