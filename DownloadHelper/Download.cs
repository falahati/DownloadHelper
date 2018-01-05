//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.Remoting.Messaging;
//using System.Threading.Tasks;
//using DownloadHelper.Exceptions;

//namespace DownloadHelper
//{
//    public class Download
//    {
//        private readonly List<DownloadConnection> _connections = new List<DownloadConnection>();

//        public Download(DownloadRequest request, DirectoryInfo directory, string fileName = null,
//            DirectoryInfo tempDirectory = null)
//        {
//            Request = request;
//            Directory = directory;
//            FileName = fileName;
//            TempDirectory = tempDirectory;
//            if (TempDirectory == null)
//            {
//                TempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
//                Directory.Create();
//            }
//            Partition = new DownloadPartition(TempDirectory, Request.FileName);
//        }

//        public long AverageSpeed => Connections.Select(connection => connection.AverageSpeed).DefaultIfEmpty().Sum();

//        public DownloadConnection[] Connections
//        {
//            get
//            {
//                lock (_connections)
//                {
//                    return _connections.ToArray();
//                }
//            }
//        }

//        public DirectoryInfo Directory { get; set; }
//        public long TotalDownloaded => Connections.Select(connection => connection.TotalDownloaded).DefaultIfEmpty().Sum();
//        public string FileName { get; set; }
//        public string Message { get; private set; } = "";
//        public int MaximumConnections { get; set; } = 4;
//        public double Progress => TotalSize > 0 ? 100d * TotalDownloaded / TotalSize : 0;
//        public DownloadRequest Request { get; }
//        public long Speed => Connections.Select(connection => connection.Speed).DefaultIfEmpty().Sum();

//        public int ActiveConnections
//            => Connections.Select(connection => connection.Status != DownloadConnectionStatus.Completed).Count();
//        public DownloadStatus Status { get; private set; } = DownloadStatus.Paused;

//        public DirectoryInfo TempDirectory { get; private set; }
//        public long TotalSize => Connections.Select(connection => connection.TotalSize).DefaultIfEmpty().Sum();
//        public long TotalWritten => Connections.Select(connection => connection.TotalWritten).DefaultIfEmpty().Sum();

//        public async Task Pause()
//        {
//            lock (this)
//            {
//                if ((Status == DownloadStatus.Connecting) || (Status == DownloadStatus.Downloading))
//                    Status = DownloadStatus.Pausing;
//            }
//            await Task.WhenAll(Connections.Select(connection => connection.Pause()).ToArray());
//            lock (this)
//            {
//                Status = Connections.All(connection => connection.Status == DownloadConnectionStatus.Completed)
//                    ? DownloadStatus.Completed
//                    : DownloadStatus.Paused;
//            }
//        }

//        public DownloadPartition Partition { get; }

//        private DownloadConnection SpawnConnection(long start, long length = -1)
//        {
//            DownloadPortionRange range;
//            if (start != TotalSize || start != 0)
//            {
//                range = Partition.GetFreeVolume(start, TotalSize, length);
//                if (range == null)
//                {
//                    return null;
//                }
//            }
//            range = new DownloadPortionRange(0, -1);
//            var file = Partition.GetRangeFile(range);
//            var stream = file.OpenWrite();
//            var connection = new DownloadConnection(Request, start, length, stream);
//            connection.ProgressChanged += ConnectionOnProgressChanged;
//            connection.Completed += ConnectionOnCompleted;
//            lock (_connections)
//            {
//                _connections.Add(connection);
//            }
//            return connection;
//        }

//        private async Task StartConnections(long totalSize)
//        {
//            long remaining;
//            List<DownloadConnection> lConnections = new List<DownloadConnection>();
//            do
//            {
//                remaining = totalSize - TotalDownloaded;
//                var partSize =
//                    (long)Math.Min(Math.Floor(remaining / (double)MaximumConnections), Math.Max(remaining, 100 * 1024));
//                //var nConnections = (int)Math.Floor(totalSize / (double)partSize);
//                var start = 0L;
//                while (true)
//                {
//                    var lC = lConnections.Select(
//                            connection => new DownloadPortionRange(connection.RangeStart, connection.RangeEnd))
//                        .FirstOrDefault(portion => portion.Contains(start));
//                    if (lC == null)
//                    {
//                        break;
//                    }
//                    start = lC.End;
//                }
//                while (ActiveConnections < MaximumConnections)
//                {
//                    var nc = SpawnConnection(start, partSize);
//                    if (nc == null)
//                    {
//                        break;
//                    }
//                    lConnections.Add(nc);
//                    start += partSize;
//                }
//                await Task.WhenAny(Connections.Select(connection => connection.Start()).ToArray());
//                foreach (var downloadConnection in lConnections)
//                {
//                    if (downloadConnection.Status != DownloadConnectionStatus.Downloading)
//                    {
//                        lConnections.Remove(downloadConnection);
//                        downloadConnection.Dispose();
//                    }
//                }
//            } while (remaining > 0);
//            lock (this)
//            {
//                Status = DownloadStatus.Completed;
//            }
//        }

//        private async Task StartInitialConnection()
//        {
//            var connection = SpawnConnection(0);
//            connection.ResponseReceived += (sender, args) =>
//            {
//                var c = sender as DownloadConnection;
//                if (c == null || (c.SupportsRange && c.TotalSize >= 0))
//                {
//                    if (c != null)
//                    {
//                        lock (_connections)
//                        {
//                            if (_connections.Contains(c))
//                                _connections.Remove(c);
//                        }
//                    }
//                    args.Abort = true;
//                }
//                else
//                {
//                    lock (this)
//                    {
//                        Status = DownloadStatus.Downloading;
//                    }
//                }
//            };
//            try
//            {
//                await connection.Start();
//                connection.Dispose();
//            }
//            catch (DownloadAbortedException)
//            {
//                var totalSize = connection.TotalSize;
//                connection.Dispose();
//                await StartConnections(totalSize);
//            }
//            catch (Exception ex)
//            {
//                lock (this)
//                {
//                    Status = DownloadStatus.Error;
//                    Message = ex.Message;
//                }
//                connection.Dispose();
//                throw;
//            }
//        }

//        private void ConnectionOnCompleted(object sender, CompletedEventArgs completedEventArgs)
//        {

//        }

//        private void ConnectionOnProgressChanged(object sender, EventArgs eventArgs)
//        {

//        }

//        public async Task Start()
//        {
//            lock (this)
//            {
//                if ((Status == DownloadStatus.Connecting) || (Status == DownloadStatus.Downloading) ||
//                    (Status == DownloadStatus.Completed) || (Status == DownloadStatus.Pausing))
//                    return;
//                Status = DownloadStatus.Connecting;
//            }

//            await StartInitialConnection();
//        }
//    }
//}