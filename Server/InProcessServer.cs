using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ReactAspNetAvalonia.EventHubs;
using ReactAspNetAvalonia.Services;

namespace ReactAspNetAvalonia.Server;

public static class InProcessServer
{
    private static TestServer? _server;
    private static HttpClient? _client;
    public static bool IsRunning { get; private set; }

    public static void Start()
    {
        if (_server is not null)
        {
            return;
        }
        
        IsRunning = true; // prevent Avalonia from starting

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITimeService, TimeService>();
                services.AddSingleton<ITodoStorage, TodoStorage>();
                
                services.AddControllers(); 
                
            })
            .Configure(app =>
            {
                app.UseRouting();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<AppEventHub>("/events");
                });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    public static HttpClient GetClient()
    {
        if (_client is null)
        {
            Start();
        }

        return _client!;
    }
}