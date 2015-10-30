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

using Recall.IO;
using System;

namespace Recall.Indexes
{
    /// <summary>
    /// An index mapping variable-sized elements to id's.
    /// </summary>
    public class Index<T> : IDisposable
    {
        private readonly MappedDelegates.CreateAccessorFunc<T> _createAccessor;
        private readonly System.Collections.Generic.List<MappedAccessor<T>> _accessors;
        private readonly int _accessorSize;

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public Index(MappedDelegates.CreateAccessorFunc<T> createAccessor)
            : this(createAccessor, 1024 * 1024)
        {

        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public Index(MappedDelegates.CreateAccessorFunc<T> createAccessor, 
            int accessorSize)
        {
            _createAccessor = createAccessor;
            _accessorSize = accessorSize;
            _accessors = new System.Collections.Generic.List<MappedAccessor<T>>();
            _accessors.Add(createAccessor(_accessorSize));
        }

        private long _nextPosition = 0;

        /// <summary>
        /// Adds a new element.
        /// </summary>
        public long Add(T element)
        {
            var accessor = _accessors[_accessors.Count - 1];
            var size = accessor.WriteTo(_nextPosition - ((_accessors.Count - 1) * _accessorSize), ref element);
            if(size < 0)
            { // write failed.
                // add/get new accessor.
                _accessors.Add(_createAccessor(_accessorSize));
                accessor = _accessors[_accessors.Count - 1];

                // calculate new next position.
                _nextPosition = (_accessors.Count - 1) * _accessorSize;
                size = accessor.WriteTo(_nextPosition - ((_accessors.Count - 1) * _accessorSize), ref element);
                if(size < 0)
                {
                    throw new System.Exception("Cannot write element bigger than one individual accessor.");
                }
            }
            var id = _nextPosition;
            _nextPosition += size;
            return id;
        }

        /// <summary>
        /// Gets the element with the given id.
        /// </summary>
        public T Get(long id)
        {
            var accessorId = (int)System.Math.Floor(id / _accessorSize);
            if (accessorId >= _accessors.Count)
            {
                throw new System.Exception("Cannot read elements with an id outside of the accessor range.");
            }
            var accessor = _accessors[accessorId];
            var result = default(T);
            if(accessor.ReadFrom(id - (accessorId * _accessorSize), ref result) < 0)
            {
                throw new System.Exception("Failed to read element, perhaps an invalid id was given.");
            }
            return result;
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
    }
}