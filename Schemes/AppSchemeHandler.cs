using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

public class AppSchemeResourceHandler(HttpClient client) : CefResourceHandler
{
    private MemoryStream? _responseStream;
    private int _statusCode = 200;
    private string _mimeType = "application/json";

    // Signal when the async fetch is complete
    private readonly ManualResetEventSlim _readyEvent = new(false);

    protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
    {
        handleRequest = request.Url.StartsWith("app://api", StringComparison.OrdinalIgnoreCase);

        if (!handleRequest)
        {
            return false;
        }

        Console.WriteLine($"[Open] URL={request.Url}, handle={handleRequest}");

        // Capture the callback
        CefCallback capturedCallback = callback;

        // Start async fetch
        Task.Run(async () =>
        {
            try
            {
                Uri uri = new Uri(request.Url.Replace("app://", "http://localhost/"));

                HttpResponseMessage response = await client.GetAsync(uri.PathAndQuery, HttpCompletionOption.ResponseContentRead);

                byte[] data = await response.Content.ReadAsByteArrayAsync();

                _responseStream = new MemoryStream(data, writable: false);
                _responseStream.Position = 0;
                _statusCode = (int)response.StatusCode;
                _mimeType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

                Console.WriteLine($"[FetchAndPrepareAsync] Ready, length={_responseStream.Length}, status={_statusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FetchAndPrepareAsync] ERROR: {ex}");
                _responseStream = new MemoryStream(Array.Empty<byte>());
                _statusCode = 500;
                _mimeType = "application/json";
            }
            finally
            {
                // Signal ready for Read/GetResponseHeaders
                _readyEvent.Set();

                // Tell CEF headers and data are ready
                CefRuntime.PostTask(CefThreadId.IO, new CefActionTask(() =>
                {
                    capturedCallback.Continue();
                    capturedCallback.Dispose();
                }));
            }
        });

        return true; // async
    }

    protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
    {
        redirectUrl = string.Empty;

        // Wait until fetch is complete
        _readyEvent.Wait();

        response.Status = _statusCode;
        response.MimeType = _mimeType;

        responseLength = _responseStream?.Length ?? -1;

        // CORS headers
        response.SetHeaderByName("Access-Control-Allow-Origin", "*", false);
        response.SetHeaderByName("Access-Control-Allow-Methods", "GET, POST, OPTIONS", false);
        response.SetHeaderByName("Access-Control-Allow-Headers", "Content-Type", false);

        Console.WriteLine($"[GetResponseHeaders] Status={response.Status}, MimeType={response.MimeType}, Length={responseLength}");
    }

    protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
    {
        bytesSkipped = 0;

        if (_responseStream == null)
        {
            return false;
        }

        try
        {
            if (_responseStream.CanSeek)
            {
                long oldPos = _responseStream.Position;
                long newPos = _responseStream.Seek(bytesToSkip, SeekOrigin.Current);
                bytesSkipped = newPos - oldPos;
                return true;
            }

            // Non-seekable fallback
            const int chunk = 32 * 1024;
            long remaining = bytesToSkip;
            byte[] buffer = new byte[chunk];
            while (remaining > 0)
            {
                int toRead = (int)Math.Min(buffer.Length, remaining);
                int read = _responseStream.Read(buffer, 0, toRead);
                if (read <= 0)
                {
                    break;
                }

                remaining -= read;
                bytesSkipped += read;
            }

            return true;
        }
        catch
        {
            bytesSkipped = 0;
            return false;
        }
    }

    protected override bool Read(Stream response, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
    {
        bytesRead = 0;

        // Wait until the response is ready
        _readyEvent.Wait();

        if (_responseStream == null)
        {
            Console.WriteLine("[Read] No stream available");
            return false;
        }

        byte[] buffer = new byte[bytesToRead];
        bytesRead = _responseStream.Read(buffer, 0, bytesToRead);

        if (bytesRead > 0)
        {
            response.Write(buffer, 0, bytesRead);
            return true;
        }

        return false; // EOF
    }

    protected override void Cancel()
    {
        if (_responseStream != null)
        {
            _responseStream.Dispose();
            _responseStream = null;
        }

        _readyEvent.Set();
        Console.WriteLine("[Cancel] Called");
    }
}