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

namespace Recall.Arrays
{
    /// <summary>
    /// A mapped huge array wrapping another array.
    /// </summary>
    /// <typeparam name="TMapped">The more 'advanced' stucture.</typeparam>
    /// <typeparam name="T">The 'simple' structure.</typeparam>
    /// <remarks>Used to map a more generic class or type to a more simple type like ints or floats.</remarks>
    public class MappedArray<TMapped, T> : ArrayBase<TMapped>
        where TMapped : struct
        where T : struct
    {
        private readonly ArrayBase<T> _baseArray;
        private readonly int _elementSize;
        private readonly MapTo _mapTo;
        private readonly MapFrom _mapFrom;

        /// <summary>
        /// Creates a new mapped huge array.
        /// </summary>
        /// <param name="baseArray">The base array.</param>
        /// <param name="elementSize">The size of one mapped structure when translate to the simpler structure.</param>
        /// <param name="mapTo">The map to implementation.</param>
        /// <param name="mapFrom">The map from implementation.</param>
        public MappedArray(ArrayBase<T> baseArray, int elementSize, MapTo mapTo, MapFrom mapFrom)
        {
            _baseArray = baseArray;
            _elementSize = elementSize;
            _mapTo = mapTo;
            _mapFrom = mapFrom;
        }

        /// <summary>
        /// Delegate to abstract mapping implementation for structure mapping.
        /// </summary>
        public delegate void MapTo(ArrayBase<T> array, long idx, TMapped toMap);

        /// <summary>
        /// Delegate to abstract mapping implementation for structure mapping.
        /// </summary>
        public delegate TMapped MapFrom(ArrayBase<T> array, long idx);

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public override long Length
        {
            get { return _baseArray.Length / _elementSize; }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        public override void Resize(long size)
        {
            _baseArray.Resize(size * _elementSize);
        }

        /// <summary>
        /// Returns the element at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public override TMapped this[long idx]
        {
            get
            {
                return _mapFrom.Invoke(_baseArray, idx * _elementSize);
            }
            set
            {
                _mapTo.Invoke(_baseArray, idx * _elementSize, value);
            }
        }

        /// <summary>
        /// Disposes of all native resources associated with this object.
        /// </summary>
        public override void Dispose()
        {
            _baseArray.Dispose();
        }
    }
}