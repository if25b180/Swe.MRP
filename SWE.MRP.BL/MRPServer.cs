using System.Net;
using System.Text;
using Swe.MRP.DL;
using static System.Net.WebRequestMethods;

namespace SWE.MRP.BL;

public class MRPServer(string httpUrl)
{
    private HttpListener _listener { get; } = new HttpListener();
    private string HttpUrl { get; } = httpUrl;
    private Database? _database;
    private Task? _serverTask;

    public void Start()
    {
        _listener.Prefixes.Add(HttpUrl);
        _listener.Start();
        Console.WriteLine($"Server gestartet auf: {HttpUrl}");
        _serverTask = ServerLoop();
    }

    public void ConnectDatabase(string connectionString)
    {
        _database = new Database(connectionString);
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

        string responseString = rnd.Next(1, 101).ToString();
        byte[] responseBuffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = responseBuffer.Length;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(responseBuffer.AsMemory(0, responseBuffer.Length));
        response.Close();
    }
}
