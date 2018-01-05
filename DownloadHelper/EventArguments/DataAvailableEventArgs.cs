using System;

namespace DownloadHelper.EventArguments
{
    public class DataAvailableEventArgs : EventArgs
    {
        public DataAvailableEventArgs(long len)
        {
            DataLength = len;
        }

        public long DataLength { get; }
    }
}