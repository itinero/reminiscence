#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.Runtime.InteropServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Implementation of <see cref="IUnmanagedMemoryAllocator"/> that uses the global heap.
    /// </summary>
    public sealed class DefaultUnmanagedMemoryAllocator : IUnmanagedMemoryAllocator
    {
        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static readonly DefaultUnmanagedMemoryAllocator Instance = new DefaultUnmanagedMemoryAllocator();

        private DefaultUnmanagedMemoryAllocator() { }

        /// <inheritdoc />
        unsafe byte* IUnmanagedMemoryAllocator.Reallocate(byte* oldPointer, long oldByteCount, long newByteCount, bool zeroFill)
        {
            if (oldPointer == null)
            {
                if (newByteCount == 0)
                {
                    return null;
                }

                byte* allocated = (byte*)Marshal.AllocHGlobal(new IntPtr(newByteCount)).ToPointer();
                GC.AddMemoryPressure(newByteCount);
                if (zeroFill)
                {
                    NativeMemoryArrayHelper.ZeroFill(allocated, newByteCount);
                }

                return allocated;
            }

            if (newByteCount == 0)
            {
                Marshal.FreeHGlobal(new IntPtr(oldPointer));
                GC.RemoveMemoryPressure(oldByteCount);
                return null;
            }

            byte* reallocated = (byte*)Marshal.ReAllocHGlobal(new IntPtr(oldPointer), new IntPtr(newByteCount)).ToPointer();
            if (oldByteCount < newByteCount)
            {
                if (zeroFill)
                {
                    NativeMemoryArrayHelper.ZeroFill(reallocated + oldByteCount, newByteCount - oldByteCount);
                }

                GC.AddMemoryPressure(newByteCount - oldByteCount);
            }
            else if (newByteCount < oldByteCount)
            {
                GC.RemoveMemoryPressure(oldByteCount - newByteCount);
            }

            return reallocated;
        }
    }
}
#endif
