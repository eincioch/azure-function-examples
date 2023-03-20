using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace BasicDurableFunction
{
    public static class FunctionWithCancellation
    {
        [FunctionName("FunctionWithCancellation")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            await context.CallActivityAsync("FunctionWithCancellation_LongRunningTask", null);
            await context.CallActivityAsync("FunctionWithCancellation_LongRunningTask", null);
            await context.CallActivityAsync("FunctionWithCancellation_LongRunningTask", null);
        }

        [FunctionName("FunctionWithCancellation_LongRunningTask")]
        public static async Task LongRunningTask([ActivityTrigger] IDurableActivityContext context)
        {
            await Task.Delay(10000);
        }

        [FunctionName("FunctionWithCancellation_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FunctionWithCancellation", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("FunctionWithCancellation_TerminateInstance")]
        public static async Task<HttpResponseMessage> TerminateInstance(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "terminate/{instanceId}")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient client,
        string instanceId,
        ILogger log)
        {
            if (!await ActiveInstanceExists(client, instanceId))
            {
                return new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent($"An instance with ID '{instanceId}' doesn't exist or is inactive."),
                };
            }

            var reason = $"Termination requested for instance ID {instanceId}.";
            await client.TerminateAsync(instanceId, reason);
            await client.PurgeInstanceHistoryAsync(instanceId);

            log.LogInformation(reason);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(reason),
            };
        }

        [FunctionName("FunctionWithCancellation_TerminateAllInstances")]
        public static async Task<HttpResponseMessage> TerminateAllInstances(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "terminate")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            var noFilter = new OrchestrationStatusQueryCondition();
            var runningInstances = await client.ListInstancesAsync(
                noFilter,
                CancellationToken.None);

            var terminatedInstanceCount = 0;
            var reason = $"Termination requested for all active instances.";

            foreach (var instance in runningInstances.DurableOrchestrationState
                .Where(i => InstanceInActiveState(i.RuntimeStatus)))
            {
                await client.TerminateAsync(instance.InstanceId, reason);
                await client.PurgeInstanceHistoryAsync(instance.InstanceId);
                log.LogInformation("Instance {InstanceId} terminated.", instance.InstanceId);
                terminatedInstanceCount++;
            }

            var outcome = terminatedInstanceCount > 0 ?
                "All active instances terminated." :
                "No active instances found to terminate.";

            log.LogInformation(outcome);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(outcome),
            };
        }

        private static async Task<bool> ActiveInstanceExists(IDurableOrchestrationClient client, string instanceId)
        {
            var existingInstance = await client.GetStatusAsync(instanceId);

            return existingInstance != null
                && InstanceInActiveState(existingInstance.RuntimeStatus);
        }

        private static bool InstanceInActiveState(OrchestrationRuntimeStatus status)
        {
            return status != OrchestrationRuntimeStatus.Completed
                && status != OrchestrationRuntimeStatus.Failed
                && status != OrchestrationRuntimeStatus.Terminated;
        }
    }
}