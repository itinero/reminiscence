using System;
using System.IO;

namespace Reminiscence.IO.Accessors
{
    /// <summary>
    /// A memory mapped accessor that stores shorts.
    /// </summary>
    public sealed class MappedAccessorByte : MappedAccessor<byte>
    {
        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedAccessorByte(MemoryMap file, byte[] data)
            : base(file, data, 1)
        {
            
        }
        
        /// <summary>
        /// Creates a new memory mapped file.
        /// </summary>
        public MappedAccessorByte(MemoryMap file, Stream stream)
            : base(file, stream, 1)
        {
            
        }

        /// <summary>
        /// Reads appropriate amount of bytes from the stream at the given position and returns the structure.
        /// </summary>
        public override long ReadFrom(Stream stream, long position, ref byte structure)
        {
            if (stream.Position != position)
            {
                stream.Seek(position, SeekOrigin.Begin);
            }
            var value = stream.ReadByte();
            if (value == -1)
            {
                structure = 0;
                return 0;
            }

            structure = (byte) value;
            return _elementSize;
        }

        /// <summary>
        /// Reads elements starting at the given position.
        /// </summary>
        public override int ReadArray(long position, byte[] array, int offset, int count)
        {
            if (_stream.Length <= position)
            { // cannot seek to this location, past the end of the stream.
                return -1;
            }

            // try and read everything.
            var elementsRead = Math.Min((int)((_stream.Length - position) / _elementSize), count);
            if (elementsRead > 0)
            { // ok, read.
                var bufferSize = array.Length * _elementSize;
                if (_stream.Position != position)
                {
                    _stream.Seek(position, SeekOrigin.Begin);
                }
                _stream.Read(array, 0, array.Length);
            }
            return elementsRead;
        }

        /// <summary>
        /// Converts the structure to bytes and writes them to the stream.
        /// </summary>
        public override long WriteTo(Stream stream, long position, ref byte structure)
        {
            if (stream.Position != position)
            {
                stream.Seek(position, SeekOrigin.Begin);
            }
            stream.WriteByte(structure);
            return _elementSize;
        }

        /// <summary>
        /// Writes an array of elements at the given position.
        /// </summary>
        public override long WriteArray(long position, byte[] array, int offset, int count)
        {
            var size = 0L;
            _stream.Seek(position, SeekOrigin.Begin);
            for (var i = 0; i < count; i++)
            {
                _stream.WriteByte(array[i]);
                size++;
            }
            return size;
        }
    }
}