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

using Reminiscence.IO;
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
    }
}
