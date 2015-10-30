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

using Recall.Arrays.Cache;
using Recall.IO;
using Recall.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;

namespace Recall.Arrays
{
    /// <summary>
    /// A memory mapped array for elements of fixed size.
    /// </summary>
    public class Array<T> : ArrayBase<T>
    {
        private long _length;
        private readonly List<MappedAccessor<T>> _accessors;
        private readonly int _elementSize;
        private readonly long _fileSizeBytes;
        private readonly long _fileElementSize = DefaultFileElementSize;
        private readonly MappedDelegates.CreateAccessorFunc<T> _createAccessor;
        public static long DefaultFileElementSize = (long)1024; // The default element file size.
        public static int DefaultBufferSize = 128; // The default buffer size.
        public static int DefaultCacheSize = 64 * 8; // The default cache size.
        /// <summary>
        /// Creates a memory mapped huge array.
        /// </summary>
        public Array(MappedDelegates.CreateAccessorFunc<T> createAccessor, int elementSize, long size)
            : this(createAccessor, elementSize, size, 1024, 1024, 32)
        {

        }

        /// <summary>
        /// Creates a memory mapped huge array.
        /// </summary>
        public Array(MappedDelegates.CreateAccessorFunc<T> createAccessor, int elementSize, long size, long arraySize, int bufferSize, int cacheSize)
        {
            if (createAccessor == null) { throw new ArgumentNullException("createAccessor"); }
            if (elementSize < 0) { throw new ArgumentOutOfRangeException("elementSize"); }
            if (arraySize < 0) { throw new ArgumentOutOfRangeException("arraySize"); }
            if (size < 0) { throw new ArgumentOutOfRangeException("size"); }

            _createAccessor = createAccessor;
            _length = size;
            _fileElementSize = arraySize;
            _elementSize = elementSize;
            _fileSizeBytes = arraySize * _elementSize;

            _bufferSize = bufferSize;
            _cachedBuffer = null;
            _cachedBuffers = new LRUCache<long, CachedBuffer>(cacheSize);
            _cachedBuffers.OnRemove += new LRUCache<long, CachedBuffer>.OnRemoveDelegate(buffer_OnRemove);

            var blockCount = (int)System.Math.Ceiling((double)size / _fileElementSize);
            _accessors = new List<MappedAccessor<T>>(blockCount);
            for (int i = 0; i < blockCount; i++)
            {
                _accessors.Add(_createAccessor(_fileSizeBytes));
            }
        }

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public sealed override long Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// 
        public sealed override void Resize(long size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException(); }

            // clear cache (and save dirty blocks).
            _cachedBuffers.Clear();
            _cachedBuffer = null;

            // store old size.
            var oldSize = _length;
            _length = size;

            var blockCount = (int)System.Math.Ceiling((double)size / _fileElementSize);
            if (blockCount < _accessors.Count)
            { // decrease files/accessors.
                for (int i = (int)blockCount; i < _accessors.Count; i++)
                {
                    _accessors[i].Dispose();
                    _accessors[i] = null;
                }
                _accessors.RemoveRange((int)blockCount, (int)(_accessors.Count - blockCount));
            }
            else
            { // increase files/accessors.
                for (int i = _accessors.Count; i < blockCount; i++)
                {
                    _accessors.Add(_createAccessor(_fileSizeBytes));
                }
            }
        }

        /// <summary>
        /// Returns the element at the given index.
        /// </summary>
        /// <returns></returns>
        public sealed override T this[long idx]
        {
            get
            {
                if (idx < 0 || idx >= _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                // sync buffer.
                var relativePosition = this.SyncBuffer(idx);
                return _cachedBuffer.Buffer[relativePosition];
            }
            set
            {
                if (idx < 0 || idx >= _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                // sync buffer.
                var relativePosition = this.SyncBuffer(idx);
                _cachedBuffer.Buffer[relativePosition] = value;
                _cachedBuffer.IsDirty = true;
            }
        }

        #region Buffering

        /// <summary>
        /// Holds all the cached buffers.
        /// </summary>
        private LRUCache<long, CachedBuffer> _cachedBuffers;

        /// <summary>
        /// Holds the buffer size.
        /// </summary>
        private int _bufferSize;

        /// <summary>
        /// Holds the last used cached buffer.
        /// </summary>
        private CachedBuffer _cachedBuffer = null;

        /// <summary>
        /// Called when an item is removed from the cache.
        /// </summary>
        void buffer_OnRemove(Array<T>.CachedBuffer item)
        {
            this.FlushBuffer(item);
        }

        /// <summary>
        /// Flushes the given buffer to disk.
        /// </summary>
        private void FlushBuffer(Array<T>.CachedBuffer buffer)
        {
            if (buffer.IsDirty)
            {
                long arrayIdx = (long)System.Math.Floor(buffer.Position / _fileElementSize);
                long localIdx = buffer.Position % _fileElementSize;
                long localPosition = localIdx * _elementSize;

                if (buffer.Position + _bufferSize > _length)
                { // only partially write buffer, do not write past the end.
                    _accessors[(int)arrayIdx].WriteArray(localPosition, buffer.Buffer, 0, (int)(_length - buffer.Position));
                    return;
                }
                _accessors[(int)arrayIdx].WriteArray(localPosition, buffer.Buffer, 0, _bufferSize);
            }
        }

        /// <summary>
        /// Flushes all buffers.
        /// </summary>
        private void FlushBuffers()
        {
            foreach(var buffer in _cachedBuffers)
            {
                this.FlushBuffer(buffer.Value);
            }
        }

        /// <summary>
        /// Syncs buffer.
        /// </summary>
        private int SyncBuffer(long idx)
        {
            // calculate the buffer position.
            var bufferPosition = idx - (idx % _bufferSize);

            // check buffer.
            if (_cachedBuffer == null ||
                _cachedBuffer.Position != bufferPosition)
            { // not in buffer.
                if (!_cachedBuffers.TryGet(bufferPosition, out _cachedBuffer))
                {
                    var newBuffer = new T[_bufferSize];

                    var arrayIdx = (long)System.Math.Floor(bufferPosition / _fileElementSize);
                    var localIdx = bufferPosition % _fileElementSize;
                    var localPosition = localIdx * _elementSize;

                    _accessors[(int)arrayIdx].ReadArray(localPosition, newBuffer, 0, _bufferSize);
                    _cachedBuffer = new CachedBuffer()
                    {
                        Buffer = newBuffer,
                        IsDirty = false,
                        Position = bufferPosition
                    };
                    _cachedBuffers.Add(_cachedBuffer.Position, _cachedBuffer);
                }
            }
            return (int)(idx - bufferPosition);
        }

        /// <summary>
        /// A cached buffer.
        /// </summary>
        private class CachedBuffer
        {
            /// <summary>
            /// Holds the position.
            /// </summary>
            public long Position;

            /// <summary>
            /// Holds the dirty flag.
            /// </summary>
            public bool IsDirty;

            /// <summary>
            /// Holds the buffer.
            /// </summary>
            public T[] Buffer;
        }

        #endregion

        /// <summary>
        /// Diposes of all native resource associated with this array.
        /// </summary>
        public sealed override void Dispose()
        {
            // clear cache.
            _cachedBuffers.Clear();

            // dispose only the accessors, the file may still be in use.
            foreach (var accessor in _accessors)
            {
                accessor.Dispose();
            }
        }

        /// <summary>
        /// Writes this array to the given stream.
        /// </summary>
        public long WriteTo(Stream stream)
        {
            // first flush all buffers.
            this.FlushBuffers();

            // first write length.
            stream.Write(BitConverter.GetBytes(_length), 0, 8);

            // read raw data from accessor(s) and write to stream.
            var element = 0;
            for(var i = 0; i < _accessors.Count; i++)
            {
                var accessor = _accessors[i];
                var elementsToRead = accessor.CapacityElements;
                if(elementsToRead + element > _length)
                {
                    elementsToRead = _length - element;
                }
                if(elementsToRead <= 0)
                {
                    break;
                }
                accessor.CopyTo(stream, 0, 
                    (int)elementsToRead * _elementSize);                
            }
            return (_elementSize * _length) + 8;
        }

        /// <summary>
        /// Reads an array from the given stream.
        /// </summary>
        public static Array<T> ReadFrom(Stream stream, MappedDelegates.CreateAccessorFunc<T> createAccessor,
            int elementSize)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);

            var size = BitConverter.ToInt64(bytes, 0);
            var array = new Array<T>(createAccessor, elementSize, size);
            using (var limitedStream = new LimitedStream(stream))
            {
                using (var map = new MappedStream(new LimitedStream(stream)))
                {
                    for (var i = 0; i < size; i++)
                    {
                        array.AddFrom(i, limitedStream, i * elementSize);
                    }
                }
            }
            return array;
        }

        /// <summary>
        /// Adds a new element from raw bytes.
        /// </summary>
        private void AddFrom(int i, Stream stream, long position)
        {
            var structure = default(T);
            _accessors[0].ReadFrom(stream, position, ref structure);
            this[i] = structure;
        }
    }
}