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

namespace Recall.IO
{
    /// <summary>
    /// Abstract representation of a memory-mapped accessor: Provides random access to unmanaged blocks of memory from managed code.
    /// </summary>
    public abstract class MappedAccessor<T> : IDisposable
    {
        /// <summary>
        /// Holds the file that created this accessor.
        /// Need to keep track of this to make sure everything is disposed correctly no matter what!
        /// </summary>
        private MappedFile _file;

        /// <summary>
        /// Holds the stream.
        /// </summary>
        protected Stream _stream;

        /// <summary>
        /// The buffer to use while read/writing.
        /// </summary>
        protected byte[] _buffer;

        /// <summary>
        /// The size of a single element.
        /// </summary>
        protected int _elementSize;

        /// <summary>
        /// Creates a new memory mapped accessor.
        /// </summary>
        /// <param name="file">The file that created this memory mapped accessor.</param>
        /// <param name="stream">The stream containing the data.</param>
        /// <param name="elementSize">The element size.</param>
        public MappedAccessor(MappedFile file, Stream stream, int elementSize)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (stream == null) { throw new ArgumentNullException("stream"); }
            if (!stream.CanSeek) { throw new ArgumentException("Stream to create a memory mapped file needs to be seekable."); }
            if (elementSize == 0 || elementSize < -1) { throw new ArgumentOutOfRangeException("elementSize need to be -1 or in the range of ]0-n]."); }

            _file = file;
            _stream = stream;
            _elementSize = elementSize;
            if (_elementSize > 0)
            { // 64 element in buffer by default.
                _buffer = new byte[64 * elementSize];
            }
            else
            { // use a default size when element size is variable.
                _buffer = new byte[1024];
            }
        }

        /// <summary>
        /// Determines whether the accessory is writable.
        /// </summary>
        public virtual bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        /// <summary>
        /// Gets the capacity of this memory mapped file in bytes.
        /// </summary>
        public long Capacity
        {
            get
            {
                return _stream.Length;
            }
        }

        /// <summary>
        /// Gets the capacity of this memory mapped file in the number of structs it can store.
        /// </summary>
        public long CapacityElements
        {
            get
            {
                return _stream.Length / this.ElementSize;
            }
        }

        /// <summary>
        /// Gets the size in bytes of one element.
        /// </summary>
        public int ElementSize
        {
            get { return _elementSize; }
        }

        /// <summary>
        /// Reads from the buffer at the given position.
        /// </summary>
        /// <param name="position">The position to read from.</param>
        /// <returns></returns>
        protected virtual T ReadFrom(int position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes to the stream.
        /// </summary>
        /// <param name="structure">The structure to write to.</param>
        /// <rereturns>The number of bytes written.</rereturns>
        protected virtual long WriteTo(T structure)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads one element at the given position.
        /// </summary>
        /// <param name="position">The position to read from.</param>
        /// <param name="structure">The resulting structure.</param>
        public virtual void Read(long position, out T structure)
        {
            _stream.Seek(position, SeekOrigin.Begin);
            _stream.Read(_buffer, 0, _elementSize);
            structure = this.ReadFrom(0);
        }

        /// <summary>
        /// Reads elements starting at the given position.
        /// </summary>
        /// <param name="position">The position to read.</param>
        /// <param name="array">The array to fill with the resulting data.</param>
        /// <param name="offset">The offset to start filling the array.</param>
        /// <param name="count">The number of elements to read.</param>
        /// <returns></returns>
        public virtual int ReadArray(long position, T[] array, int offset, int count)
        {
            if (_buffer.Length < count * _elementSize)
            { // increase buffer if needed.
                Array.Resize(ref _buffer, count * _elementSize);
            }
            _stream.Seek(position, SeekOrigin.Begin);
            _stream.Read(_buffer, 0, count * _elementSize);
            for (int i = 0; i < count; i++)
            {
                array[i + offset] = this.ReadFrom(i * _elementSize);
            }
            return count;
        }

        /// <summary>
        /// Writes an element at the given position.
        /// </summary>
        /// <param name="position">The position to write to.</param>
        /// <param name="structure">The structure.</param>
        public virtual long Write(long position, ref T structure)
        {
            _stream.Seek(position, SeekOrigin.Begin);
            return this.WriteTo(structure);
        }

        /// <summary>
        /// Writes an array of elements at the given position.
        /// </summary>
        /// <param name="position">The position to write to.</param>
        /// <param name="array">The array to with the data.</param>
        /// <param name="offset">The offset to start using the array at.</param>
        /// <param name="count">The number of elements to write.</param>
        public virtual long WriteArray(long position, T[] array, int offset, int count)
        {
            long size = 0;
            _stream.Seek(position, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                size = size + this.WriteTo(array[i + offset]);
            }
            return size;
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream.
        /// </summary>
        /// <param name="stream"></param>
        public void CopyTo(Stream stream)
        {
            this.CopyTo(stream, 0, (int)_stream.Length);
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream starting at the given position until position + length.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        public void CopyTo(Stream stream, long position, int length)
        {
            this.CopyTo(stream, position, length, _buffer);
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream starting at the given position until position + length.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="buffer"></param>
        public void CopyTo(Stream stream, long position, int length, byte[] buffer)
        {
            _stream.Seek(position, SeekOrigin.Begin);
            while (length > buffer.Length)
            {
                _stream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);

                length = length - buffer.Length;
            }
            _stream.Read(buffer, 0, length);
            stream.Write(buffer, 0, length);
        }

        /// <summary>
        /// Diposes of all native resources associated with this object.
        /// </summary>
        public virtual void Dispose()
        {
            _file.Disposed<T>(this);
            _file = null;
        }
    }
}