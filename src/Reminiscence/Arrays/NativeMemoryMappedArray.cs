#if SUPPORTS_NATIVE_MEMORY_ARRAY
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace Reminiscence.Arrays
{
    /// <summary>
    /// An array that holds instances of PDS types contiguously in virtual memory, backed by a
    /// "real" native memory-mapped file.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value to be stored in the array.
    /// </typeparam>
    public sealed unsafe class NativeMemoryMappedArray<T> : NativeMemoryArrayBase<T>
        where T : unmanaged
    {
        private readonly FileStream fileStream;

        private readonly long? byteOffset;

        private MemoryMappedFile memoryMappedFile;

        private MemoryMappedViewAccessor memoryMappedViewAccessor;

        private byte* acquiredPointer;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeMemoryMappedArray{T}"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The path to the file to memory-map.  If the file does not exist, then it will be created
        /// (with length 0).
        /// </param>
        public NativeMemoryMappedArray(string fileName)
        {
            this.fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                this.Initialize();
            }
            catch
            {
                this.fileStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeMemoryMappedArray{T}"/> class using
        /// only part of the given file.
        /// </summary>
        /// <param name="fileName">
        /// The path to the file to memory-map.  If the file does not exist, then an exception will
        /// be thrown.
        /// </param>
        /// <param name="byteOffset">
        /// The offset, <strong>in bytes</strong>, at which the data in the file starts.
        /// </param>
        /// <param name="length">
        /// The number of elements in the array.
        /// </param>
        public NativeMemoryMappedArray(string fileName, long byteOffset, long length)
        {
            this.fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                if (byteOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(byteOffset), byteOffset, "Must be non-negative.");
                }

                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length), length, "Must be non-negative.");
                }

                if (byteOffset + (length * Unsafe.SizeOf<T>()) > this.fileStream.Length)
                {
                    throw new ArgumentException("The given offset and length point to a section that extends beyond the end of the file.");
                }

                this.byteOffset = byteOffset;
                this.LengthCore = length;
                this.Initialize();
            }
            catch
            {
                this.fileStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="NativeMemoryMappedArray{T}"/> class.
        /// </summary>
        ~NativeMemoryMappedArray() => this.Dispose(false);

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override bool CanResize => !this.byteOffset.HasValue;

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

            if (size == this.LengthCore)
            {
                return;
            }

            if (!this.CanResize)
            {
                throw new InvalidOperationException("Capped memory-mapped arrays cannot be resized.  Please check CanResize before calling this method.");
            }

            long oldLength = this.LengthCore;

            this.ReleaseAllButFileStream();

            this.fileStream.SetLength(size * Unsafe.SizeOf<T>());
            this.Initialize();

            long newLength = this.LengthCore;
            if (oldLength < newLength)
            {
                byte* oldEnd = (byte*)&this.HeadPointer[oldLength];
                byte* newEnd = (byte*)&this.HeadPointer[newLength];
                NativeMemoryArrayHelper.ZeroFill(oldEnd, newEnd - oldEnd);
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.ReleasePointerIfAcquired();

            if (disposing)
            {
                this.ReleaseObjectsBetweenAcquiredPointerAndFileStream();
                this.fileStream.Dispose();
            }

            this.disposed = true;
        }

        private void Initialize()
        {
            long finalByteOffset, finalByteLength, finalLength;
            if (this.byteOffset.HasValue)
            {
                finalByteOffset = this.byteOffset.Value;
                finalLength = this.LengthCore;
                finalByteLength = finalLength * Unsafe.SizeOf<T>();
            }
            else
            {
                finalByteOffset = 0;
                finalByteLength = this.fileStream.Length;
                finalLength = finalByteLength / Unsafe.SizeOf<T>();
            }

            this.ReleaseAllButFileStream();

            try
            {
                if (finalLength == 0)
                {
                    return;
                }

#if NET45
                this.memoryMappedFile = MemoryMappedFile.CreateFromFile(this.fileStream, null, 0, MemoryMappedFileAccess.ReadWrite, null, HandleInheritability.None, leaveOpen: true);
#else
                this.memoryMappedFile = MemoryMappedFile.CreateFromFile(this.fileStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: true);
#endif
                this.memoryMappedViewAccessor = this.memoryMappedFile.CreateViewAccessor(finalByteOffset, finalByteLength);

                this.memoryMappedViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref this.acquiredPointer);

                // warning: this ignores the offset / length that we used to create the accessor, so
                // it's our responsibility to translate the offset and do bounds-checking.
                this.HeadPointer = (T*)(this.acquiredPointer + finalByteOffset);
                this.LengthCore = finalLength;
            }
            catch
            {
                this.ReleaseAllButFileStream();
                throw;
            }
        }

        private void ReleaseAllButFileStream()
        {
            this.ReleasePointerIfAcquired();
            this.ReleaseObjectsBetweenAcquiredPointerAndFileStream();
        }

        private void ReleasePointerIfAcquired()
        {
            if (this.acquiredPointer == null)
            {
                return;
            }

            this.memoryMappedViewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
            this.acquiredPointer = null;
        }

        private void ReleaseObjectsBetweenAcquiredPointerAndFileStream()
        {
            this.LengthCore = 0;
            this.HeadPointer = null;

            this.memoryMappedViewAccessor?.Dispose();
            this.memoryMappedViewAccessor = null;

            this.memoryMappedFile?.Dispose();
            this.memoryMappedFile = null;
        }
    }
}
#endif
