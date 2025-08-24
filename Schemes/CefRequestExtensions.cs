using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xilium.CefGlue;

namespace ReactAspNetAvalonia.Schemes;

public static class CefRequestExtensions
{
    /// <summary>
    /// Converts a CefRequest to an HttpRequestMessage for in-process HTTP handling.
    /// Supports GET, POST, PUT, DELETE, PATCH, OPTIONS, HEAD.
    /// </summary>
    public static HttpRequestMessage ToHttpRequestMessage(this CefRequest cefRequest)
    {
        if (cefRequest == null)
        {
            throw new ArgumentNullException(nameof(cefRequest));
        }

        // Map Cef method to HttpMethod
        HttpMethod method = new HttpMethod(cefRequest.Method.ToUpperInvariant());

        // Convert app:// URL to dummy http://localhost/ for in-process HttpClient
        // var uriString = cefRequest.Url.Replace("app://", $"http://{Guid.NewGuid():n}/");
        // Uri uri = new Uri(uriString);
        var cefUri = new Uri(cefRequest.Url);
        var uri = new Uri(cefUri.PathAndQuery, UriKind.Relative);
        
        var requestMessage = new HttpRequestMessage(method, uri);

        // Copy headers from CefRequest

        var headers = cefRequest.GetHeaderMap();

        foreach (string key in headers.Keys)
        {
            string value = headers[key]!;

            // Try adding to request headers first
            if (!requestMessage.Headers.TryAddWithoutValidation(key, value))
            {
                // If failed, try adding to content headers
                if (requestMessage.Content == null)
                {
                    requestMessage.Content = new ByteArrayContent(Array.Empty<byte>());
                }

                requestMessage.Content.Headers.TryAddWithoutValidation(key, value);
            }
        }

        // Handle body for POST, PUT, PATCH
        if (method == HttpMethod.Post || method == HttpMethod.Put || method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
        {
            byte[] bodyBytes = Array.Empty<byte>();

            if (cefRequest.PostData != null)
            {
                var elements = cefRequest.PostData.GetElements();
                if (elements.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        foreach (var element in elements)
                        {
                            if (element.ElementType == CefPostDataElementType.Bytes)
                            {
                                // Get the bytes from the element
                                byte[] elementBytes = element.GetBytes();
                                if (elementBytes != null)
                                {
                                    memoryStream.Write(elementBytes, 0, elementBytes.Length);
                                }
                            }
                            else if (element.ElementType == CefPostDataElementType.File)
                            {
                                // Handle file uploads if needed
                                string fileName = element.GetFile();
                                if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                                {
                                    byte[] fileBytes = File.ReadAllBytes(fileName);
                                    memoryStream.Write(fileBytes, 0, fileBytes.Length);
                                }
                            }
                        }

                        bodyBytes = memoryStream.ToArray();
                    }
                }
            }

            requestMessage.Content = new ByteArrayContent(bodyBytes);

            // Copy Content-Type header if provided
            var contentTypeHeader = headers.Keys.Cast<string>().FirstOrDefault(k => k!.ToString().Equals("Content-Type", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(contentTypeHeader))
            {
                requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", headers[contentTypeHeader]);
            }
        }

        return requestMessage;
    }
}