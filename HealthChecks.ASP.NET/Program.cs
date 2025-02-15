/// <summary>
/// ASP.NET Core の HealthChecks を使って、より本格的なヘルスチェック を実装
/// </summary>
class Program
{
    static async Task Main()
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    services.AddHealthChecks();
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHealthChecks("/health");
                    });
                });

                webBuilder.UseUrls("http://localhost:8080");
            });

        var host = builder.Build();
        await host.RunAsync();
    }
}
