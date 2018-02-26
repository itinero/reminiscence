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
using System.IO;

namespace Reminiscence
{
    /// <summary>
    /// Contains extensions methods for native .NET objects.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Copies a sorted list of elements to a stream.
        /// </summary>
        public static long CopyTo<T>(this IList<T> list, Stream stream)
        {
            var position = stream.Position;
            var i = 0;
            using (var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(new MemoryMapStream(), 0))
            {
                var element = default(T);
                while (i < list.Count)
                {
                    element = list[i];
                    accessor.WriteTo(stream, stream.Position, ref element);
                    i++;
                }
            }
            return stream.Position - position;
        }

        /// <summary>
        /// Copies all data to the given stream starting at it's current position prefixed with 8 bytes containing the size.
        /// </summary>
        public static long CopyToWithSize(this ISerializableToStream serializable, Stream stream)
        {
            var position = stream.Position;
            stream.Seek(position + 8, SeekOrigin.Begin);

            // copy the actual data.
            var size = serializable.CopyTo(stream);

            // write the size.
            stream.Seek(position, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(size), 0, 8);

            // reposition stream.
            stream.Seek(position + 8 + size, SeekOrigin.Begin);
            return size + 8;
        }

        /// <summary>
        /// Ensures that this <see cref="ArrayBase{T}"/> has room for at least
        /// the given number of elements, resizing if not.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element stored in this array.
        /// </typeparam>
        /// <param name="array">
        /// This array.
        /// </param>
        /// <param name="minimumSize">
        /// The minimum number of elements that this array must fit.
        /// </param>
        public static void EnsureMinimumSize<T>(this ArrayBase<T> array, long minimumSize)
        {
            if (array.Length < minimumSize)
            {
                IncreaseMinimumSize(array, minimumSize, fillEnd: false, fillValueIfNeeded: default(T));
            }
        }

        /// <summary>
        /// Ensures that this <see cref="ArrayBase{T}"/> has room for at least
        /// the given number of elements, resizing and filling the empty space
        /// with the given value if not.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element stored in this array.
        /// </typeparam>
        /// <param name="array">
        /// This array.
        /// </param>
        /// <param name="minimumSize">
        /// The minimum number of elements that this array must fit.
        /// </param>
        /// <param name="fillValue">
        /// The value to use to fill in the empty spaces if we have to resize.
        /// </param>
        public static void EnsureMinimumSize<T>(this ArrayBase<T> array, long minimumSize, T fillValue)
        {
            if (array.Length < minimumSize)
            {
                IncreaseMinimumSize(array, minimumSize, fillEnd: true, fillValueIfNeeded: fillValue);
            }
        }

        private static void IncreaseMinimumSize<T>(ArrayBase<T> array, long minimumSize, bool fillEnd, T fillValueIfNeeded)
        {
            long oldSize = array.Length;

            // fast-forward, perhaps, through the first several resizes.
            // Math.Max also ensures that we can resize from 0.
            long size = Math.Max(1024, oldSize * 2);
            while (size < minimumSize)
            {
                size *= 2;
            }

            array.Resize(size);
            if (!fillEnd)
            {
                return;
            }

            for (long i = oldSize; i < size; i++)
            {
                array[i] = fillValueIfNeeded;
            }
        }
    }
}
