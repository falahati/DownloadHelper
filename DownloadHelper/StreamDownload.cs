using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DownloadHelper.EventArguments;

namespace DownloadHelper
{
    public class StreamDownload
    {
        private readonly List<DownloadConnection> _connections = new List<DownloadConnection>();

        public StreamDownload(DownloadRequest request, StreamPartition partition)
        {
            Request = request;
            Partition = partition;
        }

        public DownloadConnection[] Connections
        {
            get
            {
                lock (_connections)
                {
                    return _connections.ToArray();
                }
            }
        }

        public long TotalWritten => Connections.DefaultIfEmpty().Sum(connection => connection?.TotalWritten ?? 0);
        public long AverageSpeed => Connections.DefaultIfEmpty().Sum(connection => connection?.AverageSpeed ?? 0);
        public long Speed => Connections.DefaultIfEmpty().Sum(connection => connection?.Speed ?? 0);
        public double Progress => Partition.TotalSize <= 0 ? double.NaN : TotalWritten*100d/Partition.TotalSize;

        public DownloadStatus Status
        {
            get
            {
                var connections = Connections;
                if (connections.Length == 0)
                {
                    return DownloadStatus.Paused;
                }
                if (connections.All(connection => connection.Status == DownloadConnectionStatus.Completed))
                {
                    return DownloadStatus.Completed;
                }
                if (connections.All(connection => connection.Status == DownloadConnectionStatus.Paused))
                {
                    return DownloadStatus.Paused;
                }
                else if (connections.Any(connection => connection.Status == DownloadConnectionStatus.Paused))
                {
                    return DownloadStatus.Pausing;
                }
                return DownloadStatus.Downloading;
            }
        }

        public StreamPartition Partition { get; set; }

        public DownloadRequest Request { get; set; }

        public async Task Start()
        {
            await Start(CancellationToken.None);
        }

        public event EventHandler<EventArgs> ProgressChanged;
        public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;

        private bool OnResponseReceived(DownloadResponse response)
        {
            var args = new ResponseReceivedEventArgs(Request, response);
            ResponseReceived?.Invoke(this, args);
            return args.Abort;
        }

        private void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, new EventArgs());
        }
        private DownloadConnection SpawnConnection(StreamVolume stream)
        {
            var connection = new DownloadConnection(Request, new DownloadRange(stream.Range.Start), stream, 0);
            connection.Completed += delegate (object sender, CompletedEventArgs args)
            {
                connection.Stream.Close();
                OnProgressChanged();
            };

            connection.ProgressChanged += delegate (object sender, EventArgs args)
            {
                OnProgressChanged();
            };

            connection.ResponseReceived += delegate (object sender, ResponseReceivedEventArgs args)
            {
                if (args.Response.SupportsRange)
                {
                    connection.Range = connection.Range.ShrinkLength(stream.Range.Length);
                }
            };
            lock (_connections)
            {
                _connections.Add(connection);
            }
            return connection;
        }

        public async Task Start(CancellationToken token)
        {
            var partSize = 1024*1024*3;
            var nConnections = 3;
            var splitPartition = new StreamVolume(Partition.Stream, StreamVolumeRange.Empty);
            var initialConnection = SpawnConnection(splitPartition);
            var taskSource = new TaskCompletionSource<DownloadResponse>();
            initialConnection.ResponseReceived += (sender, args) =>
            {
                if (OnResponseReceived(args.Response))
                {
                    args.Abort = true;
                    taskSource.SetCanceled();
                    return;
                }
                if (args.Response.SupportsRange)
                {
                    Partition.TotalSize = args.Response.FileSize;
                    Partition.Stream.SetLength(Partition.TotalSize);
                    var newRange = Partition.GetFreeVolume(0, partSize);
                    splitPartition.Range = newRange.Range;
                    initialConnection.Range = newRange.Range;
                }
                taskSource.SetResult(args.Response);
            };
            var initialTask = initialConnection.Start(token);
            var response = await taskSource.Task;
            if (!response.SupportsRange)
            {
                await Task.WhenAll(initialTask);
                if (initialTask.IsCanceled)
                {
                    // Canceled
                }
                return;
            }
            var connections = new List<Tuple<DownloadConnection, Task>>
            {
                new Tuple<DownloadConnection, Task>(initialConnection, initialTask)
            };
            StreamVolume range;
            do
            {
                range = Partition.GetFreeVolume(0, partSize);
                if (range != null)
                {
                    while (connections.Count(t => t.Item1.Status == DownloadConnectionStatus.Downloading) >=
                           nConnections)
                    {
                        await
                            Task.WhenAny(
                                connections.Where(t => t.Item1.Status == DownloadConnectionStatus.Downloading)
                                    .Select(t => t.Item2)
                                    .ToArray());
                    }
                    var connection = SpawnConnection(range);
                    var task = connection.Start(token);
                    connections.Add(new Tuple<DownloadConnection, Task>(connection, task));
                }
            } while (!token.IsCancellationRequested && (range != null));
            await Task.WhenAll(connections.Select(t => t.Item2).ToArray());
        }
    }
}