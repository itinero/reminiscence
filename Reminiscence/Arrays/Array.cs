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

using Reminiscence.Arrays.Cache;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// A memory mapped array for elements of fixed size.
    /// </summary>
    public class Array<T> : ArrayBase<T>
    {
        private readonly List<MappedAccessor<T>> _accessors;
        private readonly long _accessorSize = DefaultAccessorSize;
        private readonly MemoryMap.CreateAccessorFunc<T> _createAccessor;
        private readonly MemoryMap _map;

        /// <summary>
        /// The default element size of one accessor.
        /// </summary>
        public static long DefaultAccessorSize = 1024;
        /// <summary>
        /// The default buffer size.
        /// </summary>
        public static int DefaultBufferSize = 128;
        /// <summary>
        /// The default cache size.
        /// </summary>
        public static int DefaultCacheSize = 64 * 8;

        /// <summary>
        /// Creates a memory mapped array based on existing data.
        /// </summary>
        public Array(MappedAccessor<T> accessor)
            : this(accessor, DefaultBufferSize, DefaultCacheSize)
        {

        }

        /// <summary>
        /// Creates a memory mapped array based on existing data.
        /// </summary>
        public Array(MappedAccessor<T> accessor, ArrayProfile profile)
            : this(accessor, profile.BufferSize, profile.CacheSize)
        {

        }

        /// <summary>
        /// Creates a memory mapped array based on existing data.
        /// </summary>
        public Array(MappedAccessor<T> accessor, int bufferSize, int cacheSize)
        {
            _accessors = new List<MappedAccessor<T>>();
            _accessors.Add(accessor);
            _accessorSize = accessor.CapacityElements;
            _createAccessor = null;

            // create first accessor.
            _length = accessor.CapacityElements;

            _bufferSize = bufferSize;
            _cachedBuffer = null;
            _cachedBuffers = new LRUCache<long, CachedBuffer>(cacheSize);
            _cachedBuffers.OnRemove += new LRUCache<long, CachedBuffer>.OnRemoveDelegate(buffer_OnRemove);
        }

        /// <summary>
        /// Creates a memory mapped array.
        /// </summary>
        public Array(MemoryMap map, long length)
            : this(map, length, 1024, 1024, 32)
        {

        }

        /// <summary>
        /// Creates a memory mapped array.
        /// </summary>
        public Array(MemoryMap map, long length, 
            ArrayProfile profile)
            : this(map, length, 1024, profile.BufferSize, profile.CacheSize)
        {

        }

        /// <summary>
        /// Creates a memory mapped array.
        /// </summary>
        public Array(MemoryMap map, long length, 
            long accessorSize, int bufferSize, int cacheSize)
        {
            if (accessorSize < 0) { throw new ArgumentOutOfRangeException("accessorSize"); }
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }

            _map = map;
            _accessors = new List<MappedAccessor<T>>();
            _accessorSize = accessorSize;
            _createAccessor = MemoryMap.GetCreateAccessorFuncFor<T>();

            // create first accessor.
            var accessor = _createAccessor(_map, _accessorSize);
            _accessors.Add(accessor);
            _length = length;

            _bufferSize = bufferSize;
            _cachedBuffer = null;
            _cachedBuffers = new LRUCache<long, CachedBuffer>(cacheSize);
            _cachedBuffers.OnRemove += new LRUCache<long, CachedBuffer>.OnRemoveDelegate(buffer_OnRemove);

            var blockCount = (int)System.Math.Max(System.Math.Ceiling((double)length / _accessorSize), 1);
            for (int i = 1; i < blockCount; i++)
            {
                _accessors.Add(_createAccessor(_map, _accessorSize));
            }
        }

        private long _length;

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public sealed override long Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Returns the size of one element in bytes.
        /// </summary>
        public int ElementSizeInBytes
        {
            get { return _accessors[0].ElementSize; }
        }

        /// <summary>
        /// Returns true if this array can be resized.
        /// </summary>
        public override bool CanResize
        {
            get { return _createAccessor != null; }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        public sealed override void Resize(long size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException(); }
            if (!this.CanResize) { throw new InvalidOperationException("Array cannot be resized."); }

            // clear cache (and save dirty blocks).
            _cachedBuffers.Clear();
            _cachedBuffer = null;

            // store old size.
            var oldSize = _length;
            _length = size;

            var blockCount = (int)System.Math.Ceiling((double)size / _accessorSize);
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
                    _accessors.Add(_createAccessor(_map, _accessorSize));
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
                var arrayIdx = (long)System.Math.Floor(buffer.Position / _accessorSize);
                var localIdx = buffer.Position % _accessorSize;
                var localPosition = localIdx * _accessors[(int)arrayIdx].ElementSize;

                if (buffer.Position + _bufferSize > _length)
                { // only partially write buffer, do not write past the end.
                    _accessors[(int)arrayIdx].WriteArray(localPosition, buffer.Buffer, 0, 
                        (int)(_length - buffer.Position));
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

                    var arrayIdx = (long)System.Math.Floor(bufferPosition / _accessorSize);
                    var localIdx = bufferPosition % _accessorSize;
                    var localPosition = localIdx * _accessors[(int)arrayIdx].ElementSize;

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