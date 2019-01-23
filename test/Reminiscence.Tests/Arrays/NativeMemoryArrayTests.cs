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
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Reminiscence.Tests.Arrays
{
    /// <summary>
    /// Contains new memory array tests.
    /// </summary>
    public class NativeMemoryArrayTests
    {
        /// <summary>
        /// Tests the argument verifications on the constructors.
        /// </summary>
        [Test]
        public void ConstructorParameterExceptions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, -1);
            });
        }

        /// <summary>
        /// A comparison test for the huge array to a regular array.
        /// </summary>
        [Test]
        public void CompareToArrayTest()
        {
            var intArrayRef = new int[1000];
            var intArray = new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, 1000);

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
            var intArray = new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, 1000);

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
            intArray = new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, 1000);

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

            using (var arr1 = new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, inputBytesAsInt32.Length))
            using (var arr2 = new NativeMemoryArray<int>(DefaultUnmanagedMemoryAllocator.Instance, inputBytesAsInt32.Length))
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
        public void MemoryMappedFileTest()
        {
            var inputBytes = new byte[1024 * 1024];
            var inputBytesAsInt32 = MemoryMarshal.Cast<byte, int>(inputBytes);
            var randomGenerator = new System.Random(8675309); // make this deterministic
            for (int i = 0; i < inputBytesAsInt32.Length; i++)
            {
                inputBytesAsInt32[i] = randomGenerator.Next();
            }

            Span<byte> outputBytes;

            using (var inputFile = new FileStream(Path.GetRandomFileName(), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose))
            using (var outputFile = new FileStream(Path.GetRandomFileName(), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose))
            {
                inputFile.Write(inputBytes, 0, inputBytes.Length);
                outputFile.SetLength(inputBytes.Length);
                using (var inputMMF = MemoryMappedFile.CreateFromFile(inputFile, "TestMap1", 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
                using (var outputMMF = MemoryMappedFile.CreateFromFile(outputFile, "TestMap2", 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
                using (var inputAccessor = inputMMF.CreateViewAccessor())
                using (var outputAccessor = outputMMF.CreateViewAccessor())
                {
                    var inputAllocator = new SafeBufferMemoryAllocator(inputAccessor.SafeMemoryMappedViewHandle);
                    var outputAllocator = new SafeBufferMemoryAllocator(outputAccessor.SafeMemoryMappedViewHandle);

                    using (var arr1 = new NativeMemoryArray<int>(inputAllocator, inputBytesAsInt32.Length))
                    using (var arr2 = new NativeMemoryArray<int>(outputAllocator, inputBytesAsInt32.Length))
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
                }
            }

            Assert.True(outputBytes.SequenceEqual(inputBytes));
        }
    }
}
