namespace DownloadHelper
{
    public class StreamVolumeRange : DownloadRange
    {
        public static readonly StreamVolumeRange Empty = new StreamVolumeRange();
        public StreamVolumeRange() : this(0, -1)
        {
        }

        public StreamVolumeRange(long start) : this(start, -1)
        {
        }

        public StreamVolumeRange(long start, long end) : base(start, end)
        {
        }


        public new StreamVolumeRange ShrinkLength(long newLength)
        {
            return ShrinkEnd(Start + newLength - 1);
        }

        public new StreamVolumeRange ShrinkEnd(long newEnd)
        {
            return new StreamVolumeRange(Start, newEnd);
        }


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