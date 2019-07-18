using System;
using System.Collections.Generic;
using System.IO;
using Reminiscence.IO;

namespace Reminiscence.Arrays.Sparse
{
    /// <summary>
    /// An in-memory sparse array.
    /// </summary>
    public class SparseMemoryArray<T> : ArrayBase<T>
        where T : struct
    {
        private T[][] _blocks;
        private readonly int _blockSize; // Holds the maximum array size, always needs to be a power of 2.
        private readonly int _arrayPow;
        private long _size; // the total size of this array.
        private readonly T _default = default;
        
        /// <summary>
        /// Creates a new array.
        /// </summary>
        public SparseMemoryArray(long size, int blockSize = 1 << 16,
            T emptyDefault = default)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException(nameof(size), "Size needs to be bigger than or equal to zero."); }
            if (blockSize <= 0) { throw new ArgumentOutOfRangeException(nameof(blockSize),"Block size needs to be bigger than or equal to zero."); }
            if ((blockSize & (blockSize - 1)) != 0) { throw new ArgumentOutOfRangeException(nameof(blockSize),"Block size needs to be a power of 2."); }

            _default = emptyDefault;
            _blockSize = blockSize;
            _size = size;
            _arrayPow = ExpOf2(blockSize);

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            _blocks = new T[blockCount][];
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
        
        /// <inhertdoc/>
        public override T this[long idx]
        {
            get
            {
                var blockId = idx >> _arrayPow;
                var block = _blocks[blockId];
                if (block == null) return default(T);
                
                var localIdx = idx - (blockId << _arrayPow);
                return block[localIdx];
            }
            set
            {
                
                var blockId = idx >> _arrayPow;
                var block = _blocks[blockId];
                if (block == null)
                {
                    // don't create a new block for a default value.
                    //if (EqualityComparer<T>.Default.Equals(value, _default)) return;
                    
                    block = new T[_blockSize];
                    for (var i = 0; i < _blockSize; i++)
                    {
                        block[i] = _default;
                    }
                    _blocks[blockId] = block;
                }
                var localIdx = idx % _blockSize;
                _blocks[blockId][localIdx] = value;
            }
        }

        /// <inheritdoc/>
        public override void CopyFrom(ArrayBase<T> array)
        {
            if (array is SparseMemoryArray<T> otherSparse)
            {
                if (this._default.Equals(otherSparse._default) &&
                    this._blockSize == otherSparse._blockSize &&
                    this._size == otherSparse._size)
                {
                    for (var i = 0; i < _blocks.Length; i++)
                    {
                        var block = otherSparse._blocks[i];
                        if (block == null)
                        {
                            _blocks[i] = null;
                        }
                        else
                        {
                            _blocks[i] = otherSparse._blocks[i].Clone() as T[];
                        }
                    }
                    return;
                }
            }
            base.CopyFrom(array);
        }

        /// <inhertdoc/>
        public override bool CanResize => true;
        
        /// <inhertdoc/>
        public override void Resize(long size)
        {
            if (size < 0) { throw new ArgumentOutOfRangeException(nameof(size), "Cannot resize a huge array to a size of zero or smaller."); }

            _size = size;

            var blockCount = (long)System.Math.Ceiling((double)size / _blockSize);
            if (blockCount != _blocks.Length)
            {
                Array.Resize(ref _blocks, (int)blockCount);
            }
        }
        
        /// <summary>
        /// Copies this array to the stream with a header.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>The number of bytes written.</returns>
        public long CopyToWithHeader(Stream stream)
        {
            var position = stream.Position;
            using (var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(new MemoryMapStream(), 0))
            {
                // write non-null block count.
                var blocks = 0L;
                for (var b = 0; b < _blocks.Length; b++)
                {
                    if (_blocks[b] == null) continue;

                    blocks++;
                }
                stream.Write(BitConverter.GetBytes(blocks), 0, 8);
                
                // write size, blocksize and default value.
                stream.Write(BitConverter.GetBytes((long)_size), 0, 8);
                stream.Write(BitConverter.GetBytes((long)_blockSize), 0, 8);
                var d = _default;
                accessor.WriteTo(stream, stream.Position, ref d);
            
                // write non-null blocks on by one with the index as a prefix.
                for (var p = 0; p < _blocks.Length; p++)
                {
                    var block = _blocks[p];
                    if (block == null) continue;
                    
                    stream.Write(BitConverter.GetBytes((long)p), 0, 8);
                    for (var e = 0; e < block.Length; e++)
                    {
                        var element = block[e];
                        accessor.WriteTo(stream, stream.Position, ref element);
                    }
                }
            }
            return stream.Position - position;
        }
        
        /// <summary>
        /// Creates a new sparse memory array reading the size and copying from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A sparse memory array.</returns>
        public static SparseMemoryArray<T> CopyFromWithHeader(Stream stream)
        {
            var element = default(T);
            using (var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(new MemoryMapStream(), 0))
            {
                var buffer = new byte[8];
                stream.Read(buffer, 0, 8);
                var blockCount = BitConverter.ToInt64(buffer, 0);
                stream.Read(buffer, 0, 8);
                var size = BitConverter.ToInt64(buffer, 0);
                stream.Read(buffer, 0, 8);
                var blockSize = BitConverter.ToInt64(buffer, 0);
                accessor.ReadFrom(stream, stream.Position, ref element);
                
                var array = new SparseMemoryArray<T>(size, (int)blockSize, element);

                var b = 0;
                while (b < blockCount)
                {
                    stream.Read(buffer, 0, 8);
                    var blockPosition = BitConverter.ToInt64(buffer, 0);
                    var blockPointer = blockPosition * blockSize;
                    for (var p = 0; p < blockSize; p++)
                    {
                        accessor.ReadFrom(stream, stream.Position, ref element);
                        array[blockPointer + p] = element;
                    }

                    b++;
                }

                return array;
            }
        }

        /// <inhertdoc/>
        public override long Length => _size;
        
        /// <inhertdoc/>
        public override void Dispose()
        {
            
        }
    }
}