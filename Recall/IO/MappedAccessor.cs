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
        private MappedFile _file; // Holds the file that created this accessor.
        protected Stream _stream; // Holds the stream.
        protected int _elementSize; // The size of a single element.

        /// <summary>
        /// Creates a new mapped accessor.
        /// </summary>
        public MappedAccessor(MappedFile file, Stream stream, int elementSize)
        {
            if (file == null) { throw new ArgumentNullException("file"); }
            if (stream == null) { throw new ArgumentNullException("stream"); }
            if (!stream.CanSeek) { throw new ArgumentException("Stream to create a memory mapped file needs to be seekable."); }
            if (elementSize == 0 || elementSize < -1) { throw new ArgumentOutOfRangeException("elementSize need to be -1 or in the range of ]0-n]."); }

            _file = file;
            _stream = stream;
            _elementSize = elementSize;
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
        /// Returns true if this accessor is for elements of a fixed size.
        /// </summary>
        public virtual bool ElementSizeFixed
        {
            get
            {
                return _elementSize != -1;
            }
        }

        /// <summary>
        /// Reads appropriate amount of bytes from the given stream at the given position and returns the structure.
        /// </summary>
        public abstract long ReadFrom(Stream stream, long position, ref T structure);

        /// <summary>
        /// Reads appropriate amount of bytes from the default stream at the given position and returns the structure.
        /// </summary>
        public virtual long ReadFrom(long position, ref T structure)
        {
            return this.ReadFrom(_stream, position, ref structure);
        }

        /// <summary>
        /// Converts the structure to bytes and writes them to the given stream.
        /// </summary>
        public abstract long WriteTo(Stream stream, long position, ref T structure);

        /// <summary>
        /// Converts the structure to bytes and writes them to the stream default.
        /// </summary>
        public virtual long WriteTo(long position, ref T structure)
        {
            return this.WriteTo(_stream, position, ref structure);
        }

        /// <summary>
        /// Reads elements starting at the given position.
        /// </summary>
        public virtual int ReadArray(long position, T[] array, int offset, int count)
        {
            var structure = default(T);
            for (int i = 0; i < count; i++)
            {
                this.ReadFrom(_stream, i * _elementSize, ref structure);
                array[i + offset] = structure;
            }
            return count;
        }

        /// <summary>
        /// Writes an array of elements at the given position.
        /// </summary>
        public virtual long WriteArray(long position, T[] array, int offset, int count)
        {
            long size = 0;
            _stream.Seek(position, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                size = size + this.WriteTo(_stream, _stream.Position, 
                    ref array[i + offset]);
            }
            return size;
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream.
        /// </summary>
        public void CopyTo(Stream stream)
        {
            this.CopyTo(stream, 0, (int)_stream.Length);
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream starting at the given position until position + length.
        /// </summary>
        public void CopyTo(Stream stream, long position, int length)
        {
            this.CopyTo(stream, position, length, new byte[1024]);
        }

        /// <summary>
        /// Copies the data in this accessor to the given stream starting at the given position until position + length.
        /// </summary>
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