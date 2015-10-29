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
using System.IO;

namespace Recall.IO.Accessors
{
    /// <summary>
    /// A memory mapped accessor that stores objects of a variable size in bytes.
    /// </summary>
    internal sealed class MemoryMappedAccessorVariable<T> : MappedAccessor<T>
    {
        /// <summary>
        /// Holds the read-from delegate.
        /// </summary>
        private MappedFile.ReadFromDelegate<T> _readFrom;

        /// <summary>
        /// Holds the write-to delegate.
        /// </summary>
        private MappedFile.WriteToDelegate<T> _writeTo;

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="stream"></param>
        /// <param name="readFrom"></param>
        /// <param name="writeTo"></param>
        internal MemoryMappedAccessorVariable(MappedFile file, Stream stream, 
            MappedFile.ReadFromDelegate<T> readFrom, MappedFile.WriteToDelegate<T> writeTo)
            : base(file, stream, -1)
        {
            _readFrom = readFrom;
            _writeTo = writeTo;
        }
        
        /// <summary>
        /// Reads the structure at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="structure"></param>
        public sealed override void Read(long position, out T structure)
        {
            structure = _readFrom.Invoke(_stream, position);
        }

        /// <summary>
        /// Reads a number of structures starting at the given position adding them to the array the the given offset.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public sealed override int ReadArray(long position, T[] array, int offset, int count)
        {
            throw new NotSupportedException("Reading arrays of variable sized-structures is not suppored in a memory-mapped accessor.");
        }

        /// <summary>
        /// Writes the structure at the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="structure"></param>
        /// <returns></returns>
        public sealed override long Write(long position, ref T structure)
        {
            return _writeTo.Invoke(_stream, position, structure);
        }

        /// <summary>
        /// Writes the structures from the array as the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public sealed override long WriteArray(long position, T[] array, int offset, int count)
        {
            throw new NotSupportedException("Writing arrays of variable sized-structures is not suppored in a memory-mapped accessor.");
        }
    }
}