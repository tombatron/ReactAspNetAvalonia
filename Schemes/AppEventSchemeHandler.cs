using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public class AppEventSchemeHandler : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        throw new System.NotImplementedException();
    }
}