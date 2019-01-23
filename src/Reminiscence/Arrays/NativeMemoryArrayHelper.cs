#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.Buffers;
using System.IO;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Contains non-generic helpers for <see cref="NativeMemoryArray{T}"/> to minimize code size.
    /// </summary>
    internal static class NativeMemoryArrayHelper
    {
        // the largest power of 2 that doesn't get allocated on the LOH, to ensure that lots of
        // these array copies don't cause fragmentation when they are all active at once.
        private const int StreamCopyBufferSize = 65536;

        internal static unsafe void CopyFromStream(Stream stream, byte* dst, long length)
        {
            if (length == 0)
            {
                return;
            }

            // note: .NET Core 2.1+ and platforms that implement .NET Standard 2.1+ can all read
            // directly into the Span<byte> without needing to lease a byte[] for this copying.
            byte[] buf = ArrayPool<byte>.Shared.Rent(StreamCopyBufferSize);
            try
            {
                Span<byte> bufSpan = buf;

                // use separate 32-bit and 64-bit variables to avoid unnecessary conversions.
                int bufLength32 = bufSpan.Length;
                long bufLength64 = bufLength32;
                while (length != 0)
                {
                    int toRead = bufLength32;
                    if (length < bufLength64)
                    {
                        toRead = unchecked((int)length);
                    }

                    int cur = stream.Read(buf, 0, toRead);
                    if (cur == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    bufSpan.Slice(0, cur).CopyTo(new Span<byte>(dst, cur));
                    length -= cur;
                    dst += cur;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        internal static unsafe void CopyToStream(byte* src, Stream stream, long length)
        {
            if (length == 0)
            {
                return;
            }

            // note: .NET Core 2.1+ and platforms that implement .NET Standard 2.1+ can all write
            // directly from the Span<byte> without needing to lease a byte[] for this copying.
            byte[] buf = ArrayPool<byte>.Shared.Rent(StreamCopyBufferSize);
            try
            {
                Span<byte> bufSpan = buf;

                // use separate 32-bit and 64-bit variables to avoid unnecessary conversions.
                int bufLength32 = bufSpan.Length;
                long bufLength64 = bufLength32;
                while (length != 0)
                {
                    int toWrite = bufLength32;
                    if (length < bufLength64)
                    {
                        toWrite = unchecked((int)length);
                        length = 0;
                    }
                    else
                    {
                        length -= bufLength64;
                    }

                    new Span<byte>(src, toWrite).CopyTo(bufSpan);
                    stream.Write(buf, 0, toWrite);
                    src += toWrite;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        internal static unsafe void memmove(byte* dst, byte* src, long length)
        {
            for (long cnt = length; cnt > 0; cnt -= int.MaxValue)
            {
                int toCopy = cnt < int.MaxValue
                    ? unchecked((int)cnt)
                    : int.MaxValue;

                new Span<byte>(src, toCopy).CopyTo(new Span<byte>(dst, toCopy));
                src += toCopy;
                dst += toCopy;
            }
        }

        internal static unsafe void ZeroFill(byte* head, long length)
        {
            for (long cnt = length; cnt > 0; cnt -= int.MaxValue)
            {
                int toZero = cnt < int.MaxValue
                    ? unchecked((int)cnt)
                    : int.MaxValue;

                new Span<byte>(head, toZero).Clear();
                head += toZero;
            }
        }
    }
}
#endif
