using Avalonia;
using System;
using System.IO;
using ReactAspNetAvalonia.Schemes;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace ReactAspNetAvalonia;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    
    private static AppBuilder BuildAvaloniaApp()
    {
        var cachePath = Path.Combine(Path.GetTempPath(), "CefGlue_" + Guid.NewGuid().ToString().Replace("-", null));

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions())
            .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings()
                {
                    RootCachePath = cachePath,
                    WindowlessRenderingEnabled = false
                },
                customSchemes:
                [
                    new AppScheme()
                ]));
    }
}