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

    private string? MatchedEndpoint(string requestRaw, out Dictionary<string, string> variables)
    {
        // A request like /media/1 is matched against endpoints like /media/{mediaId}
        string? bestKey = null;
        Dictionary<string, string> bestVariables = [];
    
        foreach (KeyValuePair<string, IApiEndpoint> endpoint in _endpointHandler.Endpoints)
        {
            Dictionary<string, string> foundVariables = [];
            
            // potentially very slow...
            string[] requestRawSplit = requestRaw.Trim('/').Split('/');
            string[] endpointSplit = endpoint.Key.Trim('/').Split('/');

            // No bother in checking because they arent even the same length
            if (endpointSplit.Length != requestRawSplit.Length) continue;

            for (int i = 0; i < endpointSplit.Length; i++)
            {
                string part = endpointSplit[i];
                if (!(part.StartsWith('{') && part.EndsWith('}'))) continue;

                // "{mediaId}" -> "mediaId"
                string name = part[1..^1];
                foundVariables[name] = requestRawSplit[i];

                // swap the value for the placeholder so the strings can match
                requestRawSplit[i] = part;
            }

            if (endpoint.Key.Trim('/') != string.Join("/", requestRawSplit)) continue;

            // Prefer the route with the fewest variables (/media/search over /media/{mediaId})
            // I don't think its required anywhere in the API spec but better safe than sorry
            if (bestKey == null || foundVariables.Count < bestVariables.Count)
            {
                bestKey = endpoint.Key;
                bestVariables = foundVariables;
            }
        }

        variables = bestVariables;
        return bestKey;
    }

    private async Task HandleResponse(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        string endpointPath = $"{request.HttpMethod} {request.Url!.LocalPath}";
        string? apiRequest = MatchedEndpoint(endpointPath, out Dictionary<string, string> endpointVariables);
        JsonElement url = ParametersUrl(request, endpointVariables);
        JsonElement body = ParametersBody(request);

        string responseString = $"<p>Post</p><pre>{body}</pre><p>Get</p><pre>{url}</pre>";
        if (apiRequest != null)
            _endpointHandler.Call(apiRequest, url, body);
        else
            responseString = "endpoint not found";
        
        Console.WriteLine(responseString);

        byte[] responseBuffer = Encoding.Latin1.GetBytes(responseString); // Latin1 for german
        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
