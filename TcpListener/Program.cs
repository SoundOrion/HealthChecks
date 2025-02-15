using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// HTTP を使わず、ポートが開いているかどうかをヘルスチェックする方法
/// </summary>
class Program
{
    static async Task Main()
    {
        var listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("TCP health check server is running on port 8080");

        while (true)
        {
            using var client = await listener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            byte[] responseBytes = Encoding.UTF8.GetBytes("OK\n");
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}
