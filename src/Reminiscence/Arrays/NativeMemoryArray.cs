#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// An array that holds instances of PDS types contiguously in virtual memory.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to be stored in the array.
    /// </typeparam>
    public sealed unsafe class NativeMemoryArray<T> : ArrayBase<T>
        where T : unmanaged
    {
        private readonly IUnmanagedMemoryAllocator allocator;

        private T* head;

        private long length;

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
        public override T this[long idx]
        {
            get => Unsafe.ReadUnaligned<T>(GetPointer(idx));
            set => Unsafe.WriteUnaligned(GetPointer(idx), value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T* GetPointer(long idx)
        {
            // handle non-negative and out-of-bounds with a single test
            if (unchecked((ulong)idx >= (ulong)this.length))
            {
                ThrowArgumentOutOfRangeExceptionForIndex();
            }

            return this.head + idx;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentOutOfRangeExceptionForIndex() => throw new ArgumentOutOfRangeException("idx", "Must be non-negative and less than the size of the array.");

        /// <inheritdoc />
        public override long Length => this.length;

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
            this.head = (T*)this.allocator.Reallocate(oldPointer: (byte*)this.head,
                                                      oldByteCount: this.length * Unsafe.SizeOf<T>(),
                                                      newByteCount: size * Unsafe.SizeOf<T>(),
                                                      zeroFill: true);
#pragma warning restore 618
            this.length = size;
        }

        /// <inheritdoc />
        public override unsafe void CopyFrom(ArrayBase<T> array, long index, long start, long count)
        {
            if (!(array is NativeMemoryArray<T> nativeArray))
            {
                base.CopyFrom(array, index, start, count);
                return;
            }

            if (start + count > nativeArray.length)
            {
                throw new ArgumentException("tried to copy more items than the source has available");
            }

            if (index + count > this.length)
            {
                throw new ArgumentException("tried to copy more items than the destination has room for");
            }

            byte* srcPtr = (byte*)(nativeArray.head + start);
            byte* dstPtr = (byte*)(this.head + index);
            NativeMemoryArrayHelper.memmove(dstPtr, srcPtr, count * Unsafe.SizeOf<T>());
        }

        /// <inheritdoc />
        public override unsafe void CopyFrom(Stream stream)
        {
            long len = this.length * Unsafe.SizeOf<T>();
            NativeMemoryArrayHelper.CopyFromStream(stream: stream ?? throw new ArgumentNullException(nameof(stream)),
                                                   dst:(byte*)this.head,
                                                   length: len);
        }

        /// <inheritdoc />
        public override unsafe long CopyTo(Stream stream)
        {
            long len = this.length * Unsafe.SizeOf<T>();
            NativeMemoryArrayHelper.CopyToStream(src: (byte*)this.head,
                                                 stream: stream ?? throw new ArgumentNullException(nameof(stream)),
                                                 length: len);
            return len;
        }
    }
}
#endif
