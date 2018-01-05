using System;
using System.IO;
using System.Linq;
using System.Net;
using DownloadHelper.Exceptions;

namespace DownloadHelper
{
    public class DownloadResponse
    {
        public DownloadResponse(HttpWebResponse webResponse)
        {
            StatusCode = webResponse.StatusCode;
            Url = webResponse.ResponseUri;
            ContentType = webResponse.ContentType;
            Cookies = webResponse.Cookies;
            Headers = webResponse.Headers;
            LastModified = webResponse.LastModified;
            ContentLength = webResponse.ContentLength;
            
            if (webResponse.Headers.AllKeys.Select(s => s.ToLower()).Contains("ETag".ToLower()))
                ETag = webResponse.Headers["ETag"];

            if (webResponse.Headers.AllKeys.Select(s => s.ToLower()).Contains("Accept-Ranges".ToLower()))
            {
                var acceptRanges = webResponse.Headers["Accept-Ranges"];
                if (acceptRanges.Equals("bytes", StringComparison.InvariantCultureIgnoreCase))
                    SupportsRange = true;
            }
            else if (webResponse.StatusCode == HttpStatusCode.PartialContent)
            {
                SupportsRange = true;
            }

            FileName = Path.GetFileName(webResponse.ResponseUri.AbsolutePath);
            if (webResponse.Headers.AllKeys.Select(s => s.ToLower()).Contains("Content-Disposition".ToLower()))
            {
                var contentDisposition = webResponse.Headers["Content-Disposition"];
                var fileNameIndex = contentDisposition.IndexOf("filename=", StringComparison.InvariantCultureIgnoreCase);
                if (fileNameIndex > 0)
                {
                    fileNameIndex += "filename=".Length;
                    FileName = contentDisposition.Substring(fileNameIndex).Trim().Trim('\"');
                }
            }

            if (webResponse.Headers.AllKeys.Select(s => s.ToLower()).Contains("Content-Range".ToLower()))
            {
                var contentRange = webResponse.Headers["Content-Range"];
                var spaceIndex = contentRange.IndexOf(" ", StringComparison.InvariantCultureIgnoreCase);
                if (spaceIndex > 0)
                {
                    var unit = contentRange.Substring(0, spaceIndex);
                    var range = contentRange.Substring(spaceIndex + 1).Split('/');
                    if (!unit.Trim().Equals("bytes", StringComparison.InvariantCultureIgnoreCase))
                        throw new DownloadException("Server responded with an unknown unit of length measurement.");
                    if (range.Length == 2)
                    {
                        long totalSize;
                        if (long.TryParse(range[1], out totalSize))
                            FileSize = totalSize;
                        else if (range[1].Trim().Equals("*", StringComparison.InvariantCultureIgnoreCase))
                            FileSize = -1;
                    }
                    if (range.Length <= 2)
                        if (range[0].Trim().Equals("*", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (webResponse.StatusCode != HttpStatusCode.RequestedRangeNotSatisfiable)
                                throw new DownloadException(
                                    "Server responded with an invalid value for the range limit.");
                        }
                        else
                        {
                            var rangeLimit = range[0].Split('-');
                            long rangeStart;
                            if (long.TryParse(rangeLimit[0], out rangeStart))
                            {
                                long rangeEnd;
                                Range = long.TryParse(rangeLimit[1], out rangeEnd)
                                    ? new DownloadRange(rangeStart, rangeEnd)
                                    : new DownloadRange(rangeStart);
                            }
                            if (Range.HasLength && (webResponse.ContentLength != Range.Length))
                                throw new DownloadException(
                                    "Server responded with an invalid value for the content length.");
                        }
                    else
                        throw new DownloadException("Server responded with multiple ranges for a single range request.");
                }
            }
            else
            {
                FileSize = ContentLength;
            }
        }

        public long ContentLength { get; }
        public string ContentType { get; }
        public CookieCollection Cookies { get; }

        public string ETag { get; }
        public string FileName { get; }
        public long FileSize { get; } = -1;
        public string FileType => Path.GetExtension(FileName);
        public WebHeaderCollection Headers { get; }
        public bool IsChunked => ContentLength == -1;
        public bool IsPartial => StatusCode == HttpStatusCode.PartialContent;
        public DateTime LastModified { get; }
        public DownloadRange Range { get; } = DownloadRange.Empty;

        public HttpStatusCode StatusCode { get; }
        public bool SupportsRange { get; }
        public Uri Url { get; }
    }
}