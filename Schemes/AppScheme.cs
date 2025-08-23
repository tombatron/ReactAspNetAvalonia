using Xilium.CefGlue.Common.Shared;

namespace ReactAspNetAvalonia.Schemes;

public class AppScheme : CustomScheme
{
    public AppScheme()
    {
        SchemeName = "app";
        IsStandard = true;
        IsLocal = true;
        IsCorsEnabled = true;
        IsCSPBypassing = true;
        IsFetchEnabled = true;
        SchemeHandlerFactory = new AppSchemeHandler();
    }
}