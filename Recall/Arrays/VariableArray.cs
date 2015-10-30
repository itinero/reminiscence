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

using Recall.Indexes;

namespace Recall.Arrays
{
    /// <summary>
    /// A memory mapped array with elements of variable size.
    /// </summary>
    public class VariableArray<T> : ArrayBase<T>
    {
        private readonly Array<long> _index;
        private readonly Index<T> _data;

        /// <summary>
        /// Creates a new array.
        /// </summary>
        public VariableArray(IO.MappedDelegates.CreateAccessorFunc<long> createInt64Accessor,
            IO.MappedDelegates.CreateAccessorFunc<T> createAccessor, int accessorSize, long size)
        {
            _index = new Array<long>(createInt64Accessor, 8, size);
            _data = new Index<T>(createAccessor, accessorSize);
        }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        public sealed override T this[long idx]
        {
            get
            {
                var id = _index[idx];
                if(id == 0)
                {
                    return default(T);
                }
                return _data.Get(id - 1);
            }
            set
            {
                var id = _data.Add(value);
                _index[idx] = id + 1;
            }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        public sealed override void Resize(long size)
        {
            _index.Resize(size);
        }

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public sealed override long Length
        {
            get { return _index.Length; }
        }

        /// <summary>
        /// Disposes of all resources associated with this array.
        /// </summary>
        public override void Dispose()
        {
            _index.Dispose();
            _data.Dispose();
        }
    }
}