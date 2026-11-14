using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Swe.MRP.BL;

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

    private static JsonElement ParametersGet(HttpListenerRequest request)
    {
        Dictionary<string, string> parameters = [];

        foreach (string? key in request.QueryString.AllKeys)
        {
            if (key == null) continue;
            parameters[key] = request.QueryString[key] ?? "";
        }

        return JsonSerializer.SerializeToElement(parameters);
    }

    private static JsonElement ParametersPost(HttpListenerRequest request)
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
        
        string apiRequest = request.Url!.LocalPath;
        JsonElement get = ParametersGet(request);
        JsonElement post = ParametersPost(request);
        _endpointHandler.Call(apiRequest, get, post);

        // Debug vomit
        string responseString = $"<p>Post</p><pre>{post}</pre><p>Get</p><pre>{get}</pre>";
        Console.WriteLine(responseString);

        byte[] responseBuffer = Encoding.Latin1.GetBytes(responseString); // Latin1 for german
        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
