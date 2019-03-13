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
            // to be considered: if we accept an existing FileStream, an offset, and a byte count,
            // then we can map just a section of the file so that multiple maps could be active at
            // once for the same file (though the arrays would become not-resizable).
            this.fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
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
        public override bool CanResize => true;

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
            this.ReleaseAllButFileStream();

            try
            {
                long finalLength = this.fileStream.Length / Unsafe.SizeOf<T>();
                if (finalLength == 0)
                {
                    return;
                }

#if NET45
                this.memoryMappedFile = MemoryMappedFile.CreateFromFile(this.fileStream, null, 0, MemoryMappedFileAccess.ReadWrite, null, HandleInheritability.None, leaveOpen: true);
#else
                this.memoryMappedFile = MemoryMappedFile.CreateFromFile(this.fileStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: true);
#endif
                this.memoryMappedViewAccessor = this.memoryMappedFile.CreateViewAccessor();

                this.memoryMappedViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref this.acquiredPointer);
                this.HeadPointer = (T*)this.acquiredPointer;
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
