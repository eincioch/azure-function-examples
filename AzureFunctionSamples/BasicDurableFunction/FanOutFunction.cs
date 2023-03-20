using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicDurableFunction
{
    public static class FanOutFunction
    {
        [FunctionName("FanOutFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var userIds = await context.CallActivityAsync<List<int>>("FanOutFunction_GetUsersToProcess", null);
            var tasks = new List<Task>();

            foreach (var id in userIds)
                tasks.Add(context.CallActivityAsync("FanOutFunction_ProcessUser", id));

            await Task.WhenAll(tasks);
        }

        [FunctionName("FanOutFunction_GetUsersToProcess")]
        public static List<int> GetUsersToProcess([ActivityTrigger] IDurableActivityContext context)
        {
            var userIds = new List<int>();

            for (var i = 1; i < 101; i++)
            {
                userIds.Add(i);
            }

            return userIds;
        }

        [FunctionName("FanOutFunction_ProcessUser")]
        public static async Task ProcessUser([ActivityTrigger] int id, ILogger log)
        {
            await Task.Delay(1000);
            log.LogInformation($"User {id} processed.");
        }

        [FunctionName("FanOutFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("FanOutFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}