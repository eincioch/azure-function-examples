using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Composition;

namespace BlobTriggerDemo
{
    public class Functions
    {
        [FunctionName("BlobOutputTrigger")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "message" })]
        [OpenApiParameter(name: "message", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Message to put in the Blob Storage")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "HTTP response")]
        [return: Queue("message-queue-demo")]
        public async Task<string> BlobOutputTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Blob("sample-blobs/HttpTriggered", FileAccess.Write)] TextWriter inputWriter)
        {
            string message = req.Query["message"];

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            message = message ?? data?.name;

            if (string.IsNullOrWhiteSpace(message))
                return null;

            await inputWriter.WriteLineAsync(message);

            return message;
        }

        [FunctionName("BlobTriggerExample")]
        public void NotifyBlobInsertion([BlobTrigger("sample-blobs/{name}", Connection = "AzureWebJobsStorage")]Stream blob, string name, ILogger log)
        {
            Console.WriteLine($"Processed a Blob with the name of {name} and the size of {blob.Length} bytes.");
        }
    }
}
