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
using System.IO;
using Xunit;

namespace Reminiscense.Stresstests.Arrays
{
    /// <summary>
    /// Contains tests for arrays.
    /// </summary>
    public static class ArrayTests
    {
        /// <summary>
        /// Tests read/writing to an array.
        /// </summary>
        public static void Test()
        {
            ArrayTests.TestWrite(ArrayProfile.NoCache);
            ArrayTests.TestRead(ArrayProfile.NoCache);
            ArrayTests.TestReadRandom(ArrayProfile.NoCache);
            ArrayTests.TestParallelAccess();
            ArrayTests.TestHugeArray();
            ArrayTests.TestEnumerableArray();
        }

        /// <summary>
        /// Tests writing to an array.
        /// </summary>
        public static void TestWrite(ArrayProfile profile)
        {
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Create, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var array = new Array<int>(map, Global.ArrayTestLength,
                        profile);

                    var perf = new PerformanceInfoConsumer(
                        string.Format("Write Array: {0}", profile.ToString()), 
                            1000);
                    perf.Start();
                    for (var i = 0; i < array.Length; i++)
                    {
                        array[i] = i * 2;

                        if (Global.Verbose && i % (array.Length / 100) == 0)
                        {
                            perf.Report("Writing... {0}%", i, array.Length - 1);
                        }
                    }
                    perf.Stop();
                }
            }
        }

        /// <summary>
        /// Tests read from an array.
        /// </summary>
        public static void TestRead(ArrayProfile profile)
        {
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Open, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var array = new Array<int>(map.CreateInt32(mapStream.Length / 4), profile);

                    var perf = new PerformanceInfoConsumer(
                        string.Format("Read Array: {0}", profile.ToString()),
                            1000);
                    perf.Start();
                    for (var i = 0; i < array.Length; i++)
                    {
                        var val = array[i];
                        if (val != i * 2)
                        { // oeps, something went wrong here!
                            throw new System.Exception();
                        }

                        if (Global.Verbose && i % (array.Length / 100) == 0)
                        {
                            perf.Report("Reading... {0}%", i, array.Length - 1);
                        }
                    }
                    perf.Stop();

                    //var size = 1000000;
                    //perf = new PerformanceInfoConsumer(
                    //    string.Format("Read Random Array: {0} {1}", size, profile.ToString()),
                    //        1000);
                    //perf.Start();
                    //var rand = new Random();
                    //for (var i = 0; i < size; i++)
                    //{
                    //    var ran = (long)rand.Next((int)array.Length);
                    //    var val = array[ran];
                    //    if (val != ran * 2)
                    //    { // oeps, something went wrong here!
                    //        throw new System.Exception();
                    //    }

                    //    if (Global.Verbose && i % (size / 100) == 0)
                    //    {
                    //        perf.Report("Reading... {0}%", i, size);
                    //    }
                    //}
                    //perf.Stop();
                }
            }
        }

        /// <summary>
        /// Tests read from an array.
        /// </summary>
        public static void TestReadRandom(ArrayProfile profile)
        {
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Open, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var array = new Array<int>(map.CreateInt32(mapStream.Length / 4), profile);

                    var size = 1000000;
                    var perf = new PerformanceInfoConsumer(
                        string.Format("Read Random Array: {0} {1}", size, profile.ToString()),
                            1000);
                    perf.Start();
                    var rand = new Random();
                    for (var i = 0; i < size; i++)
                    {
                        var ran = (long)rand.Next((int)array.Length);
                        var val = array[ran];
                        if (val != ran * 2)
                        { // oeps, something went wrong here!
                            throw new System.Exception();
                        }

                        if (Global.Verbose && i % (size / 100) == 0)
                        {
                            perf.Report("Reading... {0}%", i, size);
                        }
                    }
                    perf.Stop();
                }
            }
        }

        /// <summary>
        /// Tests a huge array, reading and writing elements.
        /// </summary>
        public static void TestHugeArray()
        {
            var array = new MemoryArray<long>(1024 * 1024);
            for (var i = 0L; i < (long)int.MaxValue * 2; i++)
            {
                if (i >= array.Length)
                {
                    array.Resize(array.Length + (1024 * 1024));
                }
                array[i] = i * 4 + i / 3;

                var j = array[i];

                Assert.Equal(j, i * 4 + i / 3);
            }

        }

        /// <summary>
        /// Tests reading through the enumerator
        /// </summary>
        public static void TestEnumerableArray()
        {
            var len = 100;
            var array = new MemoryArray<long>(len);
            for (var i = 0L; i < len; i++) array[i] = i;
            foreach (var i in array) Assert.Equal(i, array[i]);
        }

        /// <summary>
        /// Tests using an array with Parallel Access
        /// </summary>
        public static void TestParallelAccess()
        {
            var len = 1024;
            var array = new MemoryArray<long>(len);
            for (var i = 0L; i < len; i++) array[i] = i;
            //Test accessing elements in parallel
            System.Linq.ParallelEnumerable.ForAll(array.AsParallel(), i =>
            {
                Assert.Equal(i, array[i]);
            });
        }
    }
}