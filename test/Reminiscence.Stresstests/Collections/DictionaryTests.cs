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
using Reminiscence.Collections;
using Reminiscence.IO;
using System;
using System.IO;
using System.Linq;

namespace Reminiscense.Stresstests.Collections
{
    /// <summary>
    /// Contains tests for the mapped dictionary.
    /// </summary>
    public static class DictionaryTests
    {
        /// <summary>
        /// Tests adding random keys.
        /// </summary>
        public static void TestRandom()
        {
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Create, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var random = new Random();
                    var dictionary = new Dictionary<int, long>(map);

                    var count = 65536 * 2;
                    var perf = new PerformanceInfoConsumer(
                        string.Format("Write Dictionary Random"), 1000);
                    perf.Start();
                    for (var i = 0; i < count; i++)
                    {
                        var r = random.Next();
                        dictionary[r] = (long)r * 2;

                        if (Global.Verbose && i % (count / 100) == 0)
                        {
                            perf.Report("Writing... {0}%", i, count - 1);
                        }
                    }
                    perf.Stop();
                }
            }
        }
        
        /// <summary>
        /// Tests adding random string.
        /// </summary>
        public static void TestRandomString()
        {
            using (var mapStream = new FileInfo(Global.FileName).Open(
                FileMode.Create, FileAccess.ReadWrite))
            {
                using (var map = new MemoryMapStream(mapStream))
                {
                    var dictionary = new Dictionary<string, long>(map);

                    var count = 65536 * 2;
                    var perf = new PerformanceInfoConsumer(
                        string.Format("Write Dictionary Random Strings"), 10000);
                    perf.Start();
                    for (var i = 0; i < count; i++)
                    {
                        var r = RandomString(5);
                        dictionary[r] = (long)(r.GetHashCode());

                        if (Global.Verbose && i % (count / 100) == 0)
                        {
                            perf.Report("Writing... {0}%", i, count - 1);
                        }
                    }
                    perf.Stop();
                }
            }
        }

        /// <summary>
        /// Tests adding random string.
        /// </summary>
        public static void TestRandomStringInMemory()
        {
            var dictionary = new Dictionary<string, long>();

            var count = 65536 * 16;
            var perf = new PerformanceInfoConsumer(
                string.Format("Write Dictionary Random Strings in memory"), 10000);
            perf.Start();
            for (var i = 0; i < count; i++)
            {
                var r = RandomString(5);
                dictionary[r] = (long) (r.GetHashCode());

                if (Global.Verbose && i % (count / 100) == 0)
                {
                    perf.Report("Writing... {0}%", i, count - 1);
                }
            }

            perf.Stop();
        }

        private static Random random = new Random(1220);
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}