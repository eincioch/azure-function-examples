using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CosmosDbTriggerDemo
{
    public static class Functions
    {

        [FunctionName("DocByIdFromQueryString")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
                HttpRequest req,
            [CosmosDB(
                databaseName: "TestDb",
                collectionName: "DemoCollection",
                ConnectionStringSetting = "CosmosDbConnection",
                Id = "{Query.id}",
                PartitionKey = "{Query.partitionKey}")] DataItem item,
            ILogger log)
        {
            try
            {
                if (item == null)
                {
                    return new NoContentResult();
                }
                else
                {
                    return new OkObjectResult($"Found item with the description of: '{item.Description}'");
                }
            }
            catch
            {
                return new NoContentResult();
            }
        }

        [FunctionName("CosmosDbOutputTrigger")]
        public static async Task<IActionResult> CosmosDbOutputTrigger([HttpTrigger] string description,
            [CosmosDB(
                databaseName: "TestDb",
                collectionName: "DemoCollection",
                ConnectionStringSetting = "CosmosDbConnection")] IAsyncCollector<DataItem> documents)
        {
            try
            {
                var document = new DataItem
                {
                    id = Guid.NewGuid().ToString(),
                    Description = description,
                };

                await documents.AddAsync(document);

                return new OkObjectResult("Document inserted successfully");
            }
            catch
            {
                Console.WriteLine("Failed to connect to CosmosDb.");
                return new NoContentResult();
            }
        }

        [FunctionName("CosmosDbBinding")]
        public static void BindToCosmosDb([CosmosDBTrigger(
            databaseName: "TestDb",
            collectionName: "DemoCollection",
            ConnectionStringSetting = "CosmosDbConnection",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                Console.WriteLine($"Documents modified {input.Count}");
                Console.WriteLine($"First document Id {input[0].Id}");
            }
        }
    }
}
