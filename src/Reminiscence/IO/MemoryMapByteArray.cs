using System;
using System.IO;
using Reminiscence.IO.Streams;

namespace Reminiscence.IO;

/// <summary>
/// An in-memory memory map using a byte array.
/// </summary>
public class MemoryMapByteArray : MemoryMap
{
    private readonly byte[] _data; // Holds the data.

    /// <summary>
    /// Creates a new mapped stream.
    /// </summary>
    /// <param name="data">The data to read.</param>
    public MemoryMapByteArray(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<uint> DoCreateNewUInt32(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorUInt32(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<ushort> DoCreateNewUInt16(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorUInt16(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<int> DoCreateNewInt32(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorInt32(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<short> DoCreateNewInt16(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorInt16(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<byte> DoCreateNewByte(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorByte(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<float> DoCreateNewSingle(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorSingle(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<double> DoCreateNewDouble(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorDouble(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<ulong> DoCreateNewUInt64(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorUInt64(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <returns></returns>
    protected override MappedAccessor<long> DoCreateNewInt64(long position, long sizeInBytes)
    {
        return new Accessors.MappedAccessorInt64(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes));
    }

    /// <summary>
    /// Creates a new memory mapped file based on the given stream and the given size in bytes.
    /// </summary>
    /// <param name="position">The position to start at.</param>
    /// <param name="sizeInBytes">The size.</param>
    /// <param name="readFrom"></param>
    /// <param name="writeTo"></param>
    /// <returns></returns>
    protected override MappedAccessor<T> DoCreateVariable<T>(long position, long sizeInBytes,
        MemoryMap.ReadFromDelegate<T> readFrom, MemoryMap.WriteToDelegate<T> writeTo)
    {
        return new Accessors.MappedAccessorVariable<T>(this, new CappedStream(new MemoryStream(_data), position, sizeInBytes), readFrom,
            writeTo);
    }
}