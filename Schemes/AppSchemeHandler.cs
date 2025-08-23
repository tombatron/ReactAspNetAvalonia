using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public class AppSchemeHandler : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
    {
        return new AppSchemeResourceHandler();
    }
}

public class AppSchemeResourceHandler : CefResourceHandler
{

    private Stream? _responseStream;
    private static int _counter = 0;
    
    protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
    {
        var requestUrl = request.Url.ToLowerInvariant();

        if (requestUrl.StartsWith("app://"))
        {
            handleRequest = true;

            // Simple JSON body
            var json = JsonSerializer.Serialize(new { message = $"Hello #{++_counter}" });
            var bytes = Encoding.UTF8.GetBytes(json);

            _responseStream = new MemoryStream(bytes);

            // tell CEF we’re ready
            callback.Continue();
            return true;
        }

        handleRequest = false;
        return false;
    }

    protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
    {
        response.Status = 200;
        response.MimeType = "application/json";
        responseLength = _responseStream!.Length;
        redirectUrl = string.Empty;
        response.SetHeaderByName("Access-Control-Allow-Origin", "*", false);
        response.SetHeaderByName("Access-Control-Allow-Methods", "GET, POST, OPTIONS", false);
        response.SetHeaderByName("Access-Control-Allow-Headers", "Content-Type", false);
    }

    protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
    {
        bytesSkipped = 0;

        if (_responseStream is null)
        {
            return false;
        }
        
        var oldPosition = _responseStream.Position;
        var newPosition = _responseStream.Seek(bytesToSkip, SeekOrigin.Current);
        bytesSkipped = newPosition - oldPosition;

        return bytesSkipped > 0;
    }

    protected override bool Read(Stream response, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
    {
        bytesRead = 0;

        if (_responseStream is null)
        {
            return false;
        }
        
        var buffer = new byte[bytesToRead];
        bytesRead = _responseStream.Read(buffer, 0, bytesToRead);

        if (bytesRead > 0)
        {
            response.Write(buffer, 0, bytesRead);
            return true;
        }

        return false;
    }

    protected override void Cancel()
    {
        _responseStream?.Dispose();
        _responseStream = null;
    }
    
    // protected override void Dispose(bool disposing)
    // {
    //     _responseStream?.Dispose();
    //     base.Dispose(disposing);
    // }

}