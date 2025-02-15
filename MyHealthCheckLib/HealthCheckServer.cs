using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        Console.WriteLine($"[INFO] {DateTime.UtcNow:O} - HealthCheckServer running on http://localhost:{_listener.Prefixes}");

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
        string ipAddress = context.Request.RemoteEndPoint?.Address.ToString() ?? "Unknown";

        //// IPv6 の `::1` を `127.0.0.1` に変換
        //if (ipAddress == "::1")
        //{
        //    ipAddress = "127.0.0.1";
        //}

        int statusCode;
        string responseJson;

        if (context.Request.RawUrl == "/")
        {
            // ルート (`/`) にアクセスした場合、簡単なメッセージを返す
            responseJson = JsonSerializer.Serialize(new
            {
                message = "Welcome to HealthCheck Server!",
                endpoints = new string[] { "/health", "/metrics" },
                timestamp = DateTime.UtcNow.ToString("O"),
                ip = ipAddress
            });
            statusCode = 200;
        }
        else if (context.Request.RawUrl == "/health")
        {
            var healthResponse = new HealthCheckResponse(ipAddress, "Healthy");
            responseJson = JsonSerializer.Serialize(healthResponse);
            statusCode = 200;
        }
        else if (context.Request.RawUrl == "/metrics")
        {
            responseJson = JsonSerializer.Serialize(new
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                ip = ipAddress,
                metrics = new { custom_metric_count = 42 }
            });
            statusCode = 200;
        }
        else
        {
            var errorResponse = new HealthCheckResponse(ipAddress, "Not Found", "Invalid endpoint");
            responseJson = JsonSerializer.Serialize(errorResponse);
            statusCode = 404;
        }

        var response = context.Response;
        response.StatusCode = statusCode;
        byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "application/json"; // JSON レスポンス

        using (var output = response.OutputStream)
        {
            output.Write(buffer, 0, buffer.Length);
        }

        // ログ出力
        Console.WriteLine($"[INFO] {DateTime.UtcNow:O} - IP: {ipAddress}, URL: {context.Request.RawUrl}, Status: {statusCode}");
    }

    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
        Console.WriteLine($"[INFO] {DateTime.UtcNow:O} - HealthCheckServer stopped.");
    }
}

public class HealthCheckResponse
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("ip")]
    public string IpAddress { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; }

    public HealthCheckResponse(string ipAddress, string status, string note = "System is operational")
    {
        Timestamp = DateTime.UtcNow.ToString("O"); // ISO 8601 形式
        IpAddress = ipAddress;
        Status = status;
        Note = note;
    }
}
