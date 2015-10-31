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

namespace Reminiscence.IO.Accessors
{
    /// <summary>
    /// A memory mapped accessor that stores uints.
    /// </summary>
    internal sealed class MappedAccessorInt32 : MappedAccessor<int>
    {
        private readonly byte[] _buffer;

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        internal MappedAccessorInt32(MemoryMap file, Stream stream)
            : base(file, stream, 4)
        {
            _buffer = new byte[_elementSize];
        }

        /// <summary>
        /// Reads appropriate amount of bytes from the stream at the given position and returns the structure.
        /// </summary>
        public override long ReadFrom(Stream stream, long position, ref int structure)
        {
            stream.Seek(position, SeekOrigin.Begin);
            if (stream.Read(_buffer, 0, _elementSize) != _elementSize)
            {
                structure = 0;
                return 0;
            }
            structure = BitConverter.ToInt32(_buffer, 0);
            return _elementSize;
        }

        /// <summary>
        /// Converts the structure to bytes and writes them to the stream.
        /// </summary>
        public override long WriteTo(Stream stream, long position, ref int structure)
        {
            stream.Write(BitConverter.GetBytes(structure), 0, _elementSize);
            return _elementSize;
        }
    }
}