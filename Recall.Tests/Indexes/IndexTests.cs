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
using System.IO;

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
                var index = new Index<string>(map);
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
            using (var map = new MappedStream())
            {
                using (var tempStream = new MemoryStream(new byte[1024]))
                {
                    var index = new Index<string>(map, 32);

                    var element = "Ben";
                    var id1 = index.Add(element);
                    Assert.AreEqual(0, id1);
                    var id2Ref = MappedDelegates.WriteToString(tempStream, id1, ref element);

                    element = "Abelshausen";
                    var id2 = index.Add(element);
                    Assert.AreEqual(id2Ref, id2);
                    var id3Ref = MappedDelegates.WriteToString(tempStream, id2, ref element) + id2;

                    element = "is";
                    var id3 = index.Add(element);
                    Assert.AreEqual(id3Ref, id3);
                    var id4Ref = MappedDelegates.WriteToString(tempStream, id3, ref element) + id3;

                    element = "the";
                    var id4 = index.Add(element);
                    Assert.AreEqual(id4Ref, id4);
                    var id5Ref = MappedDelegates.WriteToString(tempStream, id4, ref element) + id4;

                    element = "author";
                    var id5 = index.Add(element);
                    Assert.AreEqual(id5Ref, id5);
                    var id6Ref = MappedDelegates.WriteToString(tempStream, id5, ref element) + id5;

                    element = "of";
                    var id6 = index.Add(element);
                    Assert.AreEqual(id6Ref, id6);
                    var id7Ref = MappedDelegates.WriteToString(tempStream, id6, ref element) + id6;

                    element = "this";
                    var id7 = index.Add(element);
                    Assert.AreEqual(id7Ref, id7);
                    var id8Ref = MappedDelegates.WriteToString(tempStream, id7, ref element) + id7;

                    element = "library";
                    var id8 = index.Add(element);
                    Assert.AreEqual(id8Ref, id8);
                    var id9Ref = MappedDelegates.WriteToString(tempStream, id8, ref element) + id8;

                    element = "and";
                    var id9 = index.Add(element);
                    Assert.AreEqual(id9Ref, id9);
                    var id10Ref = MappedDelegates.WriteToString(tempStream, id9, ref element) + id9;

                    element = "this";
                    var id10 = index.Add(element);
                    Assert.AreEqual(id10Ref, id10);
                    var id11Ref = MappedDelegates.WriteToString(tempStream, id10, ref element) + id10;

                    element = "test!";
                    var id11 = index.Add("test!");
                    Assert.AreEqual(id11Ref, id11);
                    var id12Ref = MappedDelegates.WriteToString(tempStream, id11, ref element) + id11;

                    Assert.AreEqual(id12Ref, index.SizeInBytes);

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

        /// <summary>
        /// Tests copying the data to a stream.
        /// </summary>
        public void TestCopyTo()
        {
            using (var map = new MappedStream())
            {
                using (var refStream = new MemoryStream(new byte[1024]))
                {
                    // write to index and to a stream.
                    var index = new Index<string>(map, 32);

                    var element = "Ben";
                    var id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "Abelshausen";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "is";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "the";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "author";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "of";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "this";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "library";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "and";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "this";
                    id = index.Add(element);
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    element = "test!";
                    id = index.Add("test!");
                    MappedDelegates.WriteToString(refStream, id, ref element);

                    refStream.SetLength(refStream.Position);

                    using(var indexStream = new MemoryStream((int)index.SizeInBytes))
                    {
                        var refBytes = refStream.ToArray();

                        Assert.AreEqual(refBytes.Length, index.CopyTo(indexStream));
                        var bytes = indexStream.ToArray();

                        Assert.AreEqual(index.SizeInBytes, bytes.Length);
                        Assert.AreEqual(index.SizeInBytes, refBytes.Length);

                        for(var i = 0; i < bytes.Length; i++)
                        {
                            Assert.AreEqual(refBytes[i], bytes[i]);
                        }
                    }

                    using (var indexStream = new MemoryStream((int)index.SizeInBytes + 8))
                    {
                        var refBytes = refStream.ToArray();

                        Assert.AreEqual(refBytes.Length + 8, index.CopyToWithSize(indexStream));
                        var bytes = indexStream.ToArray();

                        Assert.AreEqual(index.SizeInBytes, bytes.Length - 8);
                        Assert.AreEqual(index.SizeInBytes, refBytes.Length);

                        for (var i = 0; i < refBytes.Length; i++)
                        {
                            Assert.AreEqual(refBytes[i], bytes[i + 8]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests create from.
        /// </summary>
        public void TestCreateFrom()
        {
            byte[] data = null;
            var indexDictionary = new System.Collections.Generic.Dictionary<long, string>();

            using (var indexStream = new MemoryStream())
            {
                using (var map = new MappedStream())
                {
                    // write to index and to a stream.
                    var index = new Index<string>(map, 32);

                    var element = "Ben";
                    var id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "Abelshausen";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "is";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "the";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "author";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "of";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "this";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "library";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "and";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "this";
                    id = index.Add(element);
                    indexDictionary[id] = element;
                    element = "test!";
                    id = index.Add("test!");
                    indexDictionary[id] = element;

                    index.CopyToWithSize(indexStream);
                    data = indexStream.ToArray();
                }
            }

            using (var indexStream = new MemoryStream(data))
            {
                var index = Index<string>.CreateFromWithSize(indexStream);

                foreach(var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }
            }

            using (var indexStream = new MemoryStream(data))
            {
                var index = Index<string>.CreateFromWithSize(indexStream, true);

                foreach (var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }
            }
        }
    }
}