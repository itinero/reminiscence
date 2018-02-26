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

using NUnit.Framework;
using Reminiscence.Arrays;
using Reminiscence.IO;

namespace Reminiscence.Tests.Arrays
{
    /// <summary>
    /// Contains tests for an array of shorts.
    /// </summary>
    public class ArrayInt16Tests
    {
        /// <summary>
        /// Tests the basics.
        /// </summary>
        [Test]
        public void TestBasics()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new Array<short>(map, 1024);
                for (ushort i = 0; i < 1024; i++)
                {
                    array[i] = (short)(i ^ 6983);
                }

                for (ushort i = 0; i < 1024; i++)
                {
                    Assert.AreEqual(array[i], (short)(i ^ 6983));
                }
            }
        }
    }
}