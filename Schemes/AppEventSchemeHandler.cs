using System;
using ReactAspNetAvalonia.Server;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public class AppEventSchemeHandler : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        Console.WriteLine($"[AppEventSchemeHandler] Creating handler for {request.Url}");
        return new AppEventSchemeResourceHandler(InProcessServer.GetClient());
    }
}