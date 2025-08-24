using System;
using Microsoft.AspNetCore.TestHost;
using ReactAspNetAvalonia.Server;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public class AppSchemeHandler : CefSchemeHandlerFactory
{
    private readonly TestServer _server = new(InProcessServer.CreateHost());

    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        Console.WriteLine($"[AppSchemeHandler] Creating handler for {request.Url}");
        return new AppSchemeResourceHandler(_server.CreateClient());
    }
}