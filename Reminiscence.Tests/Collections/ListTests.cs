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
using Reminiscence.Collections;
using Reminiscence.IO;
using System;
using System.Linq;

namespace Reminiscence.Tests.Collections
{
    /// <summary>
    /// Contains tests for the list.
    /// </summary>
    [TestFixture]
    public class ListTests
    {
        /// <summary>
        /// Tests creating new list(s).
        /// </summary>
        [Test]
        public void TestCreateNew()
        {
            using(var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                Assert.AreEqual(0, list.Count);
                Assert.AreEqual(1024, list.Capacity);

                list = new List<int>(map, 10);
                Assert.AreEqual(0, list.Count);
                Assert.AreEqual(10, list.Capacity);
            }
        }

        /// <summary>
        /// Tests adding new items.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                list.Add(100);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.AreEqual(1024, list.Capacity);

                for(var i = 0; i < 1023; i++)
                {
                    list.Add(i + 101);
                }

                Assert.AreEqual(1024, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.AreEqual(1024, list.Capacity);

                list.Add(1002);

                Assert.AreEqual(1025, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.AreEqual(1049600, list.Capacity);
            }
        }

        /// <summary>
        /// Tests clearing all items.
        /// </summary>
        [Test]
        public void TestClear()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                list.Add(100);
                list.Clear();

                Assert.AreEqual(0, list.Count);
                Assert.AreEqual(1024, list.Capacity);

                for (var i = 0; i < 1024; i++)
                {
                    list.Add(i + 101);
                }
                list.Clear();
                Assert.AreEqual(1024, list.Capacity);

                for (var i = 0; i < 1025; i++)
                {
                    list.Add(i + 101);
                }
                list.Clear();
                Assert.AreEqual(1049600, list.Capacity);
            }
        }

        /// <summary>
        /// Tests index of.
        /// </summary>
        [Test]
        public void TestIndexOf()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 1024; i++)
                {
                    list.Add(i);
                }

                for(var i = 0; i < 1024; i++)
                {
                    Assert.AreEqual(i, list.IndexOf(i));
                }
                Assert.AreEqual(-1, list.IndexOf(2048));
            }
        }

        /// <summary>
        /// Tests contains.
        /// </summary>
        [Test]
        public void TestContains()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 1024; i++)
                {
                    list.Add(i);
                }

                for (var i = 0; i < 1024; i++)
                {
                    Assert.IsTrue(list.Contains(i));
                }
                Assert.IsFalse(list.Contains(2048));
            }
        }

        /// <summary>
        /// Tests copy to.
        /// </summary>
        [Test]
        public void TestCopyTo()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 10; i++)
                {
                    list.Add(i);
                }

                var array = new int[20];
                list.CopyTo(array, 0);

                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(i, array[i]);
                }

                list.CopyTo(array, 10);

                for (var i = 10; i < 20; i++)
                {
                    Assert.AreEqual(i - 10, array[i]);
                }
            }
        }

        /// <summary>
        /// Tests insert at.
        /// </summary>
        [Test]
        public void TestInsert()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 1; i < 10; i++)
                {
                    list.Add(i);
                }
                list.Insert(0, 0);

                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(i, list[i]);
                }
            }
        }

        /// <summary>
        /// Tests set item.
        /// </summary>
        [Test]
        public void TestSetItem()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 10; i++)
                {
                    list.Add(i + 100);
                }
                for (var i = 0; i < 10; i++)
                {
                    list[i] = i;
                }
                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(i, list[i]);
                }
                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
                    list[11] = 0;
                });
                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
                    list[-1] = 0;
                });
                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
                    var t = list[11];
                });
                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
                    var t = list[-1];
                });
            }
        }

        /// <summary>
        /// Tests is readonly.
        /// </summary>
        [Test]
        public void TestIsReadonly()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                Assert.IsFalse(list.IsReadOnly);
            }
        }

        /// <summary>
        /// Tests removing an item.
        /// </summary>
        [Test]
        public void TestRemove()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 10; i++)
                {
                    list.Add(i);
                }
                list.Insert(2, 101);
                list.Insert(3, 104);
                list.Insert(4, 501);
                list.Insert(5, 201);

                Assert.IsTrue(list.Remove(101));
                Assert.IsTrue(list.Remove(104));
                Assert.IsTrue(list.Remove(501));
                Assert.IsTrue(list.Remove(201));

                Assert.IsFalse(list.Remove(101));
                Assert.IsFalse(list.Remove(104));
                Assert.IsFalse(list.Remove(501));
                Assert.IsFalse(list.Remove(201));

                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(i, list[i]);
                }
            }
        }

        /// <summary>
        /// Tests removing an item.
        /// </summary>
        [Test]
        public void TestRemoveAt()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 10; i++)
                {
                    list.Add(i);
                }
                list.Insert(2, 101);
                list.Insert(3, 104);
                list.Insert(4, 501);
                list.Insert(5, 201);

                list.RemoveAt(2);
                list.RemoveAt(2);
                list.RemoveAt(2);
                list.RemoveAt(2);

                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(i, list[i]);
                }
            }
        }

        /// <summary>
        /// Tests enumerator.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            using (var map = new MemoryMapStream())
            {
                var list = new List<int>(map);
                for (var i = 0; i < 10; i++)
                {
                    list.Add(i);
                }

                var otherList = new System.Collections.Generic.List<int>(
                    list.Where((i) => true));

                for (var i = 0; i < 10; i++)
                {
                    Assert.AreEqual(list[i], otherList[i]);
                }
            }
        }
    }
}