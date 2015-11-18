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
using System.IO;

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
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Create, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var array = new Array<int>(map, Global.ArrayTestLength, 
                        ArrayProfile.NoCache);

                    var perf = new PerformanceInfoConsumer("Write Array", 1000);
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
    }
}