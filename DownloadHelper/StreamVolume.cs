using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadHelper
{
    public class StreamVolume : Stream
    {
        private long _position;
        private bool _closed;

        public StreamVolume(Stream mainStream, StreamVolumeRange range)
        {
            UnderlyingStream = mainStream;
            Range = range;
            _position = 0;
        }

        /// <inheritdoc />
        public override bool CanRead => UnderlyingStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => UnderlyingStream.CanWrite;

        /// <inheritdoc />
        public override long Length => Range.HasLength ? Range.Length : UnderlyingStream.Length - Range.Start;

        /// <inheritdoc />
        public override long Position
        {
            get { return _position; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public StreamVolumeRange Range { get; set; }

        public Stream UnderlyingStream { get; }

        /// <inheritdoc />
        public override void Close()
        {
            lock (this)
            {
                _closed = true;
                //Range = Range.ShrinkLength(_position);
            }
        }

        /// <inheritdoc />
        public override void Flush()
        {
            lock (UnderlyingStream)
            {
                UnderlyingStream.Flush();
            }
        }

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            lock (UnderlyingStream)
            {
                return UnderlyingStream.FlushAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (UnderlyingStream)
            {
                if (UnderlyingStream.CanSeek)
                    Seek(Position, SeekOrigin.Begin);
                var read = UnderlyingStream.Read(buffer, offset, count);
                _position += read;
                return read;
            }
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (UnderlyingStream)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _position = UnderlyingStream.Seek(offset + Range.Start, SeekOrigin.Begin) - Range.Start;
                        break;
                    case SeekOrigin.Current:
                        _position = UnderlyingStream.Seek(offset + Range.Start + Position, SeekOrigin.Begin) - Range.Start;
                        break;
                    case SeekOrigin.End:
                        if (!Range.HasLength)
                            _position = UnderlyingStream.Seek(offset, SeekOrigin.End) - Range.Start;
                        else
                            _position = UnderlyingStream.Seek(Range.End - offset, SeekOrigin.Begin) - Range.Start;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }
                return _position;
            }
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            if (!CanSeek)
                throw new NotSupportedException("Stream does not support seeking.");
            if (value < 0)
                throw new ArgumentException("New length can not be negative number.");
            //UnderlyingStream.SetLength(value + Range.Start);
            //_position = UnderlyingStream.Position - Range.Start < 0 ? 0 : UnderlyingStream.Position - Range.Start;
            _position = value;
            //if (Range.HasLength)
            //Range = Range.ShrinkLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (UnderlyingStream)
            {
                if (UnderlyingStream.CanSeek)
                    Seek(Position, SeekOrigin.Begin);
                UnderlyingStream.Write(buffer, offset, count);
                _position += count;
            }
        }
    }
}