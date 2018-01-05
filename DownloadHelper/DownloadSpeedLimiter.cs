using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadHelper
{
    public class DownloadSpeedLimiter
    {
        private readonly Dictionary<DownloadConnection, Tuple<long, DateTime>> _connections =
            new Dictionary<DownloadConnection, Tuple<long, DateTime>>();

        public DownloadSpeedLimiter(long limitedSpeed)
        {
            IsActive = true;
            Limited = limitedSpeed;
        }

        public DownloadSpeedLimiter()
        {
            IsActive = false;
            Limited = 0;
        }

        public bool IsActive { get; set; }

        public long Limited { get; set; }
        public DownloadSpeedLimiter ParentLimiter { get; set; } = null;

        public long TotalSpeed
        {
            get
            {
                lock (this)
                {
                    return _connections.Keys.Select(connection => connection.Speed).DefaultIfEmpty().Sum();
                }
            }
        }

        public async Task Limit(DownloadConnection connection)
        {
            if ((Limited <= 0) || !IsActive)
                return;
            Tuple<long, DateTime> lastInfo = null;
            var limited = (float) Limited;
            lock (this)
            {
                if (!_connections.ContainsKey(connection))
                    _connections.Add(connection, new Tuple<long, DateTime>(connection.TotalDownloaded, DateTime.Now));
                else
                    lastInfo = _connections[connection];
                foreach (var c in _connections.Keys.ToArray())
                    if (c.Status != DownloadConnectionStatus.Downloading)
                        _connections.Remove(c);
                limited /= _connections.Count;
            }

            if (lastInfo != null)
            {
                // First limit by parent
                var limit = ParentLimiter?.Limit(connection);
                if (limit != null)
                    await limit;

                // Calculate elapsed time since last sample
                var elapsed = (int) (DateTime.Now - lastInfo.Item2).TotalMilliseconds;

                // Calculate downloaded bytes since last sample
                var downloaded = connection.TotalDownloaded - lastInfo.Item1;

                // Calculate expected delay
                var delay = (int) (1000f*(downloaded/limited)) - elapsed;

                //System.Diagnostics.Debug.WriteLine("{0}ms - {1}b => {2}ms (+{3}ms)", elapsed, downloaded, delay + elapsed, delay);

                // Skip this time as we cant seriously expect a delay lower than 50ms
                if ((delay < 50) && (delay > 0))
                    return;

                lock (this)
                {
                    if (_connections.ContainsKey(connection))
                        _connections[connection] = new Tuple<long, DateTime>(connection.TotalDownloaded,
                            DateTime.Now.AddMilliseconds(delay >= 50 ? delay : 0));
                }

                // Sleep for the calculated time if it is higher than 50ms and ignore it completely if it is lower than zero
                if (delay >= 50)
                    await Task.Delay(delay);
            }
        }
    }
}