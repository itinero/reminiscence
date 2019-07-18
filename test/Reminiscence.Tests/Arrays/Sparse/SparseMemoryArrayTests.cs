using System.IO;
using NUnit.Framework;
using Reminiscence.Arrays.Sparse;

namespace Reminiscence.Tests.Arrays.Sparse
{
    public class SparseMemoryArrayTests
    {
        [Test]
        public void SparseMemoryArray_ShouldStoreLikeArrayInt()
        {
            var arrayRef = new int[100000];
            var array = new SparseMemoryArray<int>(100000, 512);

            var i = 1;
            while (i < array.Length)
            {
                array[i] = i;
                arrayRef[i] = i;
                
                i *= 2;
            }

            for (var j = 0; j < array.Length; j++)
            {
                var a = array[j];
                var b = arrayRef[j];
                
                Assert.AreEqual(a, b);
            }
        }
        [Test]
        public void SparseMemoryArray_ShouldStoreLikeArrayByte()
        {
            var arrayRef = new byte[10000000];
            var array = new SparseMemoryArray<byte>(10000000, 512);

            var i = 1;
            while (i < array.Length)
            {
                array[i] = (byte)(i % byte.MaxValue);
                arrayRef[i] = (byte)(i % byte.MaxValue);
                
                i *= 2;
            }

            for (var j = 0; j < array.Length; j++)
            {
                var a = array[j];
                var b = arrayRef[j];
                
                Assert.AreEqual(a, b);
            }
        }

        [Test]
        public void SparseMemoryArray_ShouldSerializeDeserialize()
        {
            var array = new SparseMemoryArray<int>(100000, 512);

            var i = 1;
            while (i < array.Length)
            {
                array[i] = i;
                
                i *= 2;
            }

            using (var memoryStream = new MemoryStream())
            {
                array.CopyToWithHeader(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var array1 = SparseMemoryArray<int>.CopyFromWithHeader(memoryStream);
                for (var j = 0; j < array.Length; j++)
                {
                    Assert.AreEqual(array[j], array1[j]);
                }
            }
        }
    }
}