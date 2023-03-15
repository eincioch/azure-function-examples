using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace StorageQueueTriggerDemo
{
    public class Functions
    {
        [FunctionName("HttpTrigger")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "message" })]
        [OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Message to put on the queue")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Reply message")]
        [return: Queue("message-queue-demo")]
        public async Task<string> HttpTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            string message = req.Query["message"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            message = message ?? data?.name;

            if (string.IsNullOrWhiteSpace(message))
                return null;

            return message;
        }

        [FunctionName("StorageQueueTrigger")]
        public void Run([QueueTrigger("message-queue-demo", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            Console.WriteLine($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
