using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DownloadHelper
{
    public class StreamPartition
    {
        private readonly List<StreamVolume> _volumes = new List<StreamVolume>();

        public StreamPartition(Stream stream, long totalSize = -1)
        {
            Stream = stream;
            if (totalSize < 0)
                totalSize = stream.Length;
            TotalSize = totalSize;
        }

        public Stream Stream { get; }
        public long TotalSize { get; set; }

        public StreamVolume[] Volumes
        {
            get
            {
                lock (_volumes)
                {
                    return _volumes.ToArray();
                }
            }
        }

        public StreamVolume GetFreeVolume()
        {
            return GetFreeVolume(0);
        }

        public StreamVolume GetFreeVolume(long start)
        {
            return GetFreeVolume(start, -1);
        }

        public StreamVolume GetFreeVolume(long start, long maximumLength)
        {
            if (start >= TotalSize)
                throw new ArgumentException("Start position is greater or equal to the total length.", nameof(start));
            if (maximumLength == -1)
                maximumLength = TotalSize - start;
            var volumes = Volumes.ToList();
            StreamVolume overlap;
            do
            {
                overlap = volumes.FirstOrDefault(volume => volume.Range.Contains(start));
                if (overlap != null)
                    start = overlap.Range.End + 1;
            } while (overlap != null);
            var end = Math.Min(start + maximumLength, TotalSize) - 1;
            do
            {
                overlap = volumes.FirstOrDefault(volume => volume.Range.Contains(end));
                if (overlap != null)
                    end = overlap.Range.Start - 1;
            } while (overlap != null);
            if (end >= start)
            {
                var volume = new StreamVolume(Stream, new StreamVolumeRange(start, end));
                lock (_volumes)
                {
                    _volumes.Add(volume);
                }
                return volume;
            }
            return null;
        }
    }
}