using System;
using System.IO;
using System.Net.Http;
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

public class AppSchemeResourceHandler : CefResourceHandler
{
    private readonly HttpClient _client;

    private Stream? _responseStream;
    private int _statusCode = 200;
    private string _mimeType = "application/json";
    private long _responseLength = 0;

    public AppSchemeResourceHandler(HttpClient client)
    {
        _client = client;
    }

    protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
    {
        handleRequest = request.Url.StartsWith("app://api", StringComparison.OrdinalIgnoreCase);

        if (!handleRequest)
        {
            return false;
        }

        Console.WriteLine($"[Open] URL={request.Url}, handle={handleRequest}");

        // Capture callback immediately
        CefCallback capturedCallback = callback;

        // Start async fetch
        Task.Run(async () =>
        {
            try
            {
                Uri uri = new Uri(request.Url.Replace("app://", "http://localhost/"));

                HttpResponseMessage response = await _client.GetAsync(uri.PathAndQuery, HttpCompletionOption.ResponseContentRead);

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();

                _responseStream = new MemoryStream(bytes, writable: false);
                _responseStream.Position = 0;
                _responseLength = _responseStream.Length;
                _statusCode = (int)response.StatusCode;
                _mimeType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

                Console.WriteLine($"[FetchAndPrepareAsync] Ready, length={_responseLength}, status={_statusCode}");

                // Continue on UI thread
                CefRuntime.PostTask(CefThreadId.UI, new CefActionTask(() =>
                {
                    capturedCallback.Continue();
                    capturedCallback.Dispose();
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FetchAndPrepareAsync] ERROR: {ex}");

                _responseStream = new MemoryStream(Array.Empty<byte>());
                _responseLength = 0;
                _statusCode = 500;
                _mimeType = "application/json";

                CefRuntime.PostTask(CefThreadId.UI, new CefActionTask(() =>
                {
                    capturedCallback.Continue();
                    capturedCallback.Dispose();
                }));
            }
        });

        return true; // async, will call Continue() later
    }

    protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
    {
        redirectUrl = string.Empty;

        response.Status = _statusCode;
        response.MimeType = _mimeType;

        if (_responseStream != null)
        {
            responseLength = _responseLength;
        }
        else
        {
            responseLength = -1; // unknown length; CEF will call Read until EOF
        }

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

            // Non-seekable: read and discard
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

        if (_responseStream == null)
        {
            // Wait briefly for async fetch to complete
            int waitMs = 0;
            while (_responseStream == null && waitMs < 1000) // max 1 second
            {
                Task.Delay(5).Wait();
                waitMs += 5;
            }

            if (_responseStream == null)
            {
                Console.WriteLine("[Read] Stream still not ready, returning false");
                return false;
            }
        }

        byte[] buffer = new byte[bytesToRead];
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
        if (_responseStream != null)
        {
            _responseStream.Dispose();
            _responseStream = null;
        }

        Console.WriteLine("[Cancel] Called");
    }
}