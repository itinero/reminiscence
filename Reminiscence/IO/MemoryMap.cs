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

namespace Reminiscence.IO
{
    /// <summary>
    /// Represents a memory mapped file.
    /// </summary>
    public abstract class MemoryMap : IDisposable
    {
        private readonly List<IDisposable> _accessors; // Holds all acessors generated for this file.

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MemoryMap()
        {
            _accessors = new List<IDisposable>();
            _nextPosition = 0;
        }

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        public MappedAccessor<uint> CreateUInt32(long position, long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 4;
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
        public MappedAccessor<uint> CreateUInt32(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 4;
            var accessor = this.DoCreateNewUInt32(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        private long _nextPosition; // Holds the next position of a new empty accessor.

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<int> DoCreateNewInt32(long position, long sizeInByte);

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        public MappedAccessor<int> CreateInt32(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 4;
            var accessor = this.DoCreateNewInt32(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<uint> DoCreateNewUInt32(long position, long sizeInByte);

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        public MappedAccessor<float> CreateSingle(long position, long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 4;
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
        public MappedAccessor<float> CreateSingle(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 4;
            var accessor = this.DoCreateNewSingle(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<float> DoCreateNewSingle(long position, long sizeInByte);

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        public MappedAccessor<double> CreateDouble(long position, long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 8;
            var accessor = this.DoCreateNewDouble(position, sizeInBytes);
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
        public MappedAccessor<double> CreateDouble(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 8;
            var accessor = this.DoCreateNewDouble(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<double> DoCreateNewDouble(long position, long sizeInByte);

        /// <summary>
        /// Creates a new memory mapped accessor for a given part of this file with given size in bytes and the start position.
        /// </summary>
        public MappedAccessor<ulong> CreateUInt64(long position, long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 8;
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
        public MappedAccessor<ulong> CreateUInt64(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 8;
            var accessor = this.DoCreateNewUInt64(_nextPosition, sizeInBytes);
            _accessors.Add(accessor);

            _nextPosition = _nextPosition + sizeInBytes;

            return accessor;
        }

        /// <summary>
        /// Creates a new memory mapped file based on the given stream and the given size in bytes.
        /// </summary>
        protected abstract MappedAccessor<ulong> DoCreateNewUInt64(long position, long sizeInByte);

        /// <summary>
        /// Creates a new empty memory mapped accessor with given size in bytes.
        /// </summary>
        public MappedAccessor<long> CreateInt64(long sizeInElements)
        {
            var sizeInBytes = sizeInElements * 8;
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
        protected abstract MappedAccessor<T> DoCreateVariable<T>(long nextPosition, long sizeInBytes, 
            ReadFromDelegate<T> readFrom, WriteToDelegate<T> writeTo);

        /// <summary>
        /// Creates an accessor for strings.
        /// </summary>
        public MappedAccessor<string> CreateVariableString(long sizeInBytes)
        {
            return this.CreateVariable(sizeInBytes, IO.MemoryMapDelegates.ReadFromString,
                IO.MemoryMapDelegates.WriteToString);
        }

        /// <summary>
        /// Creates an accessor for and array of uint's.
        /// </summary>
        public MappedAccessor<uint[]> CreateVariableUInt32Array(long sizeInBytes)
        {
            return this.CreateVariable(sizeInBytes, IO.MemoryMapDelegates.ReadFromUIntArray, 
                IO.MemoryMapDelegates.WriteToUIntArray);
        }

        /// <summary>
        /// Creates an accessor for and array of int's.
        /// </summary>
        public MappedAccessor<int[]> CreateVariableInt32Array(long sizeInBytes)
        {
            return this.CreateVariable(sizeInBytes, IO.MemoryMapDelegates.ReadFromIntArray,
                IO.MemoryMapDelegates.WriteToIntArray);
        }

        /// <summary>
        /// Creates an accessor for strings.
        /// </summary>
        public MappedAccessor<int[]> CreateInt32Array(long sizeInBytes)
        {
            return this.CreateVariable(sizeInBytes, IO.MemoryMapDelegates.ReadFromIntArray,
                IO.MemoryMapDelegates.WriteToIntArray);
        }

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

        /// <summary>
        /// Holds accessor creating functions.
        /// </summary>
        private static System.Collections.Generic.Dictionary<Type, object> _accessorDelegates;

        /// <summary>
        /// Registers default accessors.
        /// </summary>
        public static void RegisterCreateAccessorFuncs()
        {
            _accessorDelegates = new Dictionary<Type, object>();
            _accessorDelegates.Add(typeof(int), new CreateAccessorFunc<int>(
                (map, size) => map.CreateInt32(size)));
            _accessorDelegates.Add(typeof(uint), new CreateAccessorFunc<uint>(
                (map, size) => map.CreateUInt32(size)));
            _accessorDelegates.Add(typeof(long), new CreateAccessorFunc<long>(
                (map, size) => map.CreateInt64(size)));
            _accessorDelegates.Add(typeof(ulong), new CreateAccessorFunc<ulong>(
                (map, size) => map.CreateUInt64(size)));
            _accessorDelegates.Add(typeof(float), new CreateAccessorFunc<float>(
                (map, size) => map.CreateSingle(size)));
            _accessorDelegates.Add(typeof(double), new CreateAccessorFunc<double>(
                (map, size) => map.CreateDouble(size)));
            _accessorDelegates.Add(typeof(string), new CreateAccessorFunc<string>(
                (map, size) => map.CreateVariableString(size)));
            _accessorDelegates.Add(typeof(uint[]), new CreateAccessorFunc<uint[]>(
                (map, size) => map.CreateVariableUInt32Array(size)));
            _accessorDelegates.Add(typeof(int[]), new CreateAccessorFunc<int[]>(
                (map, size) => map.CreateVariableInt32Array(size)));
        }

        /// <summary>
        /// Registers a new create accessor function.
        /// </summary>
        public static void RegisterCreateAccessorFunc<T>(CreateAccessorFunc<T> func)
        {
            _accessorDelegates[typeof(T)] = func;
        }

        /// <summary>
        /// A delegate to create an accessor.
        /// </summary>
        /// <param name="map">The memory map.</param>
        /// <param name="sizeInBytesOrElements">The size in elements for fixed-size accessors and in bytes for variable-sized accessors.</param>
        public delegate MappedAccessor<T> CreateAccessorFunc<T>(MemoryMap map, long sizeInBytesOrElements);

        /// <summary>
        /// Gets the create accessor function for the given type.
        /// </summary>
        public static CreateAccessorFunc<T> GetCreateAccessorFuncFor<T>()
        {
            var type = typeof(T);
            if (_accessorDelegates == null ||
               _accessorDelegates.Count == 0)
            { // register accessors.
                MemoryMap.RegisterCreateAccessorFuncs();
            }
            object value;
            if(!_accessorDelegates.TryGetValue(type, out value))
            {
                throw new NotSupportedException(string.Format("Type {0} not supported, try explicity registering an accessor creating function.",
                    type));
            }
            return (CreateAccessorFunc<T>)value;
        }
    }
}