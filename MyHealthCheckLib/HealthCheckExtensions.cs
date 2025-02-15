using System;
using Microsoft.Extensions.Hosting;

namespace MyHealthCheckLib;

public static class HealthCheckExtensions
{
    public static IHost UseHealthCheckServer(this IHost host, int port = 8080)
    {
        var server = new HealthCheckServer(port);
        server.Start();

        AppDomain.CurrentDomain.ProcessExit += (s, e) => server.Stop();

        return host;
    }
}
