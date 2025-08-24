using System;
using System.IO;
using Avalonia.Controls;
using Xilium.CefGlue.Avalonia;

namespace ReactAspNetAvalonia.Controls;

public partial class BrowserView : UserControl
{
    private AvaloniaCefBrowser _browser;

    public BrowserView() : this("index")
    {
    }

    public BrowserView(string entryView)
    {
        InitializeComponent();

        var browserWrapper = this.FindControl<Decorator>("BrowserWrapper");

        var entryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "index.html");

        // Create a proper file:// URI
        var fileUri = new Uri(entryFile);

        // Use UriBuilder to append the query string
        var uriBuilder = new UriBuilder(fileUri)
        {
            Query = $"viewName={Uri.EscapeDataString(entryView)}"
        };

        _browser = new AvaloniaCefBrowser
        {
            Address = uriBuilder.Uri.AbsoluteUri
        };

        browserWrapper!.Child = _browser;
    }
}