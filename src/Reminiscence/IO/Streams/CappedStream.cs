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

using System;
using System.IO;

namespace Reminiscence.IO.Streams
{
    /// <summary>
    /// Represents a capped stream that can only be used along a given region.
    /// </summary>
    public class CappedStream : Stream
    {
        private readonly Stream _stream;
        private readonly long _offset;
        private readonly long _length;

        /// <summary>
        /// Creates a new capped stream.
        /// </summary>
        public CappedStream(Stream stream, long offset, long length)
        {
            _stream = stream;
            _stream.Seek(offset, SeekOrigin.Begin);
            _offset = offset;
            _length = length;
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
            get { return _length; }
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
            if (this.Position + count >= _length)
            {
                count = (int)(_length - this.Position);
                return _stream.Read(buffer, offset, count);
            }
            return _stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current
        ///     stream.
        /// </summary>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > _length)
            {
                throw new Exception("Cannot read past end of capped stream.");
            }
            return _stream.Seek(offset + _offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        public override void SetLength(long value)
        {
            _stream.SetLength(value + _offset);
        }

        /// <summary>
        ///  Tests if it's possible to write a sequence of bytes to the current
        ///     stream and advance the current position within this stream by the number
        ///     of bytes.
        /// </summary>
        /// <returns></returns>
        public bool WriteBytePossible()
        {
            return !(this.Position + 1 > this.Length);
        }

        /// <summary>
        ///  Tests if it's possible to write a sequence of bytes to the current
        ///     stream and advance the current position within this stream by the number
        ///     of bytes.
        /// </summary>
        /// <returns></returns>
        public bool WritePossible(int offset, int count)
        {
            return !(this.Position + count > this.Length);
        }

        /// <summary>
        ///  Writes a sequence of bytes to the current
        ///     stream and advances the current position within this stream by the number
        ///     of bytes written.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.Position + count > this.Length)
            {
                throw new Exception("Cannot write past end of capped stream.");
            }
            _stream.Write(buffer, offset, count);
        }
    }
}