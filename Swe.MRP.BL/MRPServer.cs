using System.Net;
using System.Text;
using Swe.MRP.BL.Api;
using Swe.MRP.DL;

namespace Swe.MRP.BL;

public class MRPServer(string httpUrl, EndpointRegistrar endpointRegistrar)
{
    private readonly HttpListener _listener = new();
    private readonly string HttpUrl = httpUrl;
    private Database? _database;
    private Authentication? _authentication;
    private Task? _serverTask;
    private readonly EndpointRegistrar _endpointRegistrar = endpointRegistrar;

    public void Start()
    {
        _listener.Prefixes.Add(HttpUrl);
        _listener.Start();
        Console.WriteLine($"Server gestartet auf: {HttpUrl}");
        _serverTask = ServerLoop();
    }

    public void InitDatabase(string connectionString)
    {
        _database = new Database(connectionString);
        _database.ApplyMissingSchemas();

        _authentication = new Authentication(_database);
    }

    public void WaitForTermination()
    {
        _serverTask?.Wait();
    }

    private async Task ServerLoop()
    {
        Random rnd = new();

        while (_listener.IsListening)
        {
            HttpListenerContext context = await _listener.GetContextAsync();
            await HandleResponse(context, rnd);
        }
    }

    private async Task HandleResponse(HttpListenerContext context, Random rnd)
    {
        HttpListenerResponse response = context.Response;
        HttpListenerRequest request = context.Request;

        string apiRequest = context.Request.Url.LocalPath;
        _endpointRegistrar.Handle(apiRequest);

        string responseString = $@"
            <p>Deine Glückszahl ist: {rnd.Next(1, 101)}</p>
            <style>
                *{{
                    background-color:black; color:white;
                }}
            </style>
        "
        ;
        byte[] responseBuffer = Encoding.Latin1.GetBytes(responseString); // Latin1 for german

        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
