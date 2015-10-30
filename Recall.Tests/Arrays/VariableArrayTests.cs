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
using Recall.Arrays;
using Recall.IO;
using System;
using System.IO;

namespace Recall.Tests.Arrays
{
    /// <summary>
    /// Contains tests for the variable array.
    /// </summary>
    [TestFixture]
    public class VariableArrayTests
    {
        /// <summary>
        /// Tests argument checks.
        /// </summary>
        [Test]
        public void ArgumentTest()
        {
            using (var map = new MappedStream())
            {
                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 10))
                {
                    Assert.AreEqual(10, array.Length);
                    Assert.Catch<ArgumentOutOfRangeException>(() =>
                    {
                        array[1001] = 10.ToString();
                    });
                    Assert.Catch<ArgumentOutOfRangeException>(() =>
                    {
                        array[-1] = 10.ToString();
                    });

                    string value;
                    Assert.Catch<ArgumentOutOfRangeException>(() =>
                    {
                        value = array[1001];
                    });
                    Assert.Catch<ArgumentOutOfRangeException>(() =>
                    {
                        value = array[-1];
                    });
                }
            }
        }

        /// <summary>
        /// Tests for the array when it has zero-size.
        /// </summary>
        [Test]
        public void ZeroSizeTest()
        {
            using (var map = new MappedStream())
            {
                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 0))
                {
                    Assert.AreEqual(0, array.Length);
                }
                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 100))
                {
                    array.Resize(0);
                    Assert.AreEqual(0, array.Length);
                }
            }
        }

        /// <summary>
        /// A test for the huge array.
        /// </summary>
        [Test]
        public void CompareToArrayTest()
        {
            var randomGenerator = new System.Random(66707770); // make this deterministic 

            using (var map = new MappedStream())
            {
                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 1000))
                {
                    var arrayExpected = new string[1000];

                    for (uint i = 0; i < 1000; i++)
                    {
                        if (randomGenerator.Next(4) >= 2)
                        { // add data.
                            arrayExpected[i] = i.ToString();
                            array[i] = i.ToString();
                        }
                        else
                        {
                            arrayExpected[i] = int.MaxValue.ToString();
                            array[i] = int.MaxValue.ToString();
                        }
                        Assert.AreEqual(arrayExpected[i], array[i]);
                    }

                    for (var i = 0; i < 1000; i++)
                    {
                        Assert.AreEqual(arrayExpected[i], array[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Tests resizing the array.
        /// </summary>
        [Test]
        public void ResizeTests()
        {
            var randomGenerator = new System.Random(66707770); // make this deterministic 

            using (var map = new MappedStream())
            {
                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 1000))
                {
                    var arrayExepected = new string[1000];

                    for (uint i = 0; i < 1000; i++)
                    {
                        if (randomGenerator.Next(4) >= 2)
                        { // add data.
                            arrayExepected[i] = i.ToString();
                            array[i] = i.ToString();
                        }
                        else
                        {
                            arrayExepected[i] = int.MaxValue.ToString();
                            array[i] = int.MaxValue.ToString();
                        }

                        Assert.AreEqual(arrayExepected[i], array[i]);
                    }

                    Array.Resize<string>(ref arrayExepected, 335);
                    array.Resize(335);

                    Assert.AreEqual(arrayExepected.Length, array.Length);
                    for (int i = 0; i < arrayExepected.Length; i++)
                    {
                        Assert.AreEqual(arrayExepected[i], array[i]);
                    }
                }

                using (var array = new VariableArray<string>(map.CreateInt64, map.CreateVariableString, 1024, 1000))
                {
                    var arrayExpected = new string[1000];

                    for (uint i = 0; i < 1000; i++)
                    {
                        if (randomGenerator.Next(4) >= 1)
                        { // add data.
                            arrayExpected[i] = i.ToString();
                            array[i] = i.ToString();
                        }
                        else
                        {
                            arrayExpected[i] = int.MaxValue.ToString();
                            array[i] = int.MaxValue.ToString();
                        }

                        Assert.AreEqual(arrayExpected[i], array[i]);
                    }

                    Array.Resize<string>(ref arrayExpected, 1235);
                    var oldSize = array.Length;
                    array.Resize(1235);

                    Assert.AreEqual(arrayExpected.Length, array.Length);
                    for (int i = 0; i < arrayExpected.Length; i++)
                    {
                        Assert.AreEqual(arrayExpected[i], array[i], 
                            string.Format("Array element not equal at index: {0}. Expected {1}, found {2}",
                                i, array[i], arrayExpected[i]));
                    }
                }
            }
        }

        ///// <summary>
        ///// Tests write to stream.
        ///// </summary>
        //[Test]
        //public void TestWriteToAndReadFrom()
        //{
        //    using (var map = new MappedStream())
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            using (var array = new Array<int>(map.CreateInt32, 4, 10))
        //            {
        //                for (var i = 0; i < array.Length; i++)
        //                {
        //                    array[i] = i + 100;
        //                }

        //                array.WriteTo(memoryStream);
        //                memoryStream.Seek(0, SeekOrigin.Begin);

        //                using (var array1 = Array<int>.ReadFrom(memoryStream, map.CreateInt32, 4))
        //                {
        //                    for (var i = 0; i < array.Length; i++)
        //                    {
        //                        Assert.AreEqual(array[i], array1[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    using (var map = new MappedStream())
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            using (var array = new Array<int>(map.CreateInt32, 4, 10000, 32, 32, 2))
        //            {
        //                for (var i = 0; i < array.Length; i++)
        //                {
        //                    array[i] = i + 100;
        //                }

        //                array.WriteTo(memoryStream);
        //                memoryStream.Seek(0, SeekOrigin.Begin);

        //                using (var array1 = Array<int>.ReadFrom(memoryStream, map.CreateInt32, 4))
        //                {
        //                    for (var i = 0; i < array.Length; i++)
        //                    {
        //                        Assert.AreEqual(array[i], array1[i]);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}