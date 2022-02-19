using System;
using System.Runtime.CompilerServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// An array that holds instances of PDS types contiguously in virtual memory using an injected
    /// instance of <see cref="IUnmanagedMemoryAllocator"/> to handle the actual allocations.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to be stored in the array.
    /// </typeparam>
    public sealed unsafe class NativeMemoryArray<T> : NativeMemoryArrayBase<T>
        where T : unmanaged
    {
        private readonly IUnmanagedMemoryAllocator allocator;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeMemoryArray{T}"/> class.
        /// </summary>
        /// <param name="allocator">
        /// The <see cref="IUnmanagedMemoryAllocator"/> to use to allocate, reallocate, and free
        /// contiguous blocks of virtual memory.
        /// </param>
        /// <param name="size">
        /// The number of elements that the array should be able to store.
        /// </param>
        public NativeMemoryArray(IUnmanagedMemoryAllocator allocator, long size)
        {
            this.allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
            this.Resize(size);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NativeMemoryArray{T}"/> class.
        /// </summary>
        ~NativeMemoryArray() => this.Resize(0);

        /// <inheritdoc />
        public override bool CanResize => true;

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Resize(0);
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override void Resize(long size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "Must be non-negative.");
            }

            if (size != 0 && this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            // this is the only line anywhere that ought to have any business calling Reallocate.
#pragma warning disable 618
            this.HeadPointer = (T*)this.allocator.Reallocate(oldPointer: (byte*)this.HeadPointer,
                                                             oldByteCount: this.LengthCore * Unsafe.SizeOf<T>(),
                                                             newByteCount: size * Unsafe.SizeOf<T>(),
                                                             zeroFill: true);
#pragma warning restore 618
            this.LengthCore = size;
        }
    }
}
