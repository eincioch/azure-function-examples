using System;
using Microsoft.Azure.WebJobs;

namespace ServiceBusTriggerExample
{
    public class Functions
    {
        [FunctionName("ServiceBusOutputTrigger")]
        [return: ServiceBus("examplequeue", Connection = "ServiceBusConnectionString")]
        public string ServiceBusOutput([HttpTrigger] dynamic input)
        {
            try
            {
                return input.Text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("No Service Bus connection found.");
            }

            return null;
        }

        [FunctionName("ServiceBusTopicTrigger")]
        public void SubscribeToTopic(
            [ServiceBusTrigger("exampletopic", "examplesubscription", Connection = "ServiceBusConnectionString")] string message)
        {
            Console.WriteLine($"Message received from topic: {message}");
        }

        [FunctionName("ServiceBusQueueTrigger")]
        public void SubscribeToQueue(
            [ServiceBusTrigger("examplequeue", Connection = "ServiceBusConnectionString")] string message,
            int deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId)
        {
            Console.WriteLine($"C# ServiceBus queue trigger function processed message: {message}");
            Console.WriteLine($"EnqueuedTimeUtc={enqueuedTimeUtc}");
            Console.WriteLine($"DeliveryCount={deliveryCount}");
            Console.WriteLine($"MessageId={messageId}");
        }
    }
}
