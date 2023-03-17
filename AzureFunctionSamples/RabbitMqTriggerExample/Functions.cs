using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace RabbitMqTriggerExample
{
    public class Functions
    {
        [FunctionName("RabbitMqOutputTrigger")]
        [return: RabbitMQ(QueueName = "testqueue", ConnectionStringSetting = "RabbitMqConnectionString")]
        public string RabbitMqOutputTrigger([HttpTrigger] string input)
        {
            try
            {
                return input;
            }
            catch
            {
                Console.WriteLine("No RabbitMQ connection found.");
            }

            return null;
        }

        [FunctionName("RabbitMqTrigger")]
        public void RabbitMqTrigger([RabbitMQTrigger("testqueue", ConnectionStringSetting = "RabbitMqConnectionString")]string queueItem, ILogger log)
        {
            Console.WriteLine($"Item processed by RabbitMQ: {queueItem}");
        }
    }
}
