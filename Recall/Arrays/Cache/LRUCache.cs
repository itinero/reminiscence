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

using System.Collections.Generic;
using System.Linq;

namespace Recall.Arrays.Cache
{
    /// <summary>
    /// Generic LRU cache implementation.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class LRUCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Holds the cached data.
        /// </summary>
        private Dictionary<TKey, CacheEntry> _data;

        /// <summary>
        /// Holds the next id.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Holds the last id.
        /// </summary>
        private ulong _lastId;

        /// <summary>
        /// A delegate to use for when an item is pushed out of the cache.
        /// </summary>
        /// <param name="item"></param>
        public delegate void OnRemoveDelegate(TValue item);

        /// <summary>
        /// Called when an item is pushed out of the cache.
        /// </summary>
        public OnRemoveDelegate OnRemove;

        /// <summary>
        /// Initializes this cache.
        /// </summary>
        /// <param name="capacity"></param>
        public LRUCache(int capacity)
        {
            _id = ulong.MinValue;
            _lastId = _id;
            _data = new Dictionary<TKey, CacheEntry>();

            this.MaxCapacity = ((capacity / 100) * 10) + capacity + 1;
            this.MinCapacity = capacity;
        }

        /// <summary>
        /// Gets the maximum number of items to keep until the cache is full.
        /// </summary>
        public int MaxCapacity { get; private set; }

        /// <summary>
        /// Gets the number of items keep when cache overflows.
        /// </summary>
        public int MinCapacity { get; private set; }

        /// <summary>
        /// Adds a new value for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            CacheEntry entry = new CacheEntry
            {
                Id = _id,
                Value = value
            };
            lock (_data)
            {
                _id++;
                _data[key] = entry;
            }

            this.ResizeCache();
        }

        /// <summary>
        /// Returns the amount of entries in this cache.
        /// </summary>
        public int Count
        {
            get
            {
                return _data.Count;
            }
        }

        /// <summary>
        /// Returns the value for this given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            lock (_data)
            {
                CacheEntry entry;
                _id++;
                if (_data.TryGetValue(key, out entry))
                {
                    entry.Id = _id;
                    value = entry.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Returns the value for this given key but does not effect the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPeek(TKey key, out TValue value)
        {
            lock (_data)
            {
                CacheEntry entry;
                if (_data.TryGetValue(key, out entry))
                {
                    value = entry.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            lock (_data)
            {
                if (this.OnRemove != null)
                { // call the OnRemove delegate.
                    foreach (var entry in _data)
                    {
                        this.OnRemove(entry.Value.Value);
                    }
                }
                _data.Clear();
            }
            _id = ulong.MinValue;
            _lastId = _id;
        }

        /// <summary>
        /// Removes the value for the given key.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(TKey id)
        {
            lock (_data)
            {
                _data.Remove(id);
            }
        }

        /// <summary>
        /// Resizes the cache.
        /// </summary>
        private void ResizeCache()
        {
            lock (_data)
            {
                if (_data.Count > this.MaxCapacity)
                {
                    var n = this.MaxCapacity - this.MinCapacity + 1;
                    var pairEnumerator = _data.GetEnumerator();
                    var queue = new BinaryHeapULong<KeyValuePair<TKey, CacheEntry>>((uint)n + 1);
                    while (queue.Count < n &&
                        pairEnumerator.MoveNext())
                    {
                        var current = pairEnumerator.Current;
                        queue.Push(current, ulong.MaxValue - current.Value.Id);
                    }
                    ulong min = queue.PeekWeight();
                    while (pairEnumerator.MoveNext())
                    {
                        var current = pairEnumerator.Current;
                        if (min < ulong.MaxValue - current.Value.Id)
                        {
                            queue.Push(current, ulong.MaxValue - current.Value.Id);
                            queue.Pop();
                            min = queue.PeekWeight();
                        }
                    }
                    while (queue.Count > 0)
                    {
                        var toRemove = queue.Pop();
                        if (this.OnRemove != null)
                        { // call the OnRemove delegate.
                            this.OnRemove(toRemove.Value.Value);
                        }
                        _data.Remove(toRemove.Key);
                        // update the 'last_id'
                        _lastId++;
                    }
                }
            }
        }

        /// <summary>
        /// An entry in this cache.
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// The id of the object.
            /// </summary>
            public ulong Id { get; set; }

            /// <summary>
            /// The object being cached.
            /// </summary>
            public TValue Value { get; set; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _data.Select<KeyValuePair<TKey, CacheEntry>, KeyValuePair<TKey, TValue>>(
                (source) =>
                {
                    return new KeyValuePair<TKey, TValue>(source.Key, source.Value.Value);
                }).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.Select<KeyValuePair<TKey, CacheEntry>, KeyValuePair<TKey, TValue>>(
                (source) =>
                {
                    return new KeyValuePair<TKey, TValue>(source.Key, source.Value.Value);
                }).GetEnumerator();
        }
    }
}