using System;

namespace DownloadHelper.EventArguments
{
    public class ResponseReceivedEventArgs : EventArgs
    {
        public ResponseReceivedEventArgs(DownloadRequest request, DownloadResponse response)
        {
            Request = request;
            Response = response;
        }

        public bool Abort { get; set; } = false;
        public DownloadRequest Request { get; }
        public DownloadResponse Response { get; }
    }
}