using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Paging
{
    public class Page
    {
        public byte[] Data { get; }

        public int Size => Data.Length;

        public ref PageHeader FileHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.As<byte, PageHeader>(ref Data[0]);
        }

        public Page()
        {
            Data = new byte[Constants.PageSize];
        }

        public Page(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Span<byte> Slice(int offset, int length)
        {
            if (offset + length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return Data.AsSpan().Slice(offset, length);
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

            return ref Unsafe.As<byte, T>(ref Data[offset]);
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

            Unsafe.As<byte, T>(ref Data[offset]) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write(int offset, Span<byte> value)
        {
            if (offset + value.Length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.CopyBlockUnaligned(ref Data[offset], ref value[0], (uint)value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Write(int offset, ref byte value, uint length)
        {
            if (offset + length > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.CopyBlockUnaligned(ref Data[offset], ref value, (uint)length);
        }
    }
}
