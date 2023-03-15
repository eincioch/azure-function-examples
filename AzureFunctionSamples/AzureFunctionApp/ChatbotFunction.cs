using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionApp
{
    public class ChatbotFunction
    {
        private readonly ILogger _logger;

        public ChatbotFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ChatbotFunction>();
        }

        [Function("Chatbot")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, string name)
        {
            _logger.LogInformation("Message received.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            string responseMessage = string.IsNullOrEmpty(name)
                ? "Hello. What is your name?"
                : $"Hello, {name}. What can I help you with?";

            response.WriteString(responseMessage);

            return response;
        }
    }
}
