using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DownloadHelper.EventArguments;
using DownloadHelper.Exceptions;

namespace DownloadHelper
{
    public class DownloadConnection : IDisposable
    {
        private const int AverageSpeedDurationInMillisecond = 5*1000;
        private const int UpdateMillisecond = 300;

        private int _bufferSize = 1024;
        private int _cacheSize = 8*1024;
        private long _downloaded;
        private DateTime _downloadStarted = DateTime.Now;
        private string _message = "Paused";
        private DownloadRange _range = DownloadRange.Empty;
        private int _retries = 3;
        private DownloadConnectionStatus _status = DownloadConnectionStatus.Paused;
        private int _timeout = 5000;
        private long _totalSize = -1L;
        private int _tries;

        public DownloadConnection(DownloadRequest request, Stream stream) : this(request, stream, -1)
        {
        }

        public DownloadConnection(DownloadRequest request, Stream stream, long startPosition)
            : this(request, DownloadRange.Empty, stream, startPosition)
        {
        }

        public DownloadConnection(DownloadRequest request, DownloadRange range, Stream stream)
            : this(request, range, stream, -1)
        {
        }

        public DownloadConnection(DownloadRequest request, DownloadRange range, Stream stream,
            long startPosition)
        {
            if (startPosition < -1)
                throw new ArgumentException("Negative download length is invalid.", nameof(startPosition));
            Request = request;
            Range = range;
            Stream = stream;
            TotalDownloaded = startPosition >= 0 ? startPosition : (Stream.CanSeek ? Stream.Length : 0);
            ShrinkStream(TotalDownloaded, CancellationToken.None).Wait();
            if (Range.HasLength && (TotalWritten > Range.Length))
                throw new ArgumentException("Invalid stream specified. Size is bigger than the requested portion.",
                    nameof(stream));
        }

        public long AverageSpeed { get; private set; }

        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                lock (this)
                {
                    if (Status == DownloadConnectionStatus.Downloading)
                        throw new InvalidOperationException(
                            "Can not change the value of this property while download is in progress.");
                }
                _bufferSize = value;
            }
        }

        public int CacheSize
        {
            get { return _cacheSize; }
            set
            {
                lock (this)
                {
                    if (Status == DownloadConnectionStatus.Downloading)
                        throw new InvalidOperationException(
                            "Can not change the value of this property while download is in progress.");
                }
                _cacheSize = value;
            }
        }

        public long TotalDownloaded { get; private set; }

        public string Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                OnMessageChanged();
            }
        }

        public double Progress
            => TotalSize >= 0 ? Math.Min(Math.Max(TotalDownloaded*100d/TotalSize, 0), 100) : double.NaN;

        public DownloadRange Range
        {
            get { return _range; }
            set
            {
                lock (this)
                {
                    if ((Status == DownloadConnectionStatus.Downloading) && (value.End > _range.End) &&
                        (_range.End >= 0))
                        throw new InvalidOperationException(
                            "Can not extend the end range while download is in progress.");

                    if ((Status == DownloadConnectionStatus.Downloading) && (value.Start != _range.Start))
                        throw new InvalidOperationException(
                            "Can not change the value of range start while download is in progress.");

                    if (value.HasLength && (value.Length < TotalDownloaded))
                        throw new InvalidOperationException(
                            "Can not shrink the end range to less than what we have downloaded since.");

                    _range = value;
                }
            }
        }

        public DownloadRequest Request { get; }

        public int Retries
        {
            get { return _retries; }
            set
            {
                lock (this)
                {
                    if (Status == DownloadConnectionStatus.Downloading)
                        throw new InvalidOperationException(
                            "Can not change the value of this property while download is in progress.");
                }
                _retries = value;
            }
        }

        public long Speed { get; private set; }

        public DownloadSpeedLimiter SpeedLimiter { get; set; }

        public DownloadConnectionStatus Status
        {
            get { return _status; }
            private set
            {
                if (_status == value)
                    return;
                var oldStatus = _status;
                _status = value;
                switch (_status)
                {
                    case DownloadConnectionStatus.Downloading:
                        _downloaded = 0;
                        _downloadStarted = DateTime.Now;
                        break;
                    case DownloadConnectionStatus.Completed:
                        Message = "Completed";
                        OnCompleted(_downloaded, _downloadStarted);
                        break;
                    case DownloadConnectionStatus.Paused:
                        Message = "Paused";
                        OnCompleted(_downloaded, _downloadStarted);
                        break;
                    case DownloadConnectionStatus.Error:
                        OnCompleted(_downloaded, _downloadStarted);
                        break;
                }
                OnStatusChanged(oldStatus, _status);
            }
        }

        public Stream Stream { get; }


        public int Timeout
        {
            get { return _timeout; }
            set
            {
                lock (this)
                {
                    if (Status == DownloadConnectionStatus.Downloading)
                        throw new InvalidOperationException(
                            "Can not change the value of this property while download is in progress.");
                }
                _timeout = value;
            }
        }

        public long TotalSize
        {
            get { return Range.HasLength ? Math.Min(_totalSize, Range.Length) : _totalSize; }
            private set { _totalSize = value; }
        }

        public long TotalWritten { get; private set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Stream?.Flush();
            Stream?.Close();
            Stream?.Dispose();
        }

        public event EventHandler<CompletedEventArgs> Completed;
        public event EventHandler<DataAvailableEventArgs> DataAvailable;

        public event EventHandler MessageChanged;
        
        public event EventHandler ProgressChanged;

        public event EventHandler<ResponseReceivedEventArgs> ResponseReceived;


        public async Task Start()
        {
            await Start(CancellationToken.None, null);
        }

        public async Task Start(CancellationToken stopToken)
        {
            await Start(stopToken, null);
        }


        public async Task Start(CancellationToken stopToken, object state)
        {
            // TODO state
            lock (this)
            {
                if (Status == DownloadConnectionStatus.Downloading)
                    throw new DownloadException("Can not start multiple downloads for one connection.");
                Status = DownloadConnectionStatus.Downloading;
            }
            _tries = 0;
            do
            {
                _tries++;
                try
                {
                    await Download(stopToken);
                }
                catch (InvalidOperationException exception)
                {
                    Message = exception.Message;
                    Status = DownloadConnectionStatus.Error;
                    throw;
                }
                catch (OperationCanceledException exception)
                {
                    Message = exception.Message;
                    Status = DownloadConnectionStatus.Error;
                    throw;
                }
                catch (Exception exception)
                {
                    Message = exception.Message;
                    if (_tries < Retries)
                        continue;
                    Status = DownloadConnectionStatus.Error;
                    throw;
                }
                break;
            } while (!stopToken.IsCancellationRequested);
        }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        protected async Task CopyStream(Stream responseStream, CancellationToken stopToken)
        {
            using (responseStream)
            {
                using (var cache = new MemoryStream(CacheSize))
                {
                    var buffer = new byte[BufferSize];
                    var oldTotalDownloaded = TotalDownloaded;
                    var lastSpeedUpdate = DateTime.Now;
                    var endReached = false;
                    int bytesRead;
                    do
                    {
                        // Read data from internet
                        bytesRead =
                            await
                                responseStream.ReadAsync(buffer, 0, buffer.Length, stopToken);

                        // Notify user about new data
                        if (bytesRead > 0)
                            OnDataAvailable(bytesRead);

                        lock (this)
                        {
                            // Limit buffer early if we meet the range expected bytes
                            if ((TotalSize > 0) && (TotalDownloaded + bytesRead > TotalSize))
                            {
                                bytesRead = (int) (TotalSize - TotalDownloaded);
                                if (bytesRead < 0)
                                    throw new DownloadException("Invalid value for the end range.");
                                endReached = true;
                            }
                        }

                        // Write newly read data to the memory cache
                        await cache.WriteAsync(buffer, 0, bytesRead);
                        _downloaded += bytesRead;
                        TotalDownloaded += bytesRead;

                        // If the connection is closed or canceled, 
                        // or if we have meet the end, 
                        // or if the memory cache don't have enough space for another full buffer
                        if (endReached ||
                            (bytesRead == 0) ||
                            stopToken.IsCancellationRequested ||
                            (cache.Length < cache.Position + buffer.Length))
                        {
                            // Dump memory cache to stream
                            var cacheContent = new byte[cache.Position];
                            cache.Seek(0, SeekOrigin.Begin);
                            await cache.ReadAsync(cacheContent, 0, cacheContent.Length);
                            if (Stream.CanSeek)
                                Stream.Seek(TotalWritten, SeekOrigin.Begin);
                            await Stream.WriteAsync(cacheContent, 0, cacheContent.Length);

                            // Reset memory cache
                            cache.Seek(0, SeekOrigin.Begin);
                            TotalWritten += cacheContent.Length;

                            // Notify user
                            OnProgressChanged();
                        }

                        // Limit the reading speed
                        var limit = SpeedLimiter?.Limit(this);
                        if (limit != null)
                            await limit;

                        // Calculating time elapsed since last update
                        var speedUpdateElapsed = (DateTime.Now - lastSpeedUpdate).TotalMilliseconds;

                        // Calculate total download in the elapsed time
                        var downloadedBytes = TotalDownloaded - oldTotalDownloaded;

                        // If necessary, update the speed information
                        if (speedUpdateElapsed > UpdateMillisecond)
                        {
                            lastSpeedUpdate = DateTime.Now;
                            oldTotalDownloaded = TotalDownloaded;

                            // Calculating elapsed fraction
                            var fractionOfSecond = speedUpdateElapsed/1000d;

                            // Calculating current Bps speed
                            Speed = (long) (downloadedBytes/fractionOfSecond);

                            // Normalizing speed for the last average duration
                            var fractionOfAverage = speedUpdateElapsed/AverageSpeedDurationInMillisecond;
                            AverageSpeed = AverageSpeed > 0
                                ? (long) (AverageSpeed*(1 - fractionOfAverage) + Speed*fractionOfAverage)
                                : Speed;

                            // Notify user
                            OnProgressChanged();
                        }
                    } while (!endReached && (bytesRead > 0) && !stopToken.IsCancellationRequested);
                }
            }
        }

        protected async Task Download(CancellationToken stopToken)
        {
            if (Range.HasLength && (TotalWritten == Range.Length))
            {
                lock (this)
                {
                    Status = DownloadConnectionStatus.Completed;
                }
                return;
            }
            if (!stopToken.IsCancellationRequested)
            {
                Message = "Initializing ...";
                var webRequest = await GetRequest(stopToken);
                using (var webResponse = await GetResponse(webRequest, stopToken))
                {
                    Message = "Sending request ...";
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new DownloadException("Failed to retrieve response stream. Connection refused.");
                        responseStream.ReadTimeout = Timeout;
                        Message = "Downloading ...";
                        await CopyStream(responseStream, stopToken);
                    }
                }
            }
            lock (this)
            {
                if ((TotalSize >= 0) && (TotalWritten == TotalSize))
                    Status = DownloadConnectionStatus.Completed;
                else if ((TotalSize < 0) && !stopToken.IsCancellationRequested)
                    Status = DownloadConnectionStatus.Completed;
                else
                    Status = DownloadConnectionStatus.Paused;
            }
        }

        protected async Task<HttpWebRequest> GetRequest(CancellationToken stopToken)
        {
            var webRequest = WebRequest.CreateHttp(Request.Url);
            webRequest.Method = Request.PostData == null ? WebRequestMethods.Http.Get : WebRequestMethods.Http.Post;
            webRequest.Timeout = Timeout;

            if (Request.Headers.Count > 0)
                webRequest.Headers = Request.Headers;

            if (Request.Cookies.Count > 0)
            {
                webRequest.CookieContainer = new CookieContainer();
                webRequest.CookieContainer.Add(Request.Cookies);
            }

            webRequest.PreAuthenticate = Request.ServerLogin != null;
            webRequest.Credentials = webRequest.PreAuthenticate
                ? Request.ServerLogin
                : CredentialCache.DefaultCredentials;
            webRequest.Proxy = Request.Proxy ?? WebRequest.DefaultWebProxy;

            if (Request.NetworkInterface != null)
            {
                if ((Request.NetworkInterface.OperationalStatus != OperationalStatus.Up) &&
                    (Request.NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback))
                    throw new DownloadException("Invalid network adapter is selected or the network is down.");
                var ipAddress =
                    Request.NetworkInterface.GetIPProperties()
                        .UnicastAddresses.Where(
                            information =>
                                (information.Address.AddressFamily == AddressFamily.InterNetwork) ||
                                (information.Address.AddressFamily == AddressFamily.InterNetworkV6))
                        .OrderBy(
                            information => information.Address.AddressFamily == AddressFamily.InterNetwork ? 0 : 1)
                        .Select(information => information.Address).FirstOrDefault();
                if (ipAddress == null)
                    throw new DownloadException(
                        "Invalid network adapter is selected or failed to get network IP address.");
                ServicePointManager.MaxServicePointIdleTime = 1;
                webRequest.ServicePoint.BindIPEndPointDelegate = delegate { return new IPEndPoint(ipAddress, 0); };
            }

            Message = "Connecting ...";
            if (Request.PostData != null)
            {
                Message = "Sending request ...";
                webRequest.ContentLength = Request.PostData.Length;
                Stream requestStream;
                using (requestStream = await webRequest.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(Request.PostData, 0, Request.PostData.Length);
                    requestStream.Close();
                }
            }
            else
            {
                if (Range.HasLength)
                    webRequest.AddRange(Range.Start + TotalWritten, Range.End);
                else
                    webRequest.AddRange(Range.Start + TotalWritten);
            }
            return webRequest;
        }

        protected async Task<HttpWebResponse> GetResponse(HttpWebRequest webRequest, CancellationToken stopToken)
        {
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse) await webRequest.GetResponseAsync();
            }
            catch (WebException we)
            {
                webResponse = we.Response as HttpWebResponse;
                if (webResponse == null)
                    throw;
            }
            var response = new DownloadResponse(webResponse);

            if (OnResponseReceived(response))
                throw new OperationCanceledException("Aborted by the user.");

            await ShrinkStream(response.Range.Start - Range.Start, stopToken);

            if (response.ContentLength > 0)
                TotalSize = response.IsPartial ? response.ContentLength + TotalWritten : response.ContentLength;

            if ((response.StatusCode != HttpStatusCode.OK) &&
                (response.StatusCode != HttpStatusCode.PartialContent))
                throw new DownloadException($"Failed to start downloading file. {webResponse.StatusDescription}");

            _tries = 0;
            return webResponse;
        }

        protected void OnCompleted(long downloaded, DateTime started)
        {
            Completed?.Invoke(this, new CompletedEventArgs(downloaded, started));
        }

        protected void OnDataAvailable(long len)
        {
            DataAvailable?.Invoke(this, new DataAvailableEventArgs(len));
        }

        protected void OnMessageChanged()
        {
            MessageChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        protected bool OnResponseReceived(DownloadResponse response)
        {
            var eventArgs = new ResponseReceivedEventArgs(Request, response);
            ResponseReceived?.Invoke(this, eventArgs);
            return eventArgs.Abort;
        }

        protected void OnStatusChanged(DownloadConnectionStatus oldStatus, DownloadConnectionStatus newStatus)
        {
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(oldStatus, newStatus));
        }

        // ReSharper disable once UnusedParameter.Local
        private async Task ShrinkStream(long size, CancellationToken stopToken)
        {
            Stream.Flush();
            if (Stream.CanSeek && Stream.Length != size)
            {
                Stream.SetLength(size);
            }
            TotalWritten = size;
        }
    }
}