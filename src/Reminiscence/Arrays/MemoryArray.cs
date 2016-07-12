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

namespace Reminiscence.Arrays
{
    /// <summary>
    /// An in-memory array working around the pre .NET 4.5 memory limitations for one object.
    /// </summary>
    public class MemoryArray<T> : ArrayBase<T>
    {
        private T[][] blocks;
        private readonly int _blockSize = (int)System.Math.Pow(2, 20); // Holds the maximum array size, always needs to be a power of 2.
        private int _arrayPow = 20;
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
            if (blockSize < 0) { throw new ArgumentOutOfRangeException("Blocksize needs to be bigger than or equal to zero."); }
            if ((blockSize & (blockSize - 1)) != 0) { throw new ArgumentOutOfRangeException("Blocksize needs to be a power of 2."); }

            _blockSize = blockSize;
            _size = size;

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            blocks = new T[blockCount][];
            for (var i = 0; i < blockCount - 1; i++)
            {
                blocks[i] = new T[_blockSize];
            }
            if (blockCount > 0)
            {
                blocks[blockCount - 1] = new T[size - ((blockCount - 1) * _blockSize)];
            }
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
                return blocks[block][localIdx];
            }
            set
            {
                long block = (long)System.Math.Floor((double)idx / _blockSize);
                long localIdx = idx % _blockSize;
                blocks[block][localIdx] = value;
            }
        }

        /// <summary>
        /// Returns true if this array can be resized.
        /// </summary>
        public override bool CanResize
        {
            get { return true; }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <param name="size"></param>
        public override void Resize(long size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException("Cannot resize a huge array to a size of zero or smaller."); }

            _size = size;

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            if (blockCount != blocks.Length)
            {
                Array.Resize<T[]>(ref blocks, (int)blockCount);
            }
            for (int i = 0; i < blockCount - 1; i++)
            {
                if (blocks[i] == null)
                { // there is no array, create it.
                    blocks[i] = new T[_blockSize];
                }
                if (blocks[i].Length != _blockSize)
                { // the size is the same, keep it as it.
                    var localArray = blocks[i];
                    Array.Resize<T>(ref localArray, (int)_blockSize);
                    blocks[i] = localArray;
                }
            }
            if (blockCount > 0)
            {
                var lastBlockSize = size - ((blockCount - 1) * _blockSize);
                if (blocks[blockCount - 1] == null)
                { // there is no array, create it.
                    blocks[blockCount - 1] = new T[lastBlockSize];
                }
                if (blocks[blockCount - 1].Length != lastBlockSize)
                { // the size is the same, keep it as it.
                    var localArray = blocks[blockCount - 1];
                    Array.Resize<T>(ref localArray, (int)lastBlockSize);
                    blocks[blockCount - 1] = localArray;
                }
            }
        }

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public override long Length
        {
            get
            {
                return _size;
            }
        }

        /// <summary>
        /// Diposes of all associated native resources held by this object.
        /// </summary>
        public override void Dispose()
        {

        }
    }
}