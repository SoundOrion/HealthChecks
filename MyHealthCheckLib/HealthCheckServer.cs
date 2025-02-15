using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHealthCheckLib;

public class HealthCheckServer
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public HealthCheckServer(int port)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine($"HealthCheckServer running on http://localhost:{_listener.Prefixes}");

        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
        });
    }

    private void HandleRequest(HttpListenerContext context)
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

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        Console.WriteLine("HealthCheckServer stopped.");
    }
}
