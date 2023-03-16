using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SignalRTriggerDemo
{
    public static class Functions
    {
        [FunctionName("Negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "ExampleHub")] SignalRConnectionInfo connectionInfo)
        {
            try
            {
                return connectionInfo;
            }
            catch
            {
                Console.WriteLine($"No SignalR connection is established");
            }

            return new SignalRConnectionInfo();
        }

        [FunctionName("ReceiveMessage")]
        public static async Task ReceiveMessage(
            [SignalRTrigger("ExampleHub", "messages", "SendMessage", 
            parameterNames: new string[] { "message" })] InvocationContext invocationContext, string message, ILogger logger)
        {
            Console.WriteLine($"Receive {message} from {invocationContext.ConnectionId}.");
        }

        [FunctionName("ReceiveMessageWithParams")]
        public static async Task ReceiveMessageWithParams(
            [SignalRTrigger("ExampleHub", "messages", "SendMessageWithParams")] InvocationContext invocationContext, 
            [SignalRParameter] string message, ILogger logger)
        {
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
        }

        [FunctionName("SendMessage")]
        public static async Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object message,
            [SignalR(HubName = "ExampleHub")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            try
            {
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = "BroadcastMessage",
                        Arguments = new[] { message }
                    });
            }
            catch
            {
                Console.WriteLine($"No SignalR connection is established");
            }
        }
    }
}