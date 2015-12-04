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

using Reminiscence.Arrays;
using Reminiscence.IO;
using System;
using System.Collections.Generic;

namespace Reminiscence.Collections
{
    /// <summary>
    /// A list of objects that can be accessed by index that uses a memory mapped array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class List<T> : IDisposable, IList<T>
    {
        private readonly ArrayBase<T> _data;

        /// <summary>
        /// Creates a new list.
        /// </summary>
        public List()
        {
            _data = new MemoryArray<T>(1024);
        }

        /// <summary>
        /// Creates a new list.
        /// </summary>
        public List(MemoryMap map)
            : this(map, 1024)
        {

        }

        /// <summary>
        /// Creates a new list.
        /// </summary>
        public List(MemoryMap map, long capacity)
        {
            _data = ArrayBase<T>.CreateFor(map, capacity, ArrayProfile.NoCache);
        }

        /// <summary>
        /// Creates a new list.
        /// </summary>
        public List(MemoryMap map, long capacity, ArrayProfile arrayProfile)
        {
            _data = ArrayBase<T>.CreateFor(map, capacity, arrayProfile);
        }

        private int _count = 0; // hold the current number of elements.
        
        /// <summary>
        /// Determines the index of a specific item.
        /// </summary>
        public int IndexOf(T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if(_data[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        public void Insert(int index, T item)
        {
            this.ResizeFor(_count + 1);

            for(var i = _count - 1; i >= index; i--)
            {
                _data[i + 1] = _data[i];
            }
            _data[index] = item;

            _count++;
        }

        /// <summary>
        /// Removes the item at the given index.
        /// </summary>
        public void RemoveAt(int index)
        {
            for (var i = index; i < _count; i++)
            {
                _data[i] = _data[i + 1];
            }

            _count--;
        }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        public T this[long index]
        {
            get
            {
                if (index < 0 || index >= _count) { throw new ArgumentOutOfRangeException("index"); }

                return _data[index];
            }
            set
            {
                if (index < 0 || index >= _count) { throw new ArgumentOutOfRangeException("index"); }

                _data[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the element at the given index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                return this[(long)index];
            }
            set
            {
                this[(long)index] = value;
            }
        }

        /// <summary>
        /// Adds a new item.
        /// </summary>
        public void Add(T item)
        {
            this.ResizeFor(_count + 1);

            _data[_count] = item;
            _count++;
        }

        /// <summary>
        /// Clears all data.
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }

        /// <summary>
        /// Returns true if the given item exists.
        /// </summary>
        public bool Contains(T item)
        {
            return this.IndexOf(item) >= 0;
        }

        /// <summary>
        /// Copies the data from this list to the given array.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for(var i = 0; i < _count; i++)
            {
                array[i + arrayIndex] = _data[i];
            }
        }

        /// <summary>
        /// Returns the number of elements in this list.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Returns the capacity of this list.
        /// </summary>
        public int Capacity
        {
            get { return (int)_data.Length; }
        }

        /// <summary>
        /// Returns true if this list is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurance of the given element.
        /// </summary>
        public bool Remove(T item)
        {
            for (var i = 0; i < _count; i++)
            {
                if (_data[i].Equals(item))
                {
                    this.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// An enumerator.
        /// </summary>
        private struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private List<T> _parent;
            private int _i;

            public Enumerator(List<T> parent)
            {
                _parent = parent;
                _i = -1;
            }

            /// <summary>
            /// Returns the current element.
            /// </summary>
            public T Current
            {
                get
                {
                    return _parent[_i];
                }
            }

            /// <summary>
            /// Returns the current element.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return _parent._data[_i]; }
            }

            /// <summary>
            /// Moves to the next element.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                _i++;
                return _i < _parent.Count;
            }

            /// <summary>
            /// Resets the list.
            /// </summary>
            public void Reset()
            {
                _i = -1;
            }

            /// <summary>
            /// Disposes this enumerator.
            /// </summary>
            public void Dispose()
            {
                _parent = null;
            }
        }

        #region Data management

        private int _block = 1024;

        /// <summary>
        /// Resizes the internal array for a future count.
        /// </summary>
        private void ResizeFor(int count)
        {
            var current = _data.Length;
            while (count > current)
            {
                current += _block;
            }
            if (current != _data.Length)
            { // resize if needed.
                _data.Resize(current);
            }
        }

        #endregion

        /// <summary>
        /// Disposes of all native resources associated with this list.
        /// </summary>
        public void Dispose()
        {
            _data.Dispose();
        }
    }
}