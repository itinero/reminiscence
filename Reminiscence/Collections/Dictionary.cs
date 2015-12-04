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
using System.Linq;

namespace Reminiscence.Collections
{
    /// <summary>
    /// A dictionary of objects that can be accessed by key and uses a memory mapped data structures.
    /// </summary>
    public class Dictionary<TKey, TValue> : System.Collections.Generic.IDictionary<TKey, TValue>
    {
        private readonly Array<uint> _hashedPointers; // a list of points to keys per hash.
        private readonly List<uint> _keyValueList; // contains sections of powers of 2.
        private readonly Indexes.Index<TKey> _keys; // an index of all keys.
        private readonly Indexes.Index<TValue> _values; // an index of all values.
        private readonly Func<TKey, int> _keyGetHashCode; // a function to calculate hashcodes for keys.
        private const int ENTRY_SIZE = 3;
        private readonly Func<TKey, TKey, bool> _keysEqual; // a function to compare keys when hashcodes collide.
        private readonly bool _verifyUniqueKeys = true; // a flag to disable unique key checking if needed.
        private readonly int _minimumKeyCount = 4; // minimum amount of space allocated for key-value pairs.

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        public Dictionary(MemoryMap map)
            : this(map, 1024)
        {

        }

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        public Dictionary(MemoryMap map, int hashes)
            : this(map, hashes,
                (key) => key.GetHashCode(),
                (key1, key2) => key1.Equals(key2))
        {

        }

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        public Dictionary(MemoryMap map, IEqualityComparer<TKey> equalityComparer)
            : this(map, 1024,
                (key) => equalityComparer.GetHashCode(key),
                (key1, key2) => equalityComparer.Equals(key1, key2))
        {

        }

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        public Dictionary(MemoryMap map, int hashes, IEqualityComparer<TKey> equalityComparer)
            : this(map, hashes,
                (key) => equalityComparer.GetHashCode(key),
                (key1, key2) => equalityComparer.Equals(key1, key2))
        {

        }

        /// <summary>
        /// Creates a new dictionary.
        /// </summary>
        public Dictionary(MemoryMap map, int hashes, Func<TKey, int> keyGetHashCode, Func<TKey, TKey, bool> keyEquals)
        {
            _hashedPointers = new Array<uint>(map, hashes, ArrayProfile.NoCache);
            _keyValueList = new List<uint>(map, 4, ArrayProfile.NoCache);
            _keys = new Indexes.Index<TKey>(map);
            _values = new Indexes.Index<TValue>(map);

            _keyGetHashCode = keyGetHashCode;
            _keysEqual = keyEquals;

            _keyValueList.Add(0); // zero cannot be used.
        }

        private int _count = 0;

        /// <summary>
        /// Adds an element with the provided key and value 
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            var hash = CalculateHash(key, (int)_hashedPointers.Length);
            var pointer = _hashedPointers[hash];
            if (pointer == 0)
            { // ok, there is no data yet here.
                _hashedPointers[hash] = (uint)_keyValueList.Count;
                _keyValueList.Add(1);
                _keyValueList.Add((uint)_keys.Add(key));
                _keyValueList.Add(this.CalculateFullHash(key));
                _keyValueList.Add((uint)_values.Add(value));
                for (var i = 0; i < _minimumKeyCount - 1; i++)
                {
                    _keyValueList.Add(0);
                    _keyValueList.Add(0);
                    _keyValueList.Add(0);
                }
            }
            else
            { // ok, add at the end of the linked-list.
                var keyHash = this.CalculateFullHash(key);
                var count = _keyValueList[(int)pointer];
                if (count > (_minimumKeyCount / 2) &&
                  (count & (count - 1)) == 0)
                { // a power of two, copy to the end and check duplicate keys.
                    _hashedPointers[hash] = (uint)_keyValueList.Count;
                    _keyValueList.Add(count + 1);
                    for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                    {
                        if (_verifyUniqueKeys)
                        {
                            if (_keyValueList[p + 1] == keyHash)
                            { // only check if the key full hashes match.
                                var existingKey = _keys.Get(_keyValueList[p + 0]);
                                if (_keysEqual(existingKey, key))
                                {
                                    throw new ArgumentException("Key already exists.");
                                }
                            }
                        }
                        _keyValueList.Add(_keyValueList[p]);
                        _keyValueList.Add(_keyValueList[p + 1]);
                        _keyValueList.Add(_keyValueList[p + 2]);
                    }
                    _keyValueList.Add((uint)_keys.Add(key));
                    _keyValueList.Add(this.CalculateFullHash(key));
                    _keyValueList.Add((uint)_values.Add(value));
                    for (var p = 0; p < count - 1; p++)
                    {
                        _keyValueList.Add(0);
                        _keyValueList.Add(0);
                        _keyValueList.Add(0);
                    }
                }
                else
                {
                    if (_verifyUniqueKeys)
                    {
                        for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                        {
                            if (_keyValueList[p + 1] == keyHash)
                            { // only check if the key full hashes match.
                                var existingKey = _keys.Get(_keyValueList[p + 0]);
                                if (_keysEqual(existingKey, key))
                                {
                                    throw new ArgumentException("Key already exists.");
                                }
                            }
                        }
                    }
                    _keyValueList[pointer] = _keyValueList[pointer] + 1;
                    _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 0] = (uint)_keys.Add(key);
                    _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 1] = this.CalculateFullHash(key);
                    _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 2] = (uint)_values.Add(value);
                }
            }

            _count++;
        }

        /// <summary>
        /// Determines whether the specified key exists.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            var hash = CalculateHash(key, (int)_hashedPointers.Length);
            var pointer = _hashedPointers[hash];
            if (pointer == 0)
            { // ok, there is no data.
                return false;
            }
            else
            { // ok, search the keys.
                var keyHash = this.CalculateFullHash(key);
                var count = _keyValueList[pointer];
                for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                {
                    if (_keyValueList[p + 1] == keyHash)
                    { // only check keys if also their full hashes match.
                        var existingKey = _keys.Get(_keyValueList[p + 0]);
                        if (_keysEqual(existingKey, key))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Returns all the keys.
        /// </summary>
        public System.Collections.Generic.ICollection<TKey> Keys
        {
            get
            {
                return new System.Collections.Generic.List<TKey>(
                    this.Select(x => x.Key));
            }
        }

        /// <summary>
        /// Removes the element with the given key.
        /// </summary>
        public bool Remove(TKey key)
        {
            var hash = CalculateHash(key, (int)_hashedPointers.Length);
            var pointer = _hashedPointers[hash];
            if (pointer == 0)
            { // ok, there is no data yet here.
                return false;
            }
            else
            { // ok, search for the given key.
                var count = _keyValueList[pointer];
                var keyHash = this.CalculateFullHash(key);
                for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                {
                    if (_keyValueList[p + 1] == keyHash)
                    { // only check keys if also their full hashes match.
                        var existingKey = _keys.Get(_keyValueList[p + 0]);
                        if (_keysEqual(existingKey, key))
                        { // copy down all others.
                            for (; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                            {
                                _keyValueList[p + 0] = _keyValueList[p + 3];
                                _keyValueList[p + 1] = _keyValueList[p + 4];
                                _keyValueList[p + 2] = _keyValueList[p + 5];
                            }
                            _keyValueList[pointer] = _keyValueList[pointer] - 1;
                            _count--;
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Tries to get the element with the given key.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var hash = CalculateHash(key, (int)_hashedPointers.Length);
            var pointer = _hashedPointers[hash];
            if (pointer == 0)
            { // ok, there is no data here.
                value = default(TValue);
                return false;
            }
            else
            { // ok, search for the key.
                var keyHash = this.CalculateFullHash(key);
                var count = _keyValueList[pointer];
                for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                {
                    if (_keyValueList[p + 1] == keyHash)
                    { // only check keys if also their full hashes match.
                        var existingKey = _keys.Get(_keyValueList[p + 0]);
                        if (_keysEqual(existingKey, key))
                        { // key found!
                            value = _values.Get(_keyValueList[p + 2]);
                            return true;
                        }
                    }
                }
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Returns all the values.
        /// </summary>
        public System.Collections.Generic.ICollection<TValue> Values
        {
            get
            {
                return new System.Collections.Generic.List<TValue>(
                    this.Select(x => x.Value));
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the given key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (!this.TryGetValue(key, out result))
                {
                    throw new System.Collections.Generic.KeyNotFoundException();
                }
                return result;
            }
            set
            {
                var hash = CalculateHash(key, (int)_hashedPointers.Length);
                var pointer = _hashedPointers[hash];
                if (pointer == 0)
                { // ok, there is no data yet here.
                    _hashedPointers[hash] = (uint)_keyValueList.Count;
                    _keyValueList.Add(1);
                    _keyValueList.Add((uint)_keys.Add(key));
                    _keyValueList.Add(this.CalculateFullHash(key));
                    _keyValueList.Add((uint)_values.Add(value));
                    for (var i = 0; i < _minimumKeyCount - 1; i++)
                    {
                        _keyValueList.Add(0);
                        _keyValueList.Add(0);
                        _keyValueList.Add(0);
                    }
                }
                else
                { // ok, add at the end of the linked-list.
                    var keyHash = this.CalculateFullHash(key);
                    var count = _keyValueList[(int)pointer];
                    if (count > (_minimumKeyCount / 2) &&
                      (count & (count - 1)) == 0)
                    { // a power of two, copy to the end and check duplicate keys.
                        _hashedPointers[hash] = (uint)_keyValueList.Count;
                        _keyValueList.Add(count + 1);
                        for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                        {
                            if (_verifyUniqueKeys)
                            {
                                if (_keyValueList[p + 1] == keyHash)
                                { // only check if the key full hashes match.
                                    var existingKey = _keys.Get(_keyValueList[p + 0]);
                                    if (_keysEqual(existingKey, key))
                                    {
                                        _keyValueList[p + 2] = (uint)_values.Add(value);
                                        return;
                                    }
                                }
                            }
                            _keyValueList.Add(_keyValueList[p]);
                            _keyValueList.Add(_keyValueList[p + 1]);
                            _keyValueList.Add(_keyValueList[p + 2]);
                        }
                        _keyValueList.Add((uint)_keys.Add(key));
                        _keyValueList.Add(this.CalculateFullHash(key));
                        _keyValueList.Add((uint)_values.Add(value));
                        for (var p = 0; p < count - 1; p++)
                        {
                            _keyValueList.Add(0);
                            _keyValueList.Add(0);
                            _keyValueList.Add(0);
                        }
                    }
                    else
                    {
                        if (_verifyUniqueKeys)
                        {
                            for (var p = pointer + 1; p < pointer + (count * ENTRY_SIZE) + 1; p += ENTRY_SIZE)
                            {
                                if (_keyValueList[p + 1] == keyHash)
                                { // only check if the key full hashes match.
                                    var existingKey = _keys.Get(_keyValueList[p + 0]);
                                    if (_keysEqual(existingKey, key))
                                    {
                                        _keyValueList[p + 2] = (uint)_values.Add(value);
                                        return;
                                    }
                                }
                            }
                        }
                        _keyValueList[pointer] = _keyValueList[pointer] + 1;
                        _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 0] = (uint)_keys.Add(key);
                        _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 1] = this.CalculateFullHash(key);
                        _keyValueList[pointer + 1 + (count * ENTRY_SIZE) + 2] = (uint)_values.Add(value);
                    }
                }

                _count++;
            }
        }

        /// <summary>
        /// Adds a new element.
        /// </summary>
        public void Add(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all elements.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < _hashedPointers.Length; i++)
            {
                _hashedPointers[i] = 0;
            }
            _count = 0;
        }

        /// <summary>
        /// Returns true if the given element exists.
        /// </summary>
        public bool Contains(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (!this.TryGetValue(item.Key, out value))
            {
                return false;
            }
            return value.Equals(item.Value);
        }

        /// <summary>
        /// Copies this entire dictionary to the given array starting a the given index.
        /// </summary>
        public void CopyTo(System.Collections.Generic.KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int i = 0;
            foreach (var element in this)
            {
                array[arrayIndex + i] = element;
                i++;
            }
        }

        /// <summary>
        /// Returns the number of elements.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Returns true if readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        public bool Remove(System.Collections.Generic.KeyValuePair<TKey, TValue> item)
        {
            if (this.Contains(item))
            {
                return this.Remove(item.Key);
            }
            return false;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class Enumerator : System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.IEnumerator
        {
            private Dictionary<TKey, TValue> _dic;
            private int _hash = -1;
            private uint _pointer = 0;
            private int _count = -1;
            private uint _i = 0;

            /// <summary>
            /// Creates a new enumerator.
            /// </summary>
            public Enumerator(Dictionary<TKey, TValue> dic)
            {
                _dic = dic;
                this.Reset();
            }

            /// <summary>
            /// Returns the current item.
            /// </summary>
            public System.Collections.Generic.KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_hash == 0)
                    {
                        throw new InvalidOperationException("Enumerator not initialized.");
                    }
                    return new System.Collections.Generic.KeyValuePair<TKey, TValue>(
                        _dic._keys.Get(_dic._keyValueList[_pointer + 1 + (_i * ENTRY_SIZE) + 0]),
                        _dic._values.Get(_dic._keyValueList[_pointer + 1 + (_i * ENTRY_SIZE) + 2]));
                }
            }

            /// <summary>
            /// Returns the current item.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (_hash < 0)
                {
                    _hash = -1;
                    return this.MoveNextHash();
                }
                else
                { // move to the next linked-list entry.
                    _i++;
                    if (_i == _count)
                    { // move to the next hash.
                        return this.MoveNextHash();
                    }
                    return true;
                }
            }

            /// <summary>
            /// Moves to the next hash that has data.
            /// </summary>
            private bool MoveNextHash()
            {
                _count = 0;
                while (_count == 0)
                {
                    _hash++;
                    while (_hash < _dic._hashedPointers.Length &&
                        _dic._hashedPointers[_hash] == 0)
                    {
                        _hash++;
                    }
                    if (_hash == _dic._hashedPointers.Length)
                    { // empty dictionary or the end was reached.
                        return false;
                    }
                    // a pointer should have been found.
                    _pointer = _dic._hashedPointers[_hash];
                    _count = (int)_dic._keyValueList[_pointer];
                }
                _i = 0;
                return true;
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _hash = -1;
                _i = 0;
                _pointer = 0;
                _count = -1;
            }

            /// <summary>
            /// Disposes this enumerator.
            /// </summary>
            public void Dispose()
            {

            }
        }

        /// <summary>
        /// Calculates a hashcode with a fixed size.
        /// </summary>
        private int CalculateHash(TKey obj, int size)
        {
            var hash = (_keyGetHashCode(obj) % size);
            if (hash > 0)
            {
                return hash;
            }
            return -hash;
        }

        /// <summary>
        /// Calculates a complete hashcode and converts this to a Uint32.
        /// </summary>
        public uint CalculateFullHash(TKey obj)
        {
            return BitConverter.ToUInt32(
                BitConverter.GetBytes(_keyGetHashCode(obj)), 0);
        }
    }
}