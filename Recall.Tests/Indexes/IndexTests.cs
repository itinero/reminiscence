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
using Recall.Indexes;
using Recall.IO;

namespace Recall.Tests.Indexes
{
    /// <summary>
    /// Contains tests for the index.
    /// </summary>
    [TestFixture]
    public class IndexTests
    {
        /// <summary>
        /// Tests an index with just one element.
        /// </summary>
        [Test]
        public void TestOneElement()
        {
            using(var map = new MappedStream())
            {
                var index = new Index<string>(map.CreateVariableString);
                var id = index.Add("Ben");
                Assert.AreEqual("Ben", index.Get(id));
            }
        }

        /// <summary>
        /// Tests and index with tiny accessors.
        /// </summary>
        [Test]
        public void TestTinyAccessors()
        {
            using(var map = new MappedStream())
            {
                var index = new Index<string>(map.CreateVariableString, 32);

                var id1 = index.Add("Ben");
                var id2 = index.Add("Abelshausen");
                var id3 = index.Add("is");
                var id4 = index.Add("the");
                var id5 = index.Add("author");
                var id6 = index.Add("of");
                var id7 = index.Add("this");
                var id8 = index.Add("library");
                var id9 = index.Add("and");
                var id10 = index.Add("this");
                var id11 = index.Add("test!");

                Assert.AreEqual("Ben", index.Get(id1));
                Assert.AreEqual("Abelshausen", index.Get(id2));
                Assert.AreEqual("is", index.Get(id3));
                Assert.AreEqual("the", index.Get(id4));
                Assert.AreEqual("author", index.Get(id5));
                Assert.AreEqual("of", index.Get(id6));
                Assert.AreEqual("this", index.Get(id7));
                Assert.AreEqual("library", index.Get(id8));
                Assert.AreEqual("and", index.Get(id9));
                Assert.AreEqual("this", index.Get(id10));
                Assert.AreEqual("test!", index.Get(id11));
            }
        }
    }
}