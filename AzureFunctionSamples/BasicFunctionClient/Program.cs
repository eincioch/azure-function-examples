using System.Net.Http.Headers;

using HttpClient client = new();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("text/plain; charset=utf-8"));

var response = await client.GetStringAsync(
         "http://localhost:7000/api/Function1");

Console.Write(response);