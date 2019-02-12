using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Data
{
    public abstract class AbstractPage
    {
        private byte[] _data;

        /// <summary>
        /// page data size
        /// </summary>
        public int Size => _data.Length;

        /// <summary>
        /// page data ref
        /// </summary>
        public ref byte Ptr => ref _data[0];

        public AbstractPage()
        {
            _data = new byte[Constants.PageSize];
        }

        public AbstractPage(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Span<byte> Slice(int offset, int length)
        {
            if (offset + length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return _data.AsSpan().Slice(offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ref T Read<T>(int offset) where T : struct
        {
            return ref Read<T>(offset, Unsafe.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ref T Read<T>(int offset, int sizeOf) where T : struct
        {
            if (sizeOf + offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return ref Unsafe.As<byte, T>(ref _data[offset]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write<T>(int offset, T value) where T : struct
        {
            Write(offset, value, Unsafe.SizeOf<T>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write<T>(int offset, T value, int sizeOf) where T : struct
        {
            if (sizeOf + offset > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.As<byte, T>(ref _data[offset]) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write(int offset, Span<byte> value)
        {
            if (offset + value.Length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.CopyBlockUnaligned(ref _data[offset], ref value[0], (uint)value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write(int offset, ref byte value, uint length)
        {
            if (offset + length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.CopyBlockUnaligned(ref _data[offset], ref value, (uint)length);
        }
    }
}
