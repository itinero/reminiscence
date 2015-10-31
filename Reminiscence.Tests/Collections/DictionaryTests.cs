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
using Reminiscence.Collections;
using Reminiscence.IO;
using System;
using System.IO;

namespace Reminiscence.Tests.Collections
{
    /// <summary>
    /// Contains tests for the dictionary.
    /// </summary>
    [TestFixture]
    public class DictionaryTests
    {
        /// <summary>
        /// Test creating.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            using(var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                Assert.AreEqual(0, dictionary.Count);
            }
        }

        /// <summary>
        /// Tests adding a value.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                dictionary.Add("Ben", "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary["Ben"]);

                Assert.Catch<ArgumentException>(() => dictionary.Add("Ben", "Not Abelshausen"));
            }

            MockObject.RegisterAccessorCreateFunc();
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<MockObject, string>(map);

                dictionary.Add(new MockObject("Ben"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben")]);

                Assert.Catch<ArgumentException>(() => dictionary.Add(new MockObject("Ben"), "Not Abelshausen"));

                dictionary.Add(new MockObject("Ben1"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben1")]);
                dictionary.Add(new MockObject("Ben2"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben2")]);
                dictionary.Add(new MockObject("Ben3"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben3")]);
                dictionary.Add(new MockObject("Ben4"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben4")]);
                dictionary.Add(new MockObject("Ben5"), "Abelshausen");
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben5")]);
            }
        }

        /// <summary>
        /// Tests contains key.
        /// </summary>
        [Test]
        public void TestContainsKey()
        {
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                dictionary.Add("Ben", "Abelshausen");

                Assert.IsTrue(dictionary.ContainsKey("Ben"));
            }

            MockObject.RegisterAccessorCreateFunc();
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<MockObject, string>(map);

                Assert.IsFalse(dictionary.ContainsKey(new MockObject("Ben")));
                Assert.IsFalse(dictionary.ContainsKey(new MockObject("Ben1")));

                dictionary.Add(new MockObject("Ben"), "Abelshausen");
                dictionary.Add(new MockObject("Ben1"), "Abelshausen");
                dictionary.Add(new MockObject("Ben2"), "Abelshausen");
                Assert.IsFalse(dictionary.ContainsKey(new MockObject("Ben3")));
                dictionary.Add(new MockObject("Ben3"), "Abelshausen");
                dictionary.Add(new MockObject("Ben4"), "Abelshausen");
                dictionary.Add(new MockObject("Ben5"), "Abelshausen");
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben")));
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben1")));
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben2")));
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben3")));
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben4")));
                Assert.IsTrue(dictionary.ContainsKey(new MockObject("Ben5")));
            }
        }

        /// <summary>
        /// Test getting a value.
        /// </summary>
        [Test]
        public void TestTryGetValue()
        {
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                dictionary.Add("Ben", "Abelshausen");

                string value;
                Assert.IsTrue(dictionary.TryGetValue("Ben", out value));
                Assert.AreEqual("Abelshausen", value);
            }

            MockObject.RegisterAccessorCreateFunc();
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<MockObject, string>(map);

                string value;
                Assert.IsFalse(dictionary.TryGetValue(new MockObject("Ben"), out value));
                Assert.IsFalse(dictionary.TryGetValue(new MockObject("Ben1"), out value));

                dictionary.Add(new MockObject("Ben"), "Abelshausen");
                dictionary.Add(new MockObject("Ben1"), "Abelshausen1");
                dictionary.Add(new MockObject("Ben2"), "Abelshausen2");
                Assert.IsFalse(dictionary.TryGetValue(new MockObject("Ben3"), out value));
                dictionary.Add(new MockObject("Ben3"), "Abelshausen3");
                dictionary.Add(new MockObject("Ben4"), "Abelshausen4");
                dictionary.Add(new MockObject("Ben5"), "Abelshausen5");
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben"), out value));
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben1"), out value));
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben2"), out value));
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben3"), out value));
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben4"), out value));
                Assert.IsTrue(dictionary.TryGetValue(new MockObject("Ben5"), out value));
            }
        }

        /// <summary>
        /// Tests removing a key.
        /// </summary>
        [Test]
        public void TestRemoveKey()
        {
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                dictionary.Add("Ben", "Abelshausen");

                Assert.IsFalse(dictionary.Remove("Ben2"));
                Assert.IsTrue(dictionary.Remove("Ben"));
                Assert.IsFalse(dictionary.Remove("Ben"));

                Assert.AreEqual(0, dictionary.Count);
            }

            MockObject.RegisterAccessorCreateFunc();
            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<MockObject, string>(map);

                dictionary.Add(new MockObject("Ben"), "Abelshausen");
                dictionary.Add(new MockObject("Ben1"), "Abelshausen1");
                dictionary.Add(new MockObject("Ben2"), "Abelshausen2");
                dictionary.Add(new MockObject("Ben3"), "Abelshausen3");
                dictionary.Add(new MockObject("Ben4"), "Abelshausen4");
                dictionary.Add(new MockObject("Ben5"), "Abelshausen5");

                Assert.IsTrue(dictionary.Remove(new MockObject("Ben4")));
                Assert.IsTrue(dictionary.Remove(new MockObject("Ben1")));
                Assert.IsTrue(dictionary.Remove(new MockObject("Ben2")));
                Assert.IsTrue(dictionary.Remove(new MockObject("Ben3")));
                Assert.IsTrue(dictionary.Remove(new MockObject("Ben")));
                Assert.IsTrue(dictionary.Remove(new MockObject("Ben5")));

                Assert.IsFalse(dictionary.Remove(new MockObject("Ben4")));
                Assert.IsFalse(dictionary.Remove(new MockObject("Ben1")));
                Assert.IsFalse(dictionary.Remove(new MockObject("Ben2")));
                Assert.IsFalse(dictionary.Remove(new MockObject("Ben3")));
                Assert.IsFalse(dictionary.Remove(new MockObject("Ben")));
                Assert.IsFalse(dictionary.Remove(new MockObject("Ben5")));
            }
        }

        /// <summary>
        /// Tests removing a key.
        /// </summary>
        [Test]
        public void TestItemGetOrSet()
        {
            MockObject.RegisterAccessorCreateFunc();

            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);

                dictionary["Ben"] = "Abelshausen";

                Assert.Catch<System.Collections.Generic.KeyNotFoundException>(() =>
                    {
                        var t = dictionary["Ben2"];
                    });
                Assert.AreEqual("Abelshausen", dictionary["Ben"]);
            }

            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<MockObject, string>(map);

                dictionary[new MockObject("Ben")] = "Abelshausen";
                dictionary[new MockObject("Ben1")] = "Abelshausen1";
                dictionary[new MockObject("Ben2")] = "Abelshausen2";
                dictionary[new MockObject("Ben3")] = "Abelshausen3";
                dictionary[new MockObject("Ben4")] = "Abelshausen4";
                dictionary[new MockObject("Ben5")] = "Abelshausen5";

                Assert.Catch<System.Collections.Generic.KeyNotFoundException>(() =>
                    {
                        var t = dictionary[new MockObject("Ben6")];
                    });
                Assert.AreEqual("Abelshausen", dictionary[new MockObject("Ben")]);
                Assert.AreEqual("Abelshausen1", dictionary[new MockObject("Ben1")]);
                Assert.AreEqual("Abelshausen2", dictionary[new MockObject("Ben2")]);
                Assert.AreEqual("Abelshausen3", dictionary[new MockObject("Ben3")]);
                Assert.AreEqual("Abelshausen4", dictionary[new MockObject("Ben4")]);
                Assert.AreEqual("Abelshausen5", dictionary[new MockObject("Ben5")]);

                dictionary[new MockObject("Ben")] = "Abelshausen1";
                dictionary[new MockObject("Ben1")] = "Abelshausen11";
                dictionary[new MockObject("Ben2")] = "Abelshausen12";
                dictionary[new MockObject("Ben3")] = "Abelshausen13";

                Assert.AreEqual("Abelshausen1", dictionary[new MockObject("Ben")]);
                Assert.AreEqual("Abelshausen11", dictionary[new MockObject("Ben1")]);
                Assert.AreEqual("Abelshausen12", dictionary[new MockObject("Ben2")]);
                Assert.AreEqual("Abelshausen13", dictionary[new MockObject("Ben3")]);
                Assert.AreEqual("Abelshausen4", dictionary[new MockObject("Ben4")]);
                Assert.AreEqual("Abelshausen5", dictionary[new MockObject("Ben5")]);
            }
        }

        /// <summary>
        /// Tests the enumerators.
        /// </summary>
        [Test]
        public void TestEnumerators()
        {
            MockObject.RegisterAccessorCreateFunc();

            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);
                dictionary["Ben"] = "Abelshausen";

                // use the enumerator to create a new dictionary.
                var refDictionary = new System.Collections.Generic.Dictionary<string, string>(
                    dictionary);

                Assert.AreEqual(1, refDictionary.Count);
                Assert.AreEqual("Abelshausen", dictionary["Ben"]);
            }

            using (var map = new MappedStream())
            {
                var dictionary = new Dictionary<string, string>(map);
                dictionary["Ben1"] = "Abelshausen1";
                dictionary["Ben2"] = "Abelshausen2";
                dictionary["Ben3"] = "Abelshausen3";
                dictionary["Ben4"] = "Abelshausen4";
                dictionary["Ben5"] = "Abelshausen5";

                // use the enumerator to create a new dictionary.
                var refDictionary = new System.Collections.Generic.Dictionary<string, string>(
                    dictionary);

                Assert.AreEqual(5, refDictionary.Count);
                Assert.AreEqual("Abelshausen1", dictionary["Ben1"]);
                Assert.AreEqual("Abelshausen2", dictionary["Ben2"]);
                Assert.AreEqual("Abelshausen3", dictionary["Ben3"]);
                Assert.AreEqual("Abelshausen4", dictionary["Ben4"]);
                Assert.AreEqual("Abelshausen5", dictionary["Ben5"]);
            }
        }

        private class MockObject
        {
            public MockObject(string payload)
            {
                this.Payload = payload;
            }

            public string Payload { get; set; }

            public override int GetHashCode()
            {
                return 0;
            }

            public override bool Equals(object obj)
            {
                if(obj is MockObject)
                {
                    return (obj as MockObject).Payload.Equals(this.Payload);
                }
                return false;
            }

            public static MappedAccessor<MockObject> CreateAccessor(MappedFile map, long sizeInBytes)
            {
                return map.CreateVariable<MockObject>(sizeInBytes, new MappedFile.ReadFromDelegate<MockObject>(
                    (Stream stream, long position, ref MockObject value) =>
                    {
                        var payload = string.Empty;
                        var size = MappedDelegates.ReadFromString(stream, position, ref payload);
                        value = new MockObject(payload);
                        return size;
                    }),
                    new MappedFile.WriteToDelegate<MockObject>(
                        (Stream stream, long position, ref MockObject value) =>
                        {
                            var payload = value.Payload;
                            return MappedDelegates.WriteToString(stream, position, ref payload);
                        }));
            }

            internal static void RegisterAccessorCreateFunc()
            {
                MappedFile.RegisterCreateAccessorFunc<MockObject>(
                    MockObject.CreateAccessor);
            }
        }
    }
}