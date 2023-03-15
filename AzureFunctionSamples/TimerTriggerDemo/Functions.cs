using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TimerTriggerDemo
{
    public class Functions
    {
        [FunctionName("TimerDemo")]
        public void Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            Console.WriteLine($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
