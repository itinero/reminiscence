#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Base class for arrays that hold instances of PDS types contiguously in virtual memory,
    /// irrespective of what "kind" of virtual memory that is ("regular" memory, memory-mapped file,
    /// other??).
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to be stored in the array.
    /// </typeparam>
    public abstract unsafe class NativeMemoryArrayBase<T> : ArrayBase<T>
        where T : unmanaged
    {
        // use backing fields for these two, to help make the MSIL for GetPointer as simple as it
        // possibly can be, in case that helps the JIT in any way (either now or in the future).
        private T* head;

        private long length;

        /// <inheritdoc />
        public sealed override T this[long idx]
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
        public sealed override long Length => this.length;

        /// <inheritdoc />
        public override bool CanResize => false;

        /// <inheritdoc />
        public override void Resize(long size) => throw new NotSupportedException("This array does not support resizing.  Please check CanResize before calling this method.");

        /// <summary>
        /// Gets or sets the unmanaged pointer to the first element in the array.  If the array is
        /// empty (i.e., <see cref="LengthCore"/> is 0), then this value may be undefined.
        /// </summary>
        protected T* HeadPointer
        {
            get => this.head;
            set => this.head = value;
        }

        /// <summary>
        /// Gets or sets the number of elements in the array starting at <see cref="HeadPointer"/>.
        /// Make sure you set this correctly!  Setting an incorrect value here is an invitation for
        /// access violations, which .NET developers typically do not deal with.
        /// </summary>
        /// <remarks>
        /// When this value is 0, the base class will not attempt to use <see cref="HeadPointer"/>
        /// in any meaningful way, so it is recommended that the <see cref="ArrayBase{T}.Dispose"/>
        /// implementation ultimately does so.
        /// </remarks>
        protected long LengthCore
        {
            get => this.length;
            set => this.length = value;
        }

        /// <inheritdoc />
        public sealed override unsafe void CopyFrom(ArrayBase<T> array, long index, long start, long count)
        {
            if (!(array is NativeMemoryArrayBase<T> nativeArray))
            {
                base.CopyFrom(array, index, start, count);
                return;
            }

            if (start + count > nativeArray.LengthCore)
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
        public sealed override unsafe void CopyFrom(Stream stream)
        {
            long len = this.length * Unsafe.SizeOf<T>();
            NativeMemoryArrayHelper.CopyFromStream(stream: stream ?? throw new ArgumentNullException(nameof(stream)),
                                                   dst: (byte*)this.head,
                                                   length: len);
        }

        /// <inheritdoc />
        public sealed override unsafe long CopyTo(Stream stream)
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
