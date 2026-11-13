using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL.Api;

namespace Swe.MRP.BL;

public class MRPServer(string httpUrl, ServiceProvider serviceProvider)
{
    private readonly HttpListener _listener = new();
    private readonly string HttpUrl = httpUrl;
    private Task? _serverTask;
    private readonly EndpointRegistrar _endpointRegistrar = serviceProvider.GetRequiredService<EndpointRegistrar>();

    public void Start()
    {
        _listener.Prefixes.Add(HttpUrl);
        _listener.Start();
        Console.WriteLine($"Server gestartet auf: {HttpUrl}");
        _serverTask = ServerLoop();
    }

    public void WaitForTermination()
    {
        _serverTask?.Wait();
    }

    private async Task ServerLoop()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleResponse(context));
        }
    }

    private async Task HandleResponse(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        Dictionary<string, string> parameters = [];
        foreach (string? key in request.QueryString.AllKeys)
        {
            if (key == null) continue;
            parameters.Add(key, request.QueryString[key] ?? "");
        }

        string body = "";
        if (request.HasEntityBody)
        {
            using StreamReader reader = new(request.InputStream, request.ContentEncoding);
            body = await reader.ReadToEndAsync();
        }
        
        if (!string.IsNullOrWhiteSpace(body))
        {
            Dictionary<string, JsonElement>? bodyDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);
            if (bodyDict != null)
            {
                foreach (KeyValuePair<string, JsonElement> entry in bodyDict)
                {
                    parameters.TryAdd(entry.Key, entry.Value.ToString());
                }
            }
        }
            
        string apiRequest = request.Url!.LocalPath;
        _endpointRegistrar.Handle(apiRequest, parameters);

        string responseString = "";
        foreach (KeyValuePair<string, string> entry in parameters)
        {
            responseString += $"<p>{entry.Key}: {entry.Value}</p>";
        }
        Console.WriteLine(responseString);

        byte[] responseBuffer = Encoding.Latin1.GetBytes(responseString); // Latin1 for german
        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
