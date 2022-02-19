using System;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// Interface for allocating, reallocating, and freeing contiguous blocks of virtual memory.
    /// </summary>
    public interface IUnmanagedMemoryAllocator
    {
        /// <summary>
        /// Allocates, reallocates, or frees a contiguous block of virtual memory.
        /// </summary>
        /// <param name="oldPointer">
        /// Pointer to the old block, or <c>null</c> if there's nothing yet.
        /// </param>
        /// <param name="oldByteCount">
        /// The number of bytes previously allocated at <paramref name="oldPointer"/>.
        /// </param>
        /// <param name="newByteCount">
        /// The new number of bytes to be allocated at the new pointer, or 0 if we're just freeing.
        /// </param>
        /// <param name="zeroFill">
        /// A value indicating whether or not to initialize new blocks with 0.
        /// </param>
        /// <returns>
        /// A pointer to the start of the new block, or <c>null</c> if we're just freeing.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// May be thrown if <paramref name="oldPointer"/> is not equal to the return value of the
        /// most recent call to this method on this instance (or <c>null</c> if this method has not
        /// been called on this instance before).
        /// </exception>
        [Obsolete("This API supports the Itinero infrastructure and is not intended to be used directly from your code.")]
        unsafe byte* Reallocate(byte* oldPointer, long oldByteCount, long newByteCount, bool zeroFill);
    }
}