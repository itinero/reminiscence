#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.Runtime.InteropServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Data for an event that's signaled when we're asked to acquire a <see cref="SafeBuffer"/>'s
    /// pointer and use it for a range that's bigger than the buffer can hold.
    /// </summary>
    public sealed class BufferTooSmallEventArgs : EventArgs
    {
        private SafeBuffer underlyingBuffer;

        internal BufferTooSmallEventArgs(long requestedByteCount, SafeBuffer originalBuffer)
        {
            this.RequestedByteCount = requestedByteCount;
            this.underlyingBuffer = originalBuffer;
        }

        /// <summary>
        /// Gets the number of bytes requested (known to be larger than the original
        /// <see cref="UnderlyingBuffer"/>'s <see cref="SafeBuffer.ByteLength"/>).
        /// </summary>
        public long RequestedByteCount { get; }

        /// <summary>
        /// Gets or sets the <see cref="SafeBuffer"/> to fill the request.
        /// Initially, this will be the old instance whose <see cref="SafeBuffer.ByteLength"/> was
        /// too small for <see cref="RequestedByteCount"/>.  The handler may set this to a new
        /// <see cref="SafeBuffer"/> instance that the sender may use for the remainder of its
        /// lifetime; if it does, then it's responsible for disposing the old instance and any other
        /// objects associated with it (e.g., memory-mapped view accessors, memory-mapped files,
        /// file streams, etc.).
        /// </summary>
        public SafeBuffer UnderlyingBuffer
        {
            get => this.underlyingBuffer;
            set => this.underlyingBuffer = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
#endif
