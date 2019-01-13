using NUnit.Framework;
using Reminiscence.Arrays;
using Reminiscence.IO;

namespace Reminiscence.Tests.Arrays
{
    /// <summary>
    /// Contains tests for an array of shorts.
    /// </summary>
    public class ArrayByteTests
    {
        /// <summary>
        /// Tests the basics.
        /// </summary>
        [Test]
        public void TestBasics()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new Array<byte>(map, 1024);
                for (ushort i = 0; i < 1024; i++)
                {
                    array[i] = (byte)(i ^ 6983);
                }

                for (ushort i = 0; i < 1024; i++)
                {
                    Assert.AreEqual(array[i], (byte)(i ^ 6983));
                }
            }
        }
    }
}