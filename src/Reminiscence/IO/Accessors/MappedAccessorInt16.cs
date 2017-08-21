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
    /// A memory mapped accessor that stores shorts.
    /// </summary>
    public sealed class MappedAccessorInt16 : MappedAccessor<short>
    {
        private byte[] _buffer;

        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedAccessorInt16(MemoryMap file, Stream stream)
            : base(file, stream, 2)
        {
            _buffer = new byte[_elementSize];
        }

        /// <summary>
        /// Reads appropriate amount of bytes from the stream at the given position and returns the structure.
        /// </summary>
        public override long ReadFrom(Stream stream, long position, ref short structure)
        {
            if (stream.Position != position)
            {
                stream.Seek(position, SeekOrigin.Begin);
            }
            if (stream.Read(_buffer, 0, _elementSize) != _elementSize)
            {
                structure = 0;
                return 0;
            }
            structure = BitConverter.ToInt16(_buffer, 0);
            return _elementSize;
        }

        /// <summary>
        /// Reads elements starting at the given position.
        /// </summary>
        public override int ReadArray(long position, short[] array, int offset, int count)
        {
            if (_stream.Length <= position)
            { // cannot seek to this location, past the end of the stream.
                return -1;
            }

            // try and read everything.
            var elementsRead = System.Math.Min((int)((_stream.Length - position) / _elementSize), count);
            if (elementsRead > 0)
            { // ok, read.
                var bufferSize = array.Length * _elementSize;
                if (_buffer.Length < bufferSize)
                {
                    Array.Resize(ref _buffer, bufferSize);
                }
                if (_stream.Position != position)
                {
                    _stream.Seek(position, SeekOrigin.Begin);
                }
                _stream.Read(_buffer, 0, _buffer.Length);
                for (int i = 0; i < elementsRead; i++)
                {
                    array[i + offset] = BitConverter.ToInt16(_buffer, i * _elementSize);
                }
            }
            return elementsRead;
        }

        /// <summary>
        /// Converts the structure to bytes and writes them to the stream.
        /// </summary>
        public override long WriteTo(Stream stream, long position, ref short structure)
        {
            if (stream.Position != position)
            {
                stream.Seek(position, SeekOrigin.Begin);
            }
            stream.Write(BitConverter.GetBytes(structure), 0, _elementSize);
            return _elementSize;
        }

        /// <summary>
        /// Writes an array of elements at the given position.
        /// </summary>
        public override long WriteArray(long position, short[] array, int offset, int count)
        {
            long size = 0;
            _stream.Seek(position, SeekOrigin.Begin);
            using (var binaryWriter = new BinaryWriter(_stream))
            {
                for (int i = 0; i < count; i++)
                {
                    binaryWriter.Write(array[i]);
                }
            }
            return size;
        }
    }
}