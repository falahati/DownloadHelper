using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadHelper
{
    public class DownloadPartition
    {
        private const int BufferSize = 8*1024;

        public DownloadPartition(DirectoryInfo directory, string fileName)
        {
            Directory = directory;
            FileName = fileName;
        }

        public DirectoryInfo Directory { get; }
        public string FileName { get; }

        public IEnumerable<DownloadPortionRange> Ranges => GetFilePortions();

        public FilePortion AllocateRange(DownloadPortionRange range)
        {
            var file = new FileInfo(Path.Combine(Directory.FullName, FileName + "." + range.Start + ".tmp"));
            if (!file.Exists)
                file.Create().Dispose();
            return new FilePortion(file);
        }

        public DownloadPortionRange GetFreeRange(long totalLength)
        {
            return GetFreeRange(0, totalLength, -1);
        }

        public DownloadPortionRange GetFreeRange(long start, long totalLength)
        {
            return GetFreeRange(start, totalLength, -1);
        }

        public DownloadPortionRange GetFreeRange(long start, long totalLength, long maximumLength)
        {
            if (start >= totalLength)
                throw new ArgumentException("Start position is greater or equal to the total length.", nameof(start));
            if (maximumLength == -1)
                maximumLength = totalLength - start;
            var portions = GetFilePortions().ToList();
            start = portions.FirstOrDefault(portion => portion.Contains(start))?.End ?? start;
            var end = Math.Min(start + maximumLength, totalLength);
            end = portions.FirstOrDefault(portion => portion.Contains(end))?.Start ?? end;
            if (end > start)
                return new DownloadPortionRange(start, end);
            return null;
        }

        public async void Repartition()
        {
            var portions = GetFilePortions().ToList();
            foreach (var p1 in portions.ToArray())
            {
                var shouldBreak = false;
                foreach (var p2 in portions.ToArray().Where(portion => portion != p1))
                {
                    var p3 = await Merge(p1, p2);
                    if (p3 != null)
                    {
                        portions.Remove(p1);
                        portions.Remove(p2);
                        portions.Add(p3);
                        shouldBreak = true;
                        break;
                    }
                }
                if (shouldBreak)
                    break;
            }
        }

        private IEnumerable<FilePortion> GetFilePortions()
        {
            return Directory.EnumerateFiles(FileName + ".*.tmp").Select(file => new FilePortion(file));
        }

        private async Task<FilePortion> Merge(FilePortion p1, FilePortion p2)
        {
            var first = p1.Start < p2.Start ? p1 : p2;
            var second = first == p1 ? p2 : p1;
            if (!first.Overlaps(second) && !first.Continue(second))
                return null;
            if (first.Contains(second))
            {
                second.File.Delete();
                return first;
            }
            using (var writer = first.File.OpenWrite())
            {
                writer.Seek(0, SeekOrigin.End);
                using (var reader = second.File.OpenRead())
                {
                    reader.Seek(first.End - second.Start, SeekOrigin.Begin);
                    var bytes = new byte[BufferSize];
                    int read;
                    do
                    {
                        read = await reader.ReadAsync(bytes, 0, bytes.Length);
                        await writer.WriteAsync(bytes, 0, read);
                    } while (read > 0);
                    reader.Close();
                }
                writer.Flush();
                writer.Close();
            }
            second.File.Delete();
            return new FilePortion(first.File);
        }

        public class FilePortion : DownloadPortionRange, IDisposable
        {
            internal FilePortion(FileInfo file)
            {
                File = file;
                if (Start < 0)
                    throw new InvalidOperationException("Invalid file passed.");
            }

            public DownloadConnection Connection { get; } = null;

            public long End
            {
                get
                {
                    if (Connection != null)
                        return Connection.Range.End;
                    var start = Start;
                    if (start < 0)
                        return -1;
                    return Start + File.Length;
                }
            }

            public FileInfo File { get; }

            public long Start
            {
                get
                {
                    var fileName = Path.GetFileNameWithoutExtension(File.Name);
                    var startIndex = fileName.LastIndexOf(".", StringComparison.InvariantCulture);
                    long start;
                    if ((startIndex > 0) && long.TryParse(fileName.Substring(startIndex + 1), out start))
                        return start;
                    return -1;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Connection?.Dispose();
            }

            public void AssignConnection(DownloadRequest request)
            {
            }
        }
    }
}