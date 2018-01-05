using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace DownloadHelper
{
    public class DownloadRequest
    {
        public DownloadRequest(Uri url)
        {
            Url = url;
        }

        public CookieCollection Cookies { get; } = new CookieCollection();
        public string FileType => Path.GetExtension(Url.AbsolutePath);
        public string FileName => Path.GetFileName(Url.AbsolutePath);
        public WebHeaderCollection Headers { get; } = new WebHeaderCollection();

        public NetworkInterface NetworkInterface { get; set; } = null;

        public byte[] PostData { get; set; } = null;
        public WebProxy Proxy { get; set; } = null;
        public NetworkCredential ServerLogin { get; set; } = null;
        public Uri Url { get; }
    }
}