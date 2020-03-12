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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Reminiscence.Indexes;
using Reminiscence.IO;

namespace Reminiscence.Tests.Indexes
{
    /// <summary>
    /// Contains tests for the index.
    /// </summary>
    public class IndexTests
    {
        /// <summary>
        /// Tests an index with just one element.
        /// </summary>
        [Test]
        public void TestOneElement()
        {
            using(var map = new MemoryMapStream())
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
            using(var map = new MemoryMapStream())
            {
                using(var tempStream = new MemoryStream(new byte[1024]))
                {
                    var index = new Index<string>(map, 32);

                    var element = "Ben";
                    var id1 = index.Add(element);
                    Assert.AreEqual(0, id1);
                    var id2Ref = MemoryMapDelegates.WriteToString(tempStream, id1, ref element);

                    element = "Abelshausen";
                    var id2 = index.Add(element);
                    Assert.AreEqual(id2Ref, id2);
                    var id3Ref = MemoryMapDelegates.WriteToString(tempStream, id2, ref element) + id2;

                    element = "is";
                    var id3 = index.Add(element);
                    Assert.AreEqual(id3Ref, id3);
                    var id4Ref = MemoryMapDelegates.WriteToString(tempStream, id3, ref element) + id3;

                    element = "the";
                    var id4 = index.Add(element);
                    Assert.AreEqual(id4Ref, id4);
                    var id5Ref = MemoryMapDelegates.WriteToString(tempStream, id4, ref element) + id4;

                    element = "author";
                    var id5 = index.Add(element);
                    Assert.AreEqual(id5Ref, id5);
                    var id6Ref = MemoryMapDelegates.WriteToString(tempStream, id5, ref element) + id5;

                    element = "of";
                    var id6 = index.Add(element);
                    Assert.AreEqual(id6Ref, id6);
                    var id7Ref = MemoryMapDelegates.WriteToString(tempStream, id6, ref element) + id6;

                    element = "this";
                    var id7 = index.Add(element);
                    Assert.AreEqual(id7Ref, id7);
                    var id8Ref = MemoryMapDelegates.WriteToString(tempStream, id7, ref element) + id7;

                    element = "library";
                    var id8 = index.Add(element);
                    Assert.AreEqual(id8Ref, id8);
                    var id9Ref = MemoryMapDelegates.WriteToString(tempStream, id8, ref element) + id8;

                    element = "and";
                    var id9 = index.Add(element);
                    Assert.AreEqual(id9Ref, id9);
                    var id10Ref = MemoryMapDelegates.WriteToString(tempStream, id9, ref element) + id9;

                    element = "this";
                    var id10 = index.Add(element);
                    Assert.AreEqual(id10Ref, id10);
                    var id11Ref = MemoryMapDelegates.WriteToString(tempStream, id10, ref element) + id10;

                    element = "test!";
                    var id11 = index.Add("test!");
                    Assert.AreEqual(id11Ref, id11);
                    var id12Ref = MemoryMapDelegates.WriteToString(tempStream, id11, ref element) + id11;

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
        [Test]
        public void TestCopyTo()
        {
            using(var map = new MemoryMapStream())
            {
                using(var refStream = new MemoryStream(new byte[1024]))
                {
                    // write to index and to a stream.
                    var index = new Index<string>(map, 32);

                    var element = "Ben";
                    var id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "Abelshausen";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "is";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "the";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "author";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "of";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "this";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "library";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "and";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "this";
                    id = index.Add(element);
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    element = "test!";
                    id = index.Add("test!");
                    MemoryMapDelegates.WriteToString(refStream, id, ref element);

                    refStream.SetLength(refStream.Position);

                    using(var indexStream = new MemoryStream((int) index.SizeInBytes))
                    {
                        var refBytes = refStream.ToArray();

                        Assert.AreEqual(refBytes.Length, index.CopyTo(indexStream));
                        var bytes = indexStream.ToArray();

                        Assert.AreEqual(index.SizeInBytes, bytes.Length);
                        Assert.AreEqual(index.SizeInBytes, refBytes.Length);

                        for (var i = 0; i < bytes.Length; i++)
                        {
                            Assert.AreEqual(refBytes[i], bytes[i]);
                        }
                    }

                    using(var indexStream = new MemoryStream((int) index.SizeInBytes + 8))
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
        [Test]
        public void TestCreateFrom()
        {
            byte[] data = null;
            var indexDictionary = new System.Collections.Generic.Dictionary<long, string>();

            using(var indexStream = new MemoryStream())
            {
                using(var map = new MemoryMapStream())
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

            using(var indexStream = new MemoryStream(data))
            {
                var index = Index<string>.CreateFromWithSize(indexStream);

                foreach (var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }
            }

            using(var indexStream = new MemoryStream(data))
            {
                var index = Index<string>.CreateFromWithSize(indexStream, true);

                foreach (var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }
            }
        }

        /// <summary>
        /// Tests create from and copy to in a row.
        /// </summary>
        [Test]
        public void TestCreateFromAndCopyTo()
        {
            byte[] data = null;
            var indexDictionary = new System.Collections.Generic.Dictionary<long, string>();

            using(var indexStream = new MemoryStream())
            {
                using(var map = new MemoryMapStream())
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

            using(var indexStream = new MemoryStream(data))
            {
                var index = Index<string>.CreateFromWithSize(indexStream);

                foreach (var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }

                using(var outputData = new MemoryStream())
                {
                    var size = index.CopyToWithSize(outputData);
                    Assert.AreEqual(data.Length, size);
                }
            }
        }
        /// <summary>
        /// Tests make writable after deserialization.
        /// </summary>
        [Test]
        public void TestMakeWritable()
        {
            byte[] data = null;
            var indexDictionary = new System.Collections.Generic.Dictionary<long, string>();

            // write to index and to a stream.
            var index = new Index<string>();

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

            using(var indexStream = new MemoryStream())
            {
                index.CopyToWithSize(indexStream);
                data = indexStream.ToArray();
            }

            using(var indexStream = new MemoryStream(data))
            {
                index = Index<string>.CreateFromWithSize(indexStream);
                index.MakeWritable(new MemoryMapStream());

                element = "These";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "are";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "updates";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "that";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "are";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "now";
                id = index.Add(element);
                indexDictionary[id] = element;
                element = "possible";
                id = index.Add(element);
                indexDictionary[id] = element;

                foreach (var refIndexElement in indexDictionary)
                {
                    var value = index.Get(refIndexElement.Key);
                    Assert.AreEqual(refIndexElement.Value, value);
                }
            }
        }

        /// <summary>
        /// Tests enumeration.
        /// </summary>
        [Test]
        public void TestEnumerationString()
        {
            var index = new Index<string>(new MemoryMapStream(), 32);
            var id = index.Add("this");
            id = index.Add("is");
            id = index.Add("another");
            id = index.Add("test");
            id = index.Add("sentence");

            var stringBuilder = new StringBuilder();
            foreach (var pair in index)
            {
                stringBuilder.Append(pair.Value);
            }
            Assert.AreEqual("thisisanothertestsentence", stringBuilder.ToString());
        }

        /// <summary>
        /// Tests <see cref="Index.Get(long)"/> in parallel
        /// </summary>
        [Test]
        public void TestGetParallel()
        {
            var index = new Index<string>(new MemoryMapStream(), 32);

            List<KeyValuePair<long, string>> ids = new List<KeyValuePair<long, string>>();

            ids.Add(new KeyValuePair<long, string>(index.Add("this"), "this"));
            ids.Add(new KeyValuePair<long, string>(index.Add("is"), "is"));
            ids.Add(new KeyValuePair<long, string>(index.Add("another"), "another"));
            ids.Add(new KeyValuePair<long, string>(index.Add("test"), "test"));
            ids.Add(new KeyValuePair<long, string>(index.Add("sentence"), "sentence"));

            ParallelEnumerable.ForAll(ids.AsParallel(), kvp =>
            {
                string found = index.Get(kvp.Key);
                Assert.AreEqual(found, kvp.Value);
            });
        }

        /// <summary>
        /// Tests enumeration.
        /// </summary>
        [Test]
        public void TestEnumerationUInt32Array()
        {
            var index = new Index<uint[]>(new MemoryMapStream(), 32);
            var id = index.Add(new uint[] { 1, 2 });
            id = index.Add(new uint[] { 3, 4 });
            id = index.Add(new uint[] { 5, 6, 7 });
            id = index.Add(new uint[] { 8, 9 });
            id = index.Add(new uint[] { 10 });

            var list = new List<uint>();
            foreach (var pair in index)
            {
                foreach (var item in pair.Value)
                {
                    list.Add(item);
                }
            }
            Assert.AreEqual(new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 
                list);
        }

        /// <summary>
        /// Tests enumeration.
        /// </summary>
        [Test]
        public void TestEnumerationInt32Array()
        {
            var index = new Index<int[]>(new MemoryMapStream(), 32);
            var id = index.Add(new int[] { 1, 2 });
            id = index.Add(new int[] { 3, 4 });
            id = index.Add(new int[] { 5, 6, 7 });
            id = index.Add(new int[] { 8, 9 });
            id = index.Add(new int[] { 10 });

            var list = new List<int>();
            foreach (var pair in index)
            {
                foreach (var item in pair.Value)
                {
                    list.Add(item);
                }
            }
            Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 
                list);
        }

        /// <summary>
        /// Tests enumeration.
        /// </summary>
        [Test]
        public void TestEnumerationAfterDeserializationInt32Array()
        {
            var index = new Index<int[]>(new MemoryMapStream(), 32);
            var id = index.Add(new int[] { 1, 2 });
            id = index.Add(new int[] { 3, 4 });
            id = index.Add(new int[] { 5, 6, 7 });
            id = index.Add(new int[] { 8, 9 });
            id = index.Add(new int[] { 10 });

            using (var stream = new MemoryStream())
            {
                index.CopyToWithSize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                index = Index<int[]>.CreateFromWithSize(stream);
            }

            var list = new List<int>();
            foreach (var pair in index)
            {
                foreach (var item in pair.Value)
                {
                    list.Add(item);
                }
            }
            Assert.AreEqual(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 
                list);
        }

        /// <summary>
        /// Tests enumeration.
        /// </summary>
        [Test]
        public void TestEnumerationAfterDeserializationEmpty()
        {
            var index = new Index<int[]>(new MemoryMapStream(), 32);

            using (var stream = new MemoryStream())
            {
                index.CopyToWithSize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                index = Index<int[]>.CreateFromWithSize(stream);
            }
            
            var enumerator = index.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests making an empty serialized index writable.
        /// </summary>
        [Test]
        public void TestMakeWritableEmpty()
        {
            var index = new Index<int[]>(new MemoryMapStream(), 32);

            using (var stream = new MemoryStream())
            {
                index.CopyToWithSize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                index = Index<int[]>.CreateFromWithSize(stream);
            }
            
            var id = index.Add(new int[] { 10, 100 });
            Assert.AreEqual(id, 0);
            Assert.AreEqual(new int[] { 10, 100 }, index.Get(id));
        }
    }
}