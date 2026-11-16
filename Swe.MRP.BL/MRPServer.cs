using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL;
using Swe.MRP.BL.Abstractions.Api;

namespace Swe.MRP.BL;

public class MRPServer(string httpUrl, ServiceProvider serviceProvider)
{
    private readonly HttpListener _listener = new();
    private readonly string HttpUrl = httpUrl;
    private Task? _serverTask;
    private readonly EndpointHandler _endpointHandler = serviceProvider.GetRequiredService<EndpointHandler>();

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

    private static JsonElement ParametersUrl(HttpListenerRequest request, Dictionary<string, string>? additional = null)
    {
        Dictionary<string, string> parameters = [];

        foreach (string? key in request.QueryString.AllKeys)
        {
            if (key == null) continue;
            parameters[key] = request.QueryString[key] ?? "";
        }

        if (additional != null)
        {
            foreach (KeyValuePair<string, string> pair in additional)
            {
                parameters[pair.Key] = pair.Value;
            }
        }

        return JsonSerializer.SerializeToElement(parameters);
    }

    private static JsonElement ParametersBody(HttpListenerRequest request)
    {
        string body = "";
        if (request.HasEntityBody)
        {
            using StreamReader reader = new(request.InputStream, request.ContentEncoding);
            body = reader.ReadToEnd();
        }

        if (string.IsNullOrWhiteSpace(body))
            return JsonSerializer.SerializeToElement(new { });

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone(); // Clone, because doc is disposed at the end of the method
        }
        catch (JsonException)
        {
            Console.WriteLine("WARN: Client hat invalides Body gesendet");
            return JsonSerializer.SerializeToElement(new { });
        }
    }

    private async Task HandleResponse(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        string endpointPath = $"{request.HttpMethod} {request.Url!.LocalPath}";
        string? apiRequest = _endpointHandler.Match(endpointPath, out Dictionary<string, string> endpointVariables);
        JsonElement url = ParametersUrl(request, endpointVariables);
        JsonElement body = ParametersBody(request);

        string responseString = $@"
            <style>
                html {{
                    background-color:black;color:white;
                }}
            </style>

            <p>Body: </p>
            <pre>{body}</pre>

            <p>Url Params: </p>
            <pre>{url}</pre>
        ";

        if (apiRequest != null)
        {
            responseString += $@"
                <p>Response: </p>
                <pre>{_endpointHandler.Call(apiRequest, url, body)}</pre>
            ";
        }
        else
        {
            responseString += $"Endpoint `{endpointPath}` -> `{apiRequest}` nicht gefunden";
        }
        
        Console.WriteLine(responseString);

        byte[] responseBuffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
