namespace DownloadHelper
{
    public class DownloadPortionRange
    {
        public DownloadPortionRange(long start, long end)
        {
            Start = start;
            End = end;
        }

        protected DownloadPortionRange()
        {
        }


        public long End { get; protected set; }
        public long Start { get; protected set; }

        public bool Contains(DownloadPortionRange range)
        {
            return (range.Start >= Start) && (range.End <= End);
        }
        public bool Contains(long l)
        {
            return (l >= Start) && (l <= End);
        }

        public bool Continue(DownloadPortionRange range)
        {
            return (range.End == Start) && (range.Start == End);
        }

        public bool Overlaps(DownloadPortionRange range)
        {
            return (range.End > Start) && (range.Start < End);
        }
    }
}