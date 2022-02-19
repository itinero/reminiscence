﻿// The MIT License (MIT)

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
using System.IO;

namespace Reminiscence.IO.Accessors
{
    /// <summary>
    /// A memory mapped accessor that stores objects of a variable size in bytes.
    /// </summary>
    public sealed class MappedAccessorVariable<T> : MappedAccessor<T>
    {
        private readonly MemoryMap.ReadFromDelegate<T> _readFrom; // Holds the read-from delegate.
        private readonly MemoryMap.WriteToDelegate<T> _writeTo; // Holds the write-to delegate.

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedAccessorVariable(MemoryMap file, byte[] data, 
            MemoryMap.ReadFromDelegate<T> readFrom, MemoryMap.WriteToDelegate<T> writeTo)
            : base(file, data, -1)
        {
            _readFrom = readFrom;
            _writeTo = writeTo;
        }
        
        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedAccessorVariable(MemoryMap file, Stream stream, 
            MemoryMap.ReadFromDelegate<T> readFrom, MemoryMap.WriteToDelegate<T> writeTo)
            : base(file, stream, -1)
        {
            _readFrom = readFrom;
            _writeTo = writeTo;
        }

        /// <summary>
        /// Reads appropriate amount of bytes from the stream at the given position and returns the structure.
        /// </summary>
        public override long ReadFrom(Stream stream, long position, ref T structure)
        {
            if (position < 0 || position >= stream.Length)
            {
                return -1;
            }
            return _readFrom.Invoke(stream, position, ref structure);
        }

        /// <summary>
        /// Converts the structure to bytes and writes them to the stream.
        /// </summary>
        public override long WriteTo(Stream stream, long position, ref T structure)
        {
            return _writeTo.Invoke(stream, position, ref structure);
        }
    }
}