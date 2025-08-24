using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ReactAspNetAvalonia.Server;
using Xilium.CefGlue;
using static ReactAspNetAvalonia.Schemes.SchemeConstants;

namespace ReactAspNetAvalonia.Schemes;

public class AppEventSchemeResourceHandler(HttpClient client) : CefResourceHandler
{
    private Stream? _responseStream;
    private int _statusCode = 200;
    private string _mimeType = "text/event-stream"; // Default for SSE
    
    // Signal when the async fetch is complete
    private readonly ManualResetEventSlim _readyEvent = new(false);

    protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
    {
        handleRequest = request.Url.StartsWith("app://events", StringComparison.OrdinalIgnoreCase);

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
                var requestMessage = request.ToHttpRequestMessage();
                HttpResponseMessage response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                _responseStream = await response.Content.ReadAsStreamAsync();
                _responseStream.Position = 0;
                _statusCode = (int)response.StatusCode;
                _mimeType = response.Content.Headers.ContentType?.MediaType ?? "text/event-stream";

                Console.WriteLine($"[FetchAndPrepareAsync] Ready, length={_responseStream.Length}, status={_statusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FetchAndPrepareAsync] ERROR: {ex}");
                _responseStream = new MemoryStream([]);
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

        responseLength = UnknownLength;

        // CORS headers
        response.SetHeaderByName("Access-Control-Allow-Origin", "*", false);
        response.SetHeaderByName("Cache-Control", "no-cache", false);
        response.SetHeaderByName("Connection", "keep-alive", false);

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

        try
        {
            byte[] buffer = new byte[bytesToRead];
            int read  = _responseStream.Read(buffer, 0, bytesToRead);

            if (bytesRead > 0)
            {
                response.Write(buffer, 0, bytesRead);
                bytesRead = read;
                return true;
            }

            return false; // EOF
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Read] ERROR: {ex}");
            return false;
        }
    }

    protected override void Cancel()
    {
        _responseStream?.Dispose();
        _responseStream = null;
        _readyEvent.Set();
        Console.WriteLine("[Cancel] Called");
    }
}