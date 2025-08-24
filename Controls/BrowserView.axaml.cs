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

    public BrowserView(string entryFileName)
    {
        var entryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", $"{entryFileName}.html");
        var entryFileUri = new Uri(entryFile);
        
        InitializeComponent();
        
        var browserWrapper = this.FindControl<Decorator>("BrowserWrapper");
        
        _browser = new AvaloniaCefBrowser();
        _browser.Address = entryFileUri.AbsoluteUri;

        browserWrapper!.Child = _browser;
    }
}