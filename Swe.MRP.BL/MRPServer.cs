using System.Net;
using System.Text;
using Swe.MRP.DL;

namespace Swe.MRP.BL;

public class MRPServer(string httpUrl)
{
    private readonly HttpListener _listener = new();
    private readonly string HttpUrl = httpUrl;
    private Database? _database;
    private Task? _serverTask;

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
            var context = await _listener.GetContextAsync();
            await HandleResponse(context, rnd);
        }
    }

    private static async Task HandleResponse(HttpListenerContext context, Random rnd)
    {
        HttpListenerResponse response = context.Response;

        string responseString = 
            "<p>Deine Glückszahl ist: " + rnd.Next(1, 101).ToString() + "</p>"
            + "<style>*{background-color:black; color:white;}</style>"
        ;
        byte[] responseBuffer = Encoding.Latin1.GetBytes(responseString); // Latin1 for german

        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
