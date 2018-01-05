using System;

namespace DownloadHelper
{
    public class DownloadRange
    {
        public static readonly DownloadRange Empty = new DownloadRange();

        public DownloadRange() : this(0, -1)
        {
        }

        public DownloadRange(long start) : this(start, -1)
        {
        }

        public DownloadRange ShrinkLength(long newLength)
        {
            return ShrinkEnd(newLength < 0 ? -1 : Start + newLength - 1);
        }

        public DownloadRange ShrinkEnd(long newEnd)
        {
            return new DownloadRange(Start, newEnd);
        }

        public DownloadRange(long start, long end)
        {
            if (end < -1)
                throw new ArgumentException("Negative range is invalid.", nameof(end));
            if (start < 0)
                throw new ArgumentException("Negative range is invalid.", nameof(start));
            if ((end < start) && (end >= 0))
                throw new ArgumentException("Range ending index can not be smaller than the range starting index.");
            Start = start;
            End = end;
        }

        public long End { get; }

        public long Length
        {
            get
            {
                if (End >= 0)
                    return End - Start + 1;
                return -1;
            }
        }

        public bool HasLength => End >= 0;

        public long Start { get; }
    }
}