using Microsoft.Extensions.Hosting;
using MyHealthCheckLib;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .Build()
            .UseHealthCheckServer(8080); // 🎯 これだけでOK!

        Console.WriteLine("Press Ctrl+C to stop.");
        await host.RunAsync();
    }
}
