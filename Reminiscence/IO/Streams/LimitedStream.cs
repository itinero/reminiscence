// The MIT License (MIT)

// Copyright (c) 2015 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.IO;

namespace Reminiscence.IO.Streams
{
    /// <summary>
    /// Wraps a stream to prevent some fixed data from being overwritten.
    /// </summary>
    public class LimitedStream : Stream
    {
        private readonly long _offset;
        private readonly Stream _stream;

        /// <summary>
        /// Creates a new limited stream.
        /// </summary>
        public LimitedStream(Stream stream)
        {
            _stream = stream;
            _offset = _stream.Position;
        }

        /// <summary>
        /// Creates a new limited stream.
        /// </summary>
        public LimitedStream(Stream stream, long offset)
        {
            _stream = stream;
            _offset = offset;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            _stream.Flush();
        }

        /// <summary>
        /// Returns the current length of this stream.
        /// </summary>
        public override long Length
        {
            get { return _stream.Length - _offset; }
        }

        /// <summary>
        /// Gets/sets the current position.
        /// </summary>
        public override long Position
        {
            get { return _stream.Position - _offset; }
            set { _stream.Position = value + _offset; }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current
        ///     stream and advances the position within the stream by the number of bytes
        ///     read.
        /// </summary>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current
        ///     stream.
        /// </summary>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                return _stream.Seek(offset + _offset, origin);
            }
            return _stream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        public override void SetLength(long value)
        {
            _stream.SetLength(value + _offset);
        }

        /// <summary>
        ///  Writes a sequence of bytes to the current
        ///     stream and advances the current position within this stream by the number
        ///     of bytes written.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}