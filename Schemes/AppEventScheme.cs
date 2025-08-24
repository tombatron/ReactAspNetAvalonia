using Xilium.CefGlue.Common.Shared;

namespace ReactAspNetAvalonia.Schemes;

public class AppEventScheme : CustomScheme
{
    public AppEventScheme()
    {
        SchemeName = "app";
        IsStandard = true;
        IsLocal = true;
        IsCorsEnabled = true;
        IsCSPBypassing = true;
        IsFetchEnabled = true;
        SchemeHandlerFactory = new AppEventSchemeHandler();
    }
}