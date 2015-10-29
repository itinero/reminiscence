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

namespace Recall.IO
{
    /// <summary>
    /// Contains default read/write delegates for elements of several memory mapped data structures.
    /// </summary>
    public static class MappedDelegates
    {
        /// <summary>
        /// A delegate to create an accessor.
        /// </summary>
        public delegate MappedAccessor<T> CreateAccessorFunc<T>(long sizeInBytes);

        /// <summary>
        /// A default delegate that can be use to read strings from a stream.
        /// </summary>
        public static MappedFile.ReadFromDelegate<string> ReadFromString = new MappedFile.ReadFromDelegate<string>(
            (Stream stream, long position, ref string value) =>
        {
            var buffer = new byte[255]; // TODO: make sure this is not needed here anymore!
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var size = stream.ReadByte();
            int pos = 0;
            stream.Read(buffer, pos, size);
            while (size == 255)
            {
                pos = pos + size;
                size = stream.ReadByte();
                if (buffer.Length < size + pos)
                {
                    Array.Resize(ref buffer, size + pos);
                }
                stream.Read(buffer, pos, size);
            }
            pos = pos + size;
            value = System.Text.Encoding.Unicode.GetString(buffer, 0, pos);
            return pos;
        });

        /// <summary>
        /// A default delegate that can be use to write strings to a stream.
        /// </summary>
        public static MappedFile.WriteToDelegate<string> WriteToString = new MappedFile.WriteToDelegate<string>(
            (Stream stream, long position, ref string value) =>
        {
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var bytes = System.Text.Encoding.Unicode.GetBytes(value);
            var length = bytes.Length;
            for (int idx = 0; idx <= bytes.Length; idx = idx + 255)
            {
                var size = bytes.Length - idx;
                if (size > 255)
                {
                    size = 255;
                }

                if (stream.Length <= stream.Position + size + 1)
                { // past end of stream.
                    return -1;
                }
                stream.WriteByte((byte)size);
                stream.Write(bytes, idx, size);
                length++;
            }
            return length;
        });

        /// <summary>
        /// A default delegate that can be use to read arrays of integers from a stream.
        /// </summary>
        public static MappedFile.ReadFromDelegate<int[]> ReadFromIntArray = new MappedFile.ReadFromDelegate<int[]>(
            (Stream stream, long position, ref int[] value) =>
        {
            var buffer = new byte[255 * 4]; // TODO: make sure this is not needed here anymore!
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var size = stream.ReadByte();
            value = new int[size];
            int pos = 0;
            stream.Read(buffer, pos, size * 4);
            int idx = 0;
            for (int offset = 0; offset < size * 4; offset = offset + 4)
            {
                value[idx] = BitConverter.ToInt32(buffer, offset);
                idx++;
            }
            while (size == 255)
            {
                pos = pos + size;
                size = stream.ReadByte();
                Array.Resize<int>(ref value, value.Length + size);
                stream.Read(buffer, 0, size * 4);
                for (int offset = 0; offset < size * 4; offset = offset + 4)
                {
                    value[idx] = BitConverter.ToInt32(buffer, offset);
                    idx++;
                }
            }
            return pos;
        });

        /// <summary>
        /// A default delegate that can be use to write arrays of integers to a stream.
        /// </summary>
        public static MappedFile.WriteToDelegate<int[]> WriteToIntArray = new MappedFile.WriteToDelegate<int[]>(
            (Stream stream, long position, ref int[] value) =>
        {
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var length = value.Length * 4;
            for (int idx = 0; idx < value.Length; idx = idx + 255)
            {
                var size = value.Length - idx;
                if (size > 255)
                {
                    size = 255;
                }

                stream.WriteByte((byte)size);
                for (int offset = 0; offset < size; offset++)
                {
                    var bytes = BitConverter.GetBytes(value[idx + offset]);
                    if (stream.Position + bytes.Length <= stream.Length)
                    { // write is possible.
                        stream.Write(bytes, 0, 4);
                    }
                    else
                    { // write went pas end of stream, return -1 to indicate failiure to write data.
                        return -1;
                    }
                }
                length++;
            }
            return length;
        });

        /// <summary>
        /// A default delegate that can be use to read arrays of unsigned integers from a stream.
        /// </summary>
        public static MappedFile.ReadFromDelegate<uint[]> ReadFromUIntArray = new MappedFile.ReadFromDelegate<uint[]>(
            (Stream stream, long position, ref uint[] value) =>
        {
            var buffer = new byte[255 * 4]; // TODO: make sure this is not needed here anymore!
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var size = stream.ReadByte();
            value = new uint[size];
            int pos = 0;
            stream.Read(buffer, pos, size * 4);
            int idx = 0;
            for (int offset = 0; offset < size * 4; offset = offset + 4)
            {
                value[idx] = BitConverter.ToUInt32(buffer, offset);
                idx++;
            }
            while (size == 255)
            {
                pos = pos + size;
                size = stream.ReadByte();
                Array.Resize<uint>(ref value, value.Length + size);
                stream.Read(buffer, 0, size * 4);
                for (int offset = 0; offset < size * 4; offset = offset + 4)
                {
                    value[idx] = BitConverter.ToUInt32(buffer, offset);
                    idx++;
                }
            }
            return pos;
        });

        /// <summary>
        /// A default delegate that can be use to write arrays of unsigned integers to a stream.
        /// </summary>
        public static MappedFile.WriteToDelegate<uint[]> WriteToUIntArray = new MappedFile.WriteToDelegate<uint[]>(
            (Stream stream, long position, ref uint[] value) =>
        {
            stream.Seek(position, System.IO.SeekOrigin.Begin);
            var length = value.Length * 4;
            for (int idx = 0; idx < value.Length; idx = idx + 255)
            {
                var size = value.Length - idx;
                if (size > 255)
                {
                    size = 255;
                }

                stream.WriteByte((byte)size);
                for (int offset = 0; offset < size; offset++)
                {
                    stream.Write(BitConverter.GetBytes(value[idx + offset]), 0, 4);
                }
                length++;
            }
            return length;
        });
    }
}