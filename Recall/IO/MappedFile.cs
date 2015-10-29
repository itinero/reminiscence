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
using System.Collections.Generic;
using System.IO;

namespace Recall.IO
{
    /// <summary>
    /// Represents a memory mapped file.
    /// </summary>
    public abstract class MappedFile : IDisposable
    {
        private readonly List<IDisposable> _accessors; // Holds all acessors generated for this file.

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedFile()
        {
            _accessors = new List<IDisposable>();
            _nextPosition = 0;
        }

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        /// <param name="position">The position to start this accessor at.</param>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<uint> CreateUInt32(long position, long sizeInBytes)
        {
            var accessor = this.DoCreateNewUInt32(position, sizeInBytes);
            _accessors.Add(accessor);

            var nextPosition = position + sizeInBytes;
            if (nextPosition > _nextPosition)
            {
                _nextPosition = nextPosition;
            }

            return accessor;
        }

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<uint> CreateUInt32(long sizeInBytes)
        {
            var accessor = this.DoCreateNewUInt32(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        private long _nextPosition; // Holds the next position of a new empty accessor.

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInByte">The size.</param>
        /// <returns></returns>
        protected abstract MappedAccessor<int> DoCreateNewInt32(long position, long sizeInByte);

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<int> CreateInt32(long sizeInBytes)
        {
            var accessor = this.DoCreateNewInt32(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInByte">The size.</param>
        /// <returns></returns>
        protected abstract MappedAccessor<uint> DoCreateNewUInt32(long position, long sizeInByte);

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        /// <param name="position">The position to start this accessor at.</param>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<float> CreateSingle(long position, long sizeInBytes)
        {
            var accessor = this.DoCreateNewSingle(position, sizeInBytes);
            _accessors.Add(accessor);

            var nextPosition = position + sizeInBytes;
            if (nextPosition > _nextPosition)
            {
                _nextPosition = nextPosition;
            }

            return accessor;
        }

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<float> CreateSingle(long sizeInBytes)
        {
            var accessor = this.DoCreateNewSingle(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInByte">The size.</param>
        /// <returns></returns>
        protected abstract MappedAccessor<float> DoCreateNewSingle(long position, long sizeInByte);

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        /// <param name="position">The position to start this accessor at.</param>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<ulong> CreateUInt64(long position, long sizeInBytes)
        {
            var accessor = this.DoCreateNewUInt64(position, sizeInBytes);
            _accessors.Add(accessor);

            var nextPosition = position + sizeInBytes;
            if (nextPosition > _nextPosition)
            {
                _nextPosition = nextPosition;
            }

            return accessor;
        }

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<ulong> CreateUInt64(long sizeInBytes)
        {
            var accessor = this.DoCreateNewUInt64(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInByte">The size.</param>
        /// <returns></returns>
        protected abstract MappedAccessor<ulong> DoCreateNewUInt64(long position, long sizeInByte);

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <returns></returns>
        public MappedAccessor<long> CreateInt64(long sizeInBytes)
        {
            var accessor = this.DoCreateNewInt64(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        /// <param name="position">The position to start at.</param>
        /// <param name="sizeInByte">The size.</param>
        /// <returns></returns>
        protected abstract MappedAccessor<long> DoCreateNewInt64(long position, long sizeInByte);

        /// <summary>
        /// A delegate to facilitate reading a variable-sized object.
        /// </summary>
        public delegate long ReadFromDelegate<T>(Stream stream, long position, ref T structure);

        /// <summary>
        /// A delegate to facilitate writing a variable-sized object.
        /// </summary>
        public delegate long WriteToDelegate<T>(Stream stream, long position, ref T structure);

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        /// <param name="sizeInBytes">The size of this accessor.</param>
        /// <param name="readFrom">The delegate to read a structure.</param>
        /// <param name="writeTo">The delegate to write a structure.</param>
        /// <returns></returns>
        public MappedAccessor<T> CreateVariable<T>(long sizeInBytes,
            ReadFromDelegate<T> readFrom, WriteToDelegate<T> writeTo)
        {
            var accessor = this.DoCreateVariable<T>(_nextPosition, sizeInBytes, readFrom, writeTo);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<T> DoCreateVariable<T>(long _nextPosition, long sizeInBytes, 
            ReadFromDelegate<T> readFrom, WriteToDelegate<T> writeTo);

        /// <summary>
        /// Notifies this factory that the given file was already disposed. This given the opportunity to dispose of files without disposing the entire factory.
        /// </summary>
        internal void Disposed<T>(MappedAccessor<T> fileToDispose)
        {
            _accessors.Remove(fileToDispose);
        }

        /// <summary>
        /// Disposes of all resources associated with this files.
        /// </summary>
        public virtual void Dispose()
        {
            while (_accessors.Count > 0)
            {
                _accessors[0].Dispose();
            }
            _accessors.Clear();
        }
    }
}