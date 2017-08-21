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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reminiscence.Arrays;
using System;

namespace Reminiscence.Tests.Arrays
{
    /// <summary>
    /// Contains new memory array tests.
    /// </summary>
    [TestClass]
    public class MemoryArrayTests
    {
        /// <summary>
        /// Tests the argument verifications on the constructors.
        /// </summary>
        [TestMethod]
        public void ConstructorParameterExceptions()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                new MemoryArray<int>(-1);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                new MemoryArray<int>(-1, 2);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                new MemoryArray<int>(10, -1);
            });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                new MemoryArray<int>(10, 3);
            });
        }

        /// <summary>
        /// A comparison test for the huge array to a regular array.
        /// </summary>
        [TestMethod]
        public void CompareToArrayTest()
        {
            var stringArrayRef = new string[1000];
            var stringArray = new MemoryArray<string>(1000);

            var randomGenerator = new System.Random(66707770); // make this deterministic 
            for (var idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    stringArrayRef[idx] = idx.ToString();
                    stringArray[idx] = idx.ToString();
                }
                else
                {
                    stringArrayRef[idx] = null;
                    stringArray[idx] = null;
                }
            }

            for (var idx = 0; idx < 1000; idx++)
            {
                Assert.AreEqual(stringArrayRef[idx], stringArray[idx]);
            }
        }

        /// <summary>
        /// A test resizing a huge array 
        /// </summary>
        [TestMethod]
        public void ResizeTest()
        {
            var stringArrayRef = new string[1000];
            var stringArray = new MemoryArray<string>(1000);

            var randomGenerator = new System.Random(66707770); // make this deterministic 
            for (int idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    stringArrayRef[idx] = idx.ToString();
                    stringArray[idx] = idx.ToString();
                }
                else
                {
                    stringArrayRef[idx] = null;
                    stringArray[idx] = null;
                }
            }

            Array.Resize<string>(ref stringArrayRef, 335);
            stringArray.Resize(335);

            Assert.AreEqual(stringArrayRef.Length, stringArray.Length);
            for (int idx = 0; idx < stringArrayRef.Length; idx++)
            {
                Assert.AreEqual(stringArrayRef[idx], stringArray[idx]);
            }

            stringArrayRef = new string[1000];
            stringArray = new MemoryArray<string>(1000);

            for (int idx = 0; idx < 1000; idx++)
            {
                if (randomGenerator.Next(4) >= 2)
                { // add data.
                    stringArrayRef[idx] = idx.ToString();
                    stringArray[idx] = idx.ToString();
                }
                else
                {
                    stringArrayRef[idx] = null;
                    stringArray[idx] = null;
                }
            }

            Array.Resize<string>(ref stringArrayRef, 1235);
            stringArray.Resize(1235);

            Assert.AreEqual(stringArrayRef.Length, stringArray.Length);
            for (int idx = 0; idx < stringArrayRef.Length; idx++)
            {
                Assert.AreEqual(stringArrayRef[idx], stringArray[idx]);
            }
        }
    }
}