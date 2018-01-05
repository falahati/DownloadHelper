using System;
using System.Net;

namespace DownloadHelper.Sample
{
    internal class DownloadItem
    {
        public CookieCollection Cookies { get; set; }
        public NetworkCredential Credential { get; set; }
        public DateTime DateAdded { get; set; }
        public string ETag { get; set; }
        public string FileAddress { get; set; } = null;
        public WebHeaderCollection Headers { get; set; }
        public DateTime LastModified { get; set; }
        public byte[] PostData { get; set; } = null;
        public Uri Referrer { get; set; }
        public string TempDirectory { get; set; } = null;
        public Uri Url { get; set; }
    }
}