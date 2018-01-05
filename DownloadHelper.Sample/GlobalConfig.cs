using System.Net;
using System.Net.NetworkInformation;

namespace DownloadHelper.Sample
{
    internal class GlobalConfig
    {
        public int Connections { get; set; }
        public long DiskCacheSize { get; set; }
        public long MemoryBufferSize { get; set; }
        public NetworkInterface NetworkInterface { get; set; } = null;
        public string OutputDirectory { get; set; }
        public WebProxy Proxy { get; set; } = null;
        public long SpeedLimit { get; set; }
        public string UserAgent { get; set; }
    }
}