using Xilium.CefGlue.Common.Shared;

namespace ReactAspNetAvalonia.Schemes;

public class AppApiScheme : CustomScheme
{
    public AppApiScheme()
    {
        SchemeName = "app";
        DomainName = "api";
        IsStandard = true;
        IsLocal = true;
        IsCorsEnabled = true;
        IsCSPBypassing = true;
        IsFetchEnabled = true;
        SchemeHandlerFactory = new AppApiSchemeHandler();
    }
}