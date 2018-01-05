using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DownloadHelper.Tests
{
    internal class HttpFile : IDisposable
    {
        private HttpFile(FileInfo file)
        {
            Size = file.Length;
            File = file;
        }

        public FileInfo File { get; }

        public string Md5 => BitConverter.ToString(GetMd5(File, 0, -1).Result).Replace("-", "");
        public DateTime LastModified => File.LastWriteTimeUtc;
        public long Size { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (this)
            {
                if (File.Exists)
                    File.Delete();
            }
        }

        public static async Task<HttpFile> MakeFile(long size)
        {
            var file = new FileInfo(Path.GetTempFileName());
            var bufferSize = 8*1024;
            var buffer = new byte[bufferSize];
            var rng = new Random();
            var written = 0L;
            using (var fs = file.OpenWrite())
            {
                while (written < size)
                {
                    await Task.Run(() => rng.NextBytes(buffer));
                    if (bufferSize + written >= size)
                        bufferSize = (int) (size - written);
                    await fs.WriteAsync(buffer, 0, bufferSize);
                    written += bufferSize;
                }
            }
            return new HttpFile(file);
        }

        private static async Task<byte[]> GetMd5(FileInfo file, long start, long count)
        {
            using (var md5 = MD5.Create())
            {
                using (var fs = file.OpenRead())
                {
                    fs.Seek(start, SeekOrigin.Begin);
                    var buffer = new byte[8 * 1024];
                    int read;
                    var totalRead = 0L;
                    do
                    {
                        read = await fs.ReadAsync(buffer, 0, buffer.Length);
                        if (count >= 0 && read + totalRead >= count)
                            read = (int)(count - totalRead);
                        await Task.Run(() =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            // ReSharper disable once AccessToModifiedClosure
                            md5.TransformBlock(buffer, 0, read, buffer, 0);
                        });
                        totalRead += read;
                    } while ((totalRead < count || count < 0) && read > 0);
                    return await Task.Run(() =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        md5.TransformFinalBlock(new byte[0], 0, 0);
                        return md5.Hash;
                        // ReSharper restore AccessToDisposedClosure
                    });
                }
            }
        }

        public async Task<bool> IsEqual(FileInfo file)
        {
            return await IsEqual(file, 0);
        }

        public async Task<bool> IsEqual(FileInfo file, long start)
        {
            return await IsEqual(file, start, -1);
        }

        public async Task<bool> IsEqual(FileInfo file, long start, long count)
        {
            return await IsEqual(0, -1, file, start, count);
        }

        public async Task<bool> IsEqual(long fstStart, long count, FileInfo file, long sndCount)
        {
            return await IsEqual(fstStart, count, file, sndCount, -1);
        }

        public async Task<bool> IsEqual(long fstStart, FileInfo file, long sndCount)
        {
            return await IsEqual(fstStart, -1, file, sndCount);
        }

        public async Task<bool> IsEqual(long start, long count, FileInfo file)
        {
            return await IsEqual(start, count, file, 0);
        }

        public async Task<bool> IsEqual(long start, FileInfo file)
        {
            return await IsEqual(start, -1, file, 0, -1);
        }

        public async Task<bool> IsEqual(long fstStart, long fstCount, FileInfo file, long sndStart, long sndCount)
        {
            var firstMd5 = await GetMd5(File, fstStart, fstCount);
            var secondMd5 = await GetMd5(file, sndStart, sndCount);
            return firstMd5.SequenceEqual(secondMd5);
        }
    }
}