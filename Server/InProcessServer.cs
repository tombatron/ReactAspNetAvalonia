using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ReactAspNetAvalonia.Server;

public static class InProcessServer
{
    public static bool IsRunning { get; set; } = false;

    public static IWebHostBuilder CreateHost()
    {
        IsRunning = true; // prevent Avalonia from starting

        var builder = new WebHostBuilder()
            .ConfigureServices(services => { services.AddControllers(); })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

        return builder;
    }
}