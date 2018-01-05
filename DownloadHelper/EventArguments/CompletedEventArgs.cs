using System;

namespace DownloadHelper.EventArguments
{
    public class CompletedEventArgs : System.EventArgs
    {
        public CompletedEventArgs(long downloaded, DateTime started)
        {
            Downloaded = downloaded;
            Started = started;
            Completed = DateTime.Now;
            AverageSpeed = Downloaded/(Completed - Started).TotalMilliseconds*1000d;
        }

        public double AverageSpeed { get; }
        public DateTime Completed { get; }

        public long Downloaded { get; }
        public DateTime Started { get; }
    }
}