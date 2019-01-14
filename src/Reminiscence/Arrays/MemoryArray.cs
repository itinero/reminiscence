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

using System;
using System.IO;
using Reminiscence.IO;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// An in-memory array working around the pre .NET 4.5 memory limitations for one object.
    /// </summary>
    public class MemoryArray<T> : ArrayBase<T>
    {
        private T[][] _blocks;
        private readonly int _blockSize; // Holds the maximum array size, always needs to be a power of 2.
        private readonly int _arrayPow;
        private long _size; // the total size of this array.

        /// <summary>
        /// Creates a new array.
        /// </summary>
        public MemoryArray(long size)
            : this(size, (int)System.Math.Pow(2, 20))
        {

        }

        /// <summary>
        /// Creates a new array.
        /// </summary>
        public MemoryArray(long size, int blockSize)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException("Size needs to be bigger than or equal to zero."); }
            if (blockSize <= 0) { throw new ArgumentOutOfRangeException("Blocksize needs to be bigger than or equal to zero."); }
            if ((blockSize & (blockSize - 1)) != 0) { throw new ArgumentOutOfRangeException("Blocksize needs to be a power of 2."); }

            _blockSize = blockSize;
            _size = size;
            _arrayPow = ExpOf2(blockSize);

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            _blocks = new T[blockCount][];
            for (var i = 0; i < blockCount - 1; i++)
            {
                _blocks[i] = new T[_blockSize];
            }
            if (blockCount > 0)
            {
                _blocks[blockCount - 1] = new T[size - ((blockCount - 1) * _blockSize)];
            }
        }

        private static int ExpOf2(int powerOf2)
        { // this can probably be faster but it needs to run once in the constructor,
            // feel free to improve but not crucial.
            if (powerOf2 == 1)
            {
                return 0;
            }
            return ExpOf2(powerOf2 / 2) + 1;
        }

        /// <summary>
        /// Gets or sets the element at the given idx.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public override T this[long idx]
        {
            get
            {
                var block = idx >> _arrayPow;
                var localIdx = idx - (block << _arrayPow);
                return _blocks[block][localIdx];
            }
            set
            {
                var block = (long)System.Math.Floor((double)idx / _blockSize);
                var localIdx = idx % _blockSize;
                _blocks[block][localIdx] = value;
            }
        }

        /// <summary>
        /// Returns true if this array can be resized.
        /// </summary>
        public override bool CanResize => true;

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <param name="size"></param>
        public override void Resize(long size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException("Cannot resize a huge array to a size of zero or smaller."); }

            _size = size;

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            if (blockCount != _blocks.Length)
            {
                Array.Resize<T[]>(ref _blocks, (int)blockCount);
            }
            for (var i = 0; i < blockCount - 1; i++)
            {
                if (_blocks[i] == null)
                { // there is no array, create it.
                    _blocks[i] = new T[_blockSize];
                }
                if (_blocks[i].Length != _blockSize)
                { // the size is the same, keep it as it.
                    var localArray = _blocks[i];
                    Array.Resize<T>(ref localArray, (int)_blockSize);
                    _blocks[i] = localArray;
                }
            }
            if (blockCount > 0)
            {
                var lastBlockSize = size - ((blockCount - 1) * _blockSize);
                if (_blocks[blockCount - 1] == null)
                { // there is no array, create it.
                    _blocks[blockCount - 1] = new T[lastBlockSize];
                }
                if (_blocks[blockCount - 1].Length != lastBlockSize)
                { // the size is the same, keep it as it.
                    var localArray = _blocks[blockCount - 1];
                    Array.Resize<T>(ref localArray, (int)lastBlockSize);
                    _blocks[blockCount - 1] = localArray;
                }
            }
        }

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public override long Length => _size;

        /// <summary>
        /// Creates a new memory array reading the size and copying from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A memory array.</returns>
        public static MemoryArray<T> CopyFromWithSize(Stream stream)
        {
            using (var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(new MemoryMapStream(), 0))
            {
                var buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                var size = BitConverter.ToInt64(buffer, 0);

                long length;
                if (!accessor.ElementSizeFixed) 
                { // if the element size is not fixed, it should have been written here if copy to with size was used.
                    stream.Read(buffer, 0, 8);
                    length = BitConverter.ToInt64(buffer, 0);
                }
                else
                { // the length of the array can be calculate from the size of the data.
                    length = size / accessor.ElementSize;
                }
             
                var memoryArray = new MemoryArray<T>(length);
                memoryArray.CopyFrom(accessor, stream);
                return memoryArray;
            }    
        }

        /// <summary>
        /// Disposes of all associated native resources held by this object.
        /// </summary>
        public override void Dispose()
        {

        }
    }
}