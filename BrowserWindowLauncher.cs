using Avalonia.Controls;
using Avalonia.Threading;
using ReactAspNetAvalonia.Controls;

namespace ReactAspNetAvalonia;

public static class BrowserWindowLauncher
{
    public static void OpenBrowserWindow(string htmlFileName, string title = "Browser Window", int width = 800, int height = 600)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Create a new window
            var window = new Window
            {
                Title = title,
                Width = width,
                Height = height,
            };

            // Create your BrowserView with the specified entry file
            var browserView = new BrowserView(htmlFileName);

            // Set the BrowserView as the content of the window
            window.Content = browserView;

            // Show the window
            window.Show();
        });
    }
}