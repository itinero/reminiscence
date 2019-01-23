#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.Runtime.InteropServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Implementation of <see cref="IUnmanagedMemoryAllocator"/> that uses <see cref="SafeBuffer"/>
    /// as a source for the bytes.
    /// </summary>
    public sealed unsafe class SafeBufferMemoryAllocator : IUnmanagedMemoryAllocator
    {
        private SafeBuffer buffer;

        private byte* acquiredPointer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeBufferMemoryAllocator"/> class.
        /// </summary>
        /// <param name="buffer">
        /// The (initial) underlying <see cref="SafeBuffer"/> to allocate from.
        /// </param>
        public SafeBufferMemoryAllocator(SafeBuffer buffer) => this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

        /// <summary>
        /// Raised when a caller tries to allocate more room than we have available.
        /// </summary>
        public event EventHandler<BufferTooSmallEventArgs> BufferTooSmall;

        /// <inheritdoc />
        unsafe byte* IUnmanagedMemoryAllocator.Reallocate(byte* oldPointer, long oldByteCount, long newByteCount, bool zeroFill)
        {
            if (oldPointer != this.acquiredPointer)
            {
                throw new ArgumentException("the value must be the same as the value returned by the most recent call or, if this is the first call to this method, then the value must be null", nameof(oldPointer));
            }

            if (oldPointer != null)
            {
                this.buffer.ReleasePointer();
                this.acquiredPointer = null;
            }

            this.ValidateBufferLength(newByteCount);

            try
            {
                this.buffer.AcquirePointer(ref this.acquiredPointer);
                if (zeroFill)
                {
                    NativeMemoryArrayHelper.ZeroFill(this.acquiredPointer, newByteCount);
                }

                return this.acquiredPointer;
            }
            catch when (this.acquiredPointer != null)
            {
                this.buffer.ReleasePointer();
                this.acquiredPointer = null;
                throw;
            }
        }

        private void ValidateBufferLength(long newByteCount)
        {
            ulong newByteCountUnsigned = checked((ulong)newByteCount);
            if (newByteCountUnsigned <= this.buffer.ByteLength)
            {
                return;
            }

            // we're asked for a range of more bytes than our current buffer can hold, so use our
            // fancy event to see if we can resolve that issue.
            var args = new BufferTooSmallEventArgs(newByteCount, this.buffer);
            var handler = this.BufferTooSmall;
            if (handler != null)
            {
                handler(this, args);
                this.buffer = args.UnderlyingBuffer;

                if (newByteCountUnsigned <= this.buffer.ByteLength)
                {
                    return;
                }
            }

            throw new InvalidOperationException($"underlying buffer is too small to be able to fulfill the request for {newByteCount} byte(s) (buffer can only hold up to {this.buffer.ByteLength} bytes).");
        }
    }
}
#endif
