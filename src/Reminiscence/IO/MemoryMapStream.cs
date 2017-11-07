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

using Reminiscence.IO.Streams;
using System.IO;

namespace Reminiscence.IO
{
    /// <summary>
    /// A mapped file that is using a single stream.
    /// </summary>
    public class MemoryMapStream : MemoryMap
    {
        private Stream _stream; // Holds the stream.

        /// <summary>
        /// Creates a new mapped stream using a memory stream.
        /// </summary>
        public MemoryMapStream()
            : this(new MemoryStream())
        {

        }

        /// <summary>
        /// Creates a new mapped stream.
        /// </summary>
        /// <param name="stream">The stream to read/write.</param>
        public MemoryMapStream(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<uint> DoCreateNewUInt32(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorUInt32(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<ushort> DoCreateNewUInt16(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorUInt16(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<int> DoCreateNewInt32(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorInt32(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<short> DoCreateNewInt16(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorInt16(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<float> DoCreateNewSingle(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorSingle(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<double> DoCreateNewDouble(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorDouble(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<ulong> DoCreateNewUInt64(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorUInt64(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <returns></returns>
        protected override MappedAccessor<long> DoCreateNewInt64(long position, long sizeInBytes)
        {
            return new Accessors.MappedAccessorInt64(this, new CappedStream(_stream, position, sizeInBytes));
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInBytes">The size.</param>
        /// <param name="readFrom"></param>
        /// <param name="writeTo"></param>
        /// <returns></returns>
        protected override MappedAccessor<T> DoCreateVariable<T>(long position, long sizeInBytes, MemoryMap.ReadFromDelegate<T> readFrom, MemoryMap.WriteToDelegate<T> writeTo)
        {
            return new Accessors.MappedAccessorVariable<T>(this, new CappedStream(_stream, position, sizeInBytes), readFrom, writeTo);
        }
    }
}