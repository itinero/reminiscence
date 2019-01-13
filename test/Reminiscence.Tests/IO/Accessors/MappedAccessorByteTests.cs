// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

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
using Reminiscence.IO;

namespace Reminiscence.Tests.IO.Accessors
{
    /// <summary>
    /// Contains tests for accessors for bytes.
    /// </summary>
    public class MappedAccessorByteTests
    {
        /// <summary>
        /// Tests the basics of what accessor is supposed to do.
        /// </summary>
        [Test]
        public void TestBasics()
        {
            using (var map = new MemoryMapStream())
            {
                var accessor = map.CreateByte(123456);

                Assert.AreEqual(true, accessor.CanWrite);
                Assert.AreEqual(123456, accessor.Capacity);
                Assert.AreEqual(123456, accessor.CapacityElements);
                Assert.AreEqual(1, accessor.ElementSize);
                Assert.AreEqual(true, accessor.ElementSizeFixed);

                var data = new byte[1024];
                for (ushort i = 0; i < 1024; i++)
                {
                    data[i] = (byte)(i ^ 6983);
                }

                accessor.WriteArray(15214, data, 0, 1024);

                var readData = new byte[1024];
                accessor.ReadArray(15214, readData, 0, 1024);
                for (ushort i = 0; i < 1024; i++)
                {
                    Assert.AreEqual(data[i], readData[i]);
                }
            }
        }
    }
}