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

using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.IO;

namespace Reminiscence.Indexes
{
    /// <summary>
    /// An index mapping variable-sized elements to id's. The id's represent the position as if a continuous byte stream.
    /// </summary>
    public class Index<T> : IDisposable, ISerializableToStream
    {
        private readonly MemoryMap.CreateAccessorFunc<T> _createAccessor;
        private readonly System.Collections.Generic.List<MappedAccessor<T>> _accessors;
        private readonly System.Collections.Generic.List<long> _accessorBytesLost;
        private readonly long _accessorSize;
        private readonly MemoryMap _map;

        /// <summary>
        /// Creates a new index based on one fixed-size accessor.
        /// </summary>
        public Index(MappedAccessor<T> accessor)
        {
            _accessors = new System.Collections.Generic.List<MappedAccessor<T>>();
            _accessors.Add(accessor);
            _accessorBytesLost = new System.Collections.Generic.List<long>();
            _accessorBytesLost.Add(0);
            _accessorSize = accessor.Capacity;
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public Index()
            : this(new MemoryMapStream())
        {

        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public Index(MemoryMap map)
            : this(map, 1024)
        {

        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public Index(MemoryMap map, int accessorSize)
        {
            _map = map;
            _createAccessor = MemoryMap.GetCreateAccessorFuncFor<T>();
            _accessorSize = accessorSize;
            _accessors = new System.Collections.Generic.List<MappedAccessor<T>>();
            _accessors.Add(_createAccessor(_map, _accessorSize));
            _accessorBytesLost = new System.Collections.Generic.List<long>();
            _accessorBytesLost.Add(0);
        }

        private long _nextPositionInBytes = 0; // the next position in bytes with the bytes lost.
        private long _bytesLost = 0; // the amount of bytes lost when the last element doesn't fit into an accessor.

        /// <summary>
        /// Returns true if this index is readonly.
        /// </summary>
        public bool IsReadonly
        {
            get
            {
                return _createAccessor == null;
            }
        }

        /// <summary>
        /// Adds a new element.
        /// </summary>
        public long Add(T element)
        {
            if (this.IsReadonly) { throw new InvalidOperationException("Cannot add new element, index is readonly."); }

            var accessor = _accessors[_accessors.Count - 1]; // always write to the last accessor.
            var size = accessor.WriteTo(_nextPositionInBytes - ((_accessors.Count - 1) * _accessorSize), ref element);
            if(size < 0)
            { // write failed.
                // calculate bytes lost and increase.
                var lastAccessorBytesLost = (_accessors.Count * _accessorSize)  - _nextPositionInBytes;
                _accessorBytesLost[_accessorBytesLost.Count - 1] = lastAccessorBytesLost;
                _bytesLost += lastAccessorBytesLost;

                // add/get new accessor.
                _accessors.Add(_createAccessor(_map, _accessorSize));
                _accessorBytesLost.Add(0);
                accessor = _accessors[_accessors.Count - 1];

                // calculate new next position.
                _nextPositionInBytes = (_accessors.Count - 1) * _accessorSize;
                size = accessor.WriteTo(_nextPositionInBytes - ((_accessors.Count - 1) * _accessorSize), ref element);
                if(size < 0)
                {
                    throw new System.Exception("Cannot write element bigger than one individual accessor.");
                }
            }
            var id = _nextPositionInBytes - _bytesLost;
            _nextPositionInBytes += size;
            return id;
        }

        /// <summary>
        /// Gets the element with the given id.
        /// </summary>
        public T Get(long id)
        {
            // calculate accessor id.
            var a = 0;
            var accessorBytesLostPrevious = 0L;
            var accessorBytesLost = _accessorBytesLost[a];
            var accessorBytesOffset = _accessorSize - accessorBytesLost;
            while(accessorBytesOffset <= id)
            { // keep looping until the accessor is found where the data is located.
                a++;
                if (a >= _accessors.Count)
                {
                    throw new System.Exception("Cannot read elements with an id outside of the accessor range.");
                }
                accessorBytesLostPrevious = accessorBytesLost;
                accessorBytesLost += _accessorBytesLost[a]; 
                accessorBytesOffset = (_accessorSize * (a + 1)) - accessorBytesLost;
            }
            var accessor = _accessors[a];
            var accessorOffset = id + accessorBytesLostPrevious - (_accessorSize * a);
            var result = default(T);
            if (accessor.ReadFrom(accessorOffset, ref result) < 0)
            {
                throw new System.Exception("Failed to read element, perhaps an invalid id was given.");
            }
            return result;
        }

        /// <summary>
        /// Returns the total size in bytes.
        /// </summary>
        public long SizeInBytes
        {
            get
            {
                if(_createAccessor == null)
                { // only one accessor here, no next position or lost bytes.
                    return _accessors[0].Capacity;
                }
                return _nextPositionInBytes - _bytesLost;
            }
        }

        /// <summary>
        /// Disposes all native resources associated with this index.
        /// </summary>
        public void Dispose()
        {
            // dispose only the accessors, the file may still be in use.
            foreach (var accessor in _accessors)
            {
                accessor.Dispose();
            }
        }

        /// <summary>
        /// Copies this index to the given stream.
        /// </summary>
        public long CopyToWithSize(Stream stream)
        {
            // write size first.
            stream.Write(BitConverter.GetBytes(this.SizeInBytes), 0, 8);

            // write data.
            return this.CopyTo(stream) + 8;
        }

        /// <summary>
        /// Copies this index to the given stream.
        /// </summary>
        public long CopyTo(Stream stream)
        {
            if (_createAccessor == null)
            { // just one fixed-size accessor here.
                _accessors[0].CopyTo(stream);
            }
            else
            { // write and remove the gaps.
                for (var i = 0; i < _accessors.Count; i++)
                {
                    var bytesAtStart = i * _accessorSize;
                    if (_nextPositionInBytes > bytesAtStart + _accessorSize)
                    { // copy the full accessor.
                        _accessors[i].CopyTo(stream, 0, (int)(_accessorSize - _accessorBytesLost[i]));
                    }
                    else
                    { // copy part of the accessor.
                        _accessors[i].CopyTo(stream, 0, (int)(_nextPositionInBytes - bytesAtStart));
                    }
                }
            }
            return this.SizeInBytes;
        }

        /// <summary>
        /// Creates an index from the data in the given stream.
        /// </summary>
        public static Index<T> CreateFromWithSize(Stream stream, bool useAsMap = false)
        {
            long size;
            return Index<T>.CreateFromWithSize(stream, out size, useAsMap);
        }

        /// <summary>
        /// Creates an index from the data in the given stream.
        /// </summary>
        public static Index<T> CreateFromWithSize(Stream stream, out long size, bool useAsMap = false)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            size = BitConverter.ToInt64(bytes, 0);

            return Index<T>.CreateFrom(stream, size, useAsMap);
        }

        /// <summary>
        /// Copies the data from the stream to this index.
        /// </summary>
        public static Index<T> CreateFrom(Stream stream, long size, bool useAsMap = false)
        {
            if (useAsMap)
            { // use the existing stream as map.
                var map = new MemoryMapStream(new CappedStream(stream, stream.Position, size));
                var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(map, size);
                return new Index<T>(accessor);
            }
            else
            { // copy to memory stream and release the given stream.
                var data = new byte[size];
                var position = stream.Position;
                stream.Read(data, 0, (int)size);
                var map = new MemoryMapStream(new CappedStream(new MemoryStream(data), 0, size));
                var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(map, size);
                return new Index<T>(accessor);
            }
        }
    }
}