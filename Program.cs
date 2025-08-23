using Avalonia;
using System;
using System.IO;
using ReactAspNetAvalonia.Schemes;
using ReactAspNetAvalonia.Server;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace ReactAspNetAvalonia;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (InProcessServer.IsRunning)
        {
            return;
        }
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        var cachePath = Path.Combine(Path.GetTempPath(), "CefGlue_" + Guid.NewGuid().ToString().Replace("-", null));

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions())
            .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings()
                {
                    CachePath = "",
                    //RootCachePath = cachePath,
                    WindowlessRenderingEnabled = false
                },
                customSchemes:
                [
                    new AppScheme()
                ]));
    }
}