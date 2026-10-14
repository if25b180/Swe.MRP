using System.Net;
using System.Text;
using Npgsql;
using Swe.MRP.DL;

namespace SWE.MRP.BL;

public class MRPServer
{
    private HttpListener _listener { get; }
    private string _httpUrl { get; set; }

    private Database? _database;

    public MRPServer(string httpUrl)
    {
        _httpUrl = httpUrl;
        _listener = new HttpListener();
    }

    public void start()
    {
        _listener.Prefixes.Add(_httpUrl);
        _listener.Start();
        Console.WriteLine($"Server gestartet auf: {_httpUrl}");

        while (true)
        {
            var context = _listener.GetContext();
            var response = context.Response;

            string responseString = "<html><body><h1>Hallo Software Engineering!</h1></body></html>";
            byte[] responseBuffer = Encoding.UTF8.GetBytes(responseString);
            
            response.ContentLength64 = responseBuffer.Length;
            response.ContentType = "text/html";
            
            response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            response.Close();
        }
    }

    public void connectDatabase(string connectionString)
    {
        _database = new Database(connectionString);
    }
}
