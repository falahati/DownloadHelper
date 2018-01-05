namespace DownloadHelper.EventArguments
{
    public class StatusChangedEventArgs : System.EventArgs
    {
        public StatusChangedEventArgs(DownloadConnectionStatus oldStatus, DownloadConnectionStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }

        public DownloadConnectionStatus NewStatus { get; }

        public DownloadConnectionStatus OldStatus { get; }
    }
}