using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicDurableFunction
{
    public static class ChainedFunction
    {
        [FunctionName("ChainedFunction")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var name = context.GetInput<string>();
            var userId = await context.CallActivityAsync<int>("ChainedFunction_GetUserId", name);
            var userBalance = await context.CallActivityAsync<double>("ChainedFunction_GetBankBalanceForUser", userId);
            return await context.CallActivityAsync<bool>("ChainedFunction_GetUserEligibilityForAccountUpgrade", userBalance);
        }

        [FunctionName("ChainedFunction_GetUserId")]
        public static int GetUserId([ActivityTrigger] string name, ILogger log)
        {
            var random = new Random();
            return random.Next(1, 100);
        }

        [FunctionName("ChainedFunction_GetBankBalanceForUser")]
        public static double GetBankBalanceForUser([ActivityTrigger] int id, ILogger log)
        {
            var random = new Random();
            return random.NextDouble();
        }

        [FunctionName("ChainedFunction_GetUserEligibilityForAccountUpgrade")]
        public static bool GetUserEligibilityForAccountUpgrade([ActivityTrigger] double balance, ILogger log)
        {
            return balance > 100;
        }

        [FunctionName("ChainedFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string name,
            ILogger log)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent($"'name' parameter must be provided."),
                };
            }

            string instanceId = await starter.StartNewAsync<string>("ChainedFunction", null, name);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}