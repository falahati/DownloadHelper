using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadHelper.Tests
{
    internal class HttpServer
    {
        private bool _supportsRange = true;
        private bool _chunked;

        public bool Chunked
        {
            get { return _chunked; }
            set
            {
                _chunked = value;
                if (_chunked && SupportsRange)
                {
                    SupportsRange = false;
                }
            }
        }

        public string ContactType { get; set; } = MediaTypeNames.Application.Octet;
        public string FileName { get; set; } = null;
        public ushort Port { get; private set; } = 8081;

        public bool SupportsRange
        {
            get { return _supportsRange; }
            set
            {
                _supportsRange = value;
                if (_supportsRange && Chunked)
                {
                    Chunked = false;
                }
            }
        }

        public string Root => $"http://127.0.0.1:{Port}/";

        public async Task Start(ushort port, HttpFile file, CancellationToken token)
        {
            Port = port;
            await Start(file, token);
        }

        public async Task Start(HttpFile file, CancellationToken token)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(Root);
            listener.Start();
            // ReSharper disable once MethodSupportsCancellation
            await Task.Run(async () =>
            {
                var activeResponses = new List<Task>();
                Task<HttpListenerContext> contextTask = null;
                do
                {
                    if (contextTask == null)
                        contextTask = listener.GetContextAsync();
                    try
                    {
                        contextTask.Wait(token);
                    }
                    catch
                    {
                        // ignore
                    }
                    if (contextTask.IsCompleted && !contextTask.IsCanceled && !contextTask.IsFaulted &&
                        contextTask.Result != null)
                    {
                        activeResponses.Add(HandleStaticRequest(contextTask.Result, file));
                        contextTask = null;
                    }
                } while (!token.IsCancellationRequested);
                listener.Stop();
                listener.Close();
                foreach (var activeResponse in activeResponses)
                {
                    try
                    {
                        await activeResponse;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            });
        }

        private async Task HandleStaticRequest(HttpListenerContext context, HttpFile file)
        {
            var request = context.Request;
            var response = context.Response;
            var rangeStart = 0L;
            var rangeEnd = -1L;
            var validRange = false;

            // If there is no response to serve, report back with a 404 Not Found status code
            if (file == null)
            {
                response.StatusCode = (int) HttpStatusCode.NotFound;
                if (SupportsRange)
                    response.AddHeader("Accept-Ranges", "bytes");
                return;
            }

            // Reject any other method except GET, POST and HEAD
            if (request.HttpMethod != WebRequestMethods.Http.Get && 
                request.HttpMethod != WebRequestMethods.Http.Post &&
                request.HttpMethod != WebRequestMethods.Http.Head)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                return;
            }

            // As default, we will response with a 200 OK status code
            response.StatusCode = (int) HttpStatusCode.OK;

            // If necessary, mention the response content type
            if (ContactType != null)
                response.ContentType = ContactType;

            // If necessary, mention the real file name for response
            if (FileName != null)
                response.AddHeader("Content-Disposition", "attachment; filename=" + FileName);

            // Mention the file etag and last modified date
            response.AddHeader("ETag", '"' + file.Md5 + '"');
            response.AddHeader("Last-Modified", file.LastModified.ToString("R"));
            
            // Set response mode
            response.SendChunked = Chunked;

            if (SupportsRange)
            {
                // Advertising range support
                response.AddHeader("Accept-Ranges", "bytes");

                // Read requested range
                if (request.Headers.AllKeys.Contains("Range"))
                {
                    // We do only accept byte as range unit
                    if (request.Headers["Range"].StartsWith("bytes=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var range = request.Headers["Range"].Substring("bytes=".Length).Split('-');

                        // And we do only support one-range requests
                        if (range.Length <= 2)
                            if (long.TryParse(range[0], out rangeStart))
                            {
                                validRange = true;
                                if (range.Length == 2)
                                    if (!long.TryParse(range[1], out rangeEnd))
                                        rangeEnd = -1;
                            }
                            else
                                rangeStart = 0;
                    }

                    // Check for If-Match precondition header for partial requests
                    if (request.Headers.AllKeys.Contains("If-Match"))
                    {
                        var ifMatch = request.Headers["If-Match"].Split(',').Select(s => s.Trim().Trim('"'));
                        validRange = validRange && ifMatch.Contains(file.Md5);
                    }

                    // Reject invalid requests
                    if (!validRange || rangeStart < 0 || ((rangeStart > rangeEnd) && (rangeEnd >= 0)) ||
                        (rangeEnd >= file.Size) || (rangeEnd < -1))
                    {
                        response.StatusCode = (int) HttpStatusCode.RequestedRangeNotSatisfiable;
                        response.AddHeader("Content-Range",
                            $"bytes */{(Chunked ? "*" : file.File.Length.ToString("D"))}");
                        if (!Chunked)
                            response.ContentLength64 = 0;
                        response.OutputStream.Close();
                        return;
                    }

                    // Check for If-Range precondition header for partial requests
                    if (request.Headers.AllKeys.Contains("If-Range"))
                    {
                        var ifRange = request.Headers["If-Range"];
                        var eTag = ifRange.Trim().Trim('"');
                        DateTime modified;
                        validRange = (DateTime.TryParse(ifRange, out modified) && file.LastModified == modified) ||
                                     file.Md5 == eTag;
                    }
                }
            }

            // Reject request if file is modified since
            if (request.Headers.AllKeys.Contains("If-Unmodified-Since"))
            {
                var ifUnmodifiedSince = DateTime.Parse(request.Headers["If-Unmodified-Since"]);
                if (ifUnmodifiedSince < file.LastModified)
                {
                    response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                    if (!Chunked)
                        response.ContentLength64 = 0;
                    response.OutputStream.Close();
                    return;
                }
            }
            
            // Response to requests about file modification
            if (request.Headers.AllKeys.Contains("If-Modified-Since") &&
                (request.HttpMethod == WebRequestMethods.Http.Get || request.HttpMethod == WebRequestMethods.Http.Head))
            {
                var ifModifiedSince = DateTime.Parse(request.Headers["If-Modified-Since"]);
                if (ifModifiedSince >= file.LastModified)
                {
                    response.StatusCode = (int) HttpStatusCode.NotModified;
                    if (!Chunked)
                        response.ContentLength64 = 0;
                    response.OutputStream.Close();
                    return;
                }
            }

            // Response to requests about file etag
            if (request.Headers.AllKeys.Contains("If-None-Match") &&
                (request.HttpMethod == WebRequestMethods.Http.Get || request.HttpMethod == WebRequestMethods.Http.Head))
            {
                var ifNoneMatch = request.Headers["If-None-Match"].Split(',').Select(s => s.Trim().Trim('"'));  
                if (ifNoneMatch.Contains(file.Md5))
                {
                    response.StatusCode = (int)HttpStatusCode.NotModified;
                    if (!Chunked)
                        response.ContentLength64 = 0;
                    response.OutputStream.Close();
                    return;
                }
            }

            // Calculate real range end
            if (rangeEnd < 0)
                rangeEnd = file.Size - 1;

            // If valid range, response correctly with a 202 Partial Content status code
            if (validRange)
            {
                response.StatusCode = (int) HttpStatusCode.PartialContent;
                response.AddHeader("Content-Range",
                    $"bytes {rangeStart}-{rangeEnd}/{(Chunked ? "*" : file.File.Length.ToString("D"))}");
            }

            // Calculate real response size
            var responseSize = rangeEnd - rangeStart + 1;

            // If not chunked, mention the expected response size
            if (!Chunked)
                response.ContentLength64 = responseSize;

            // Send response if the request http method is not HEAD
            if (request.HttpMethod != WebRequestMethods.Http.Head)
                await WriteFile(response.OutputStream, file.File, rangeStart, responseSize);

            // Close the response stream
            response.OutputStream.Close();
        }

        private async Task WriteFile(Stream stream, FileInfo file, long start, long len)
        {
            using (var fs = file.OpenRead())
            {
                fs.Seek(start, SeekOrigin.Begin);
                var buffer = new byte[8*1024];
                int bufferSize;
                var totalRead = 0L;
                do
                {
                    // Read from file to buffer
                    bufferSize = await fs.ReadAsync(buffer, 0, buffer.Length);

                    // If expected size is meet, shrink the buffer
                    if (bufferSize + totalRead > len)
                        bufferSize = (int) (len - totalRead);

                    // Write the content to the response stream
                    await stream.WriteAsync(buffer, 0, bufferSize);

                    // Flush content
                    await stream.FlushAsync();

                    // Increase the total read size
                    totalRead += bufferSize;

                    // Break if we have meet the expected size or if we reach the EOF
                } while ((bufferSize > 0) && (totalRead < len));
            }
        }
    }
}