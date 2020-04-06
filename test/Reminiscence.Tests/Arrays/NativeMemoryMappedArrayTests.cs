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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Reminiscence.Tests.Arrays
{
    /// <summary>
    /// Contains new memory mapped array tests.
    /// </summary>
    public class NativeMemoryMappedArrayTests : IDisposable
    {
        private readonly List<string> createdPaths = new List<string>();

        public void Dispose() => this.createdPaths.ForEach(File.Delete);

        /// <summary>
        /// A comparison test for the huge array to a regular array.
        /// </summary>
        [Test]
        public void CompareToArrayTest()
        {
            var intArrayRef = new int[1000];
            var byteLength = MemoryMarshal.AsBytes(intArrayRef.AsSpan()).Length;
            var intArray = new NativeMemoryMappedArray<int>(this.CreateFile(byteLength));

            var randomGenerator = new System.Random(8675309); // make this deterministic 
            for (var idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    intArrayRef[idx] = idx;
                    intArray[idx] = idx;
                }
                else
                {
                    intArrayRef[idx] = 0;
                    intArray[idx] = 0;
                }
            }

            for (var idx = 0; idx < 1000; idx++)
            {
                Assert.AreEqual(intArrayRef[idx], intArray[idx]);
            }

            intArray.Dispose();
        }

        /// <summary>
        /// A test resizing a huge array 
        /// </summary>
        [Test]
        public void ResizeTest()
        {
            var intArrayRef = new int[1000];
            var intArray = new NativeMemoryMappedArray<int>(this.CreateFile(1000 * sizeof(int)));

            var randomGenerator = new System.Random(8675309); // make this deterministic 
            for (int idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    intArrayRef[idx] = idx;
                    intArray[idx] = idx;
                }
                else
                {
                    intArrayRef[idx] = 0;
                    intArray[idx] = 0;
                }
            }

            Array.Resize(ref intArrayRef, 335);
            intArray.Resize(335);

            Assert.AreEqual(intArrayRef.Length, intArray.Length);
            for (int idx = 0; idx < intArrayRef.Length; idx++)
            {
                Assert.AreEqual(intArrayRef[idx], intArray[idx]);
            }

            intArrayRef = new int[1000];
            intArray.Dispose();
            intArray = new NativeMemoryMappedArray<int>(this.CreateFile(1000 * sizeof(int)));

            for (int idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    intArrayRef[idx] = idx;
                    intArray[idx] = idx;
                }
                else
                {
                    intArrayRef[idx] = 0;
                    intArray[idx] = 0;
                }
            }

            Array.Resize(ref intArrayRef, 1235);
            intArray.Resize(1235);

            Assert.AreEqual(intArrayRef.Length, intArray.Length);
            for (int idx = 0; idx < intArrayRef.Length; idx++)
            {
                Assert.AreEqual(intArrayRef[idx], intArray[idx]);
            }

            intArray.Dispose();
        }

        [Test]
        public void RoundTripTest()
        {
            var inputBytes = new byte[1024 * 1024];
            var inputBytesAsInt32 = MemoryMarshal.Cast<byte, int>(inputBytes);
            var randomGenerator = new System.Random(8675309); // make this deterministic
            for (int i = 0; i < inputBytesAsInt32.Length; i++)
            {
                inputBytesAsInt32[i] = randomGenerator.Next();
            }

            Span<byte> outputBytes;
            using (var arr1 = new NativeMemoryMappedArray<int>(this.CreateFile(inputBytes.Length)))
            using (var arr2 = new NativeMemoryMappedArray<int>(this.CreateFile(inputBytes.Length)))
            {
                // load the data in from a stream
                using (var ms = new MemoryStream(inputBytes, false))
                {
                    arr1.CopyFrom(ms);
                }

                // copy the data to another array
                arr2.CopyFrom(arr1);

                // save the data out to another stream
                using (var ms = new MemoryStream(inputBytes.Length))
                {
                    arr2.CopyTo(ms);
                    outputBytes = ms.ToArray();
                }
            }

            Assert.True(outputBytes.SequenceEqual(inputBytes));
        }

        [Test]
        public void CappedFileTest()
        {
            var randomGenerator = new System.Random(8675309); // make this deterministic

            var headValues = GenerateRandomValues(123);
            var middleValues = GenerateRandomValues(456);
            var tailValues = GenerateRandomValues(789);
            ReadOnlySpan<int> GenerateRandomValues(int length)
            {
                int[] values = new int[length];
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = randomGenerator.Next();
                }

                return values;
            }

            long headByteOffset, middleByteOffset, tailByteOffset;
            string path = Path.GetRandomFileName();
            using (var file = File.Create(path))
            {
                this.createdPaths.Add(path);

                // write variable-length garbage before and after each important section.
                file.Write(new byte[70000]);

                headByteOffset = file.Position;
                file.Write(MemoryMarshal.AsBytes(headValues));

                file.Write(new byte[29]);

                middleByteOffset = file.Position;
                file.Write(MemoryMarshal.AsBytes(middleValues));

                file.Write(new byte[365]);

                tailByteOffset = file.Position;
                file.Write(MemoryMarshal.AsBytes(tailValues));

                file.Write(new byte[99]);
            }

            // make sure to test that we can have multiple arrays alive at the same time that all
            // look at different non-overlapping sections of the same file
            using (var headArr = new NativeMemoryMappedArray<int>(path, headByteOffset, headValues.Length))
            using (var middleArr = new NativeMemoryMappedArray<int>(path, middleByteOffset, middleValues.Length))
            using (var tailArr = new NativeMemoryMappedArray<int>(path, tailByteOffset, tailValues.Length))
            {
                AssertEqual(headValues, headArr);
                AssertEqual(middleValues, middleArr);
                AssertEqual(tailValues, tailArr);
                void AssertEqual(ReadOnlySpan<int> expected, NativeMemoryMappedArray<int> actual)
                {
                    Assert.AreEqual(expected.Length, actual.Length);
                    for (int i = 0; i < expected.Length; i++)
                    {
                        Assert.AreEqual(expected[i], actual[i]);
                    }
                }
            }
        }

        private string CreateFile(int length)
        {
            string path = Path.GetRandomFileName();
            File.WriteAllBytes(path, new byte[length]);
            this.createdPaths.Add(path);
            return path;
        }
    }
}
