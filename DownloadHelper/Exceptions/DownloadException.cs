using System;

namespace DownloadHelper.Exceptions
{
    public class DownloadException : Exception
    {
        public DownloadException(string message) : base(message)
        {
        }

        public DownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}