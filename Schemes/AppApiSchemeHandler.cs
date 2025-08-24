using System;
using Microsoft.AspNetCore.TestHost;
using ReactAspNetAvalonia.Server;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public class AppApiSchemeHandler : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        Console.WriteLine($"[AppApiSchemeHandler] Creating handler for {request.Url}");
        return new AppApiSchemeResourceHandler(InProcessServer.GetClient());
    }
}