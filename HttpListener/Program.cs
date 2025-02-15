using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///  最もシンプルな HTTP ベースのヘルスチェック 
/// </summary>
class Program
{
    static async Task Main()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        Console.WriteLine("Health check server is running on http://localhost:8080/health");
        Console.WriteLine("Press Ctrl+C to stop.");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        string responseText = "OK";
        int statusCode = 200;

        if (context.Request.RawUrl == "/health")
        {
            responseText = "Healthy";
            statusCode = 200;
        }
        else
        {
            responseText = "Not Found";
            statusCode = 404;
        }

        var response = context.Response;
        response.StatusCode = statusCode;
        byte[] buffer = Encoding.UTF8.GetBytes(responseText);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/plain";

        using (var output = response.OutputStream)
        {
            output.Write(buffer, 0, buffer.Length);
        }

        Console.WriteLine($"[{DateTime.Now}] {context.Request.RawUrl} => {statusCode}");
    }
}
