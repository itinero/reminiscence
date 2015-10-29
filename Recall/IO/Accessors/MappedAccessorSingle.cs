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
    /// A memory mapped accessor that stores floats.
    /// </summary>
    internal sealed class MappedAccessorSingle : MappedAccessor<float>
    {
        /// <summary>
        /// Creates a new memory mapped acessor.
        /// </summary>
        /// <param name="file">The file this accessor is for.</param>
        /// <param name="stream">The stream used to read or write.</param>
        internal MappedAccessorSingle(MappedFile file, Stream stream)
            : base(file, stream, 4)
        {

        }

        /// <summary>
        /// Reads from the buffer at the given position.
        /// </summary>
        /// <param name="position">The position to read from.</param>
        /// <returns></returns>
        protected sealed override float ReadFrom(int position)
        {
            return BitConverter.ToSingle(_buffer, position);
        }

        /// <summary>
        /// Writes to the stream.
        /// </summary>
        /// <param name="structure"></param>
        protected sealed override long WriteTo(float structure)
        {
            _stream.Write(BitConverter.GetBytes(structure), 0, _elementSize);
            return _elementSize;
        }
    }
}