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

namespace Reminiscence.Arrays
{
    /// <summary>
    /// A profile with settings for a mapped array.
    /// </summary>
    public class ArrayProfile
    {
        /// <summary>
        /// The size of a single cached buffer.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// The number of buffers in cache.
        /// </summary>
        public int CacheSize { get; set; }

        /// <summary>
        /// An array profile that used no caching.
        /// </summary>
        public static ArrayProfile NoCache = new ArrayProfile()
        {
            BufferSize = 0,
            CacheSize = 0
        };

        /// <summary>
        /// An array profile that uses just one buffer, good for sequential access.
        /// </summary>
        public static ArrayProfile OneBuffer = new ArrayProfile()
        {
            BufferSize = 1024,
            CacheSize = 1
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 8Kb of cached data.
        /// </summary>
        public static ArrayProfile Aggressive8 = new ArrayProfile()
        {
            BufferSize = 1024,
            CacheSize = 8
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 32Kb of cached data.
        /// </summary>
        public static ArrayProfile Aggressive32 = new ArrayProfile()
        {
            BufferSize = 1024,
            CacheSize = 32
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 64Kb of cached data.
        /// </summary>
        public static ArrayProfile Aggressive64 = new ArrayProfile()
        {
            BufferSize = 1024,
            CacheSize = 64
        };

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Array Profile: B={0} C={1}",
                this.BufferSize, this.CacheSize);
        }
    }
}