using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Paging
{
    public class PageAccessor
    {
        public byte[] Data { get; }

        public int Size => Data.Length;

        public ref int StoreId => ref Read<int>(Constants.FixedPagerIdOffset);

        public ref long PageNumber => ref Read<long>(Constants.FixedPageNumberOffset);

        public PageAccessor()
        {
            Data = new byte[Constants.PageSize];
        }

        public PageAccessor(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Span<byte> Slice(int offset, int len)
        {
            if (offset + len > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            return Data.AsSpan().Slice(offset, len);
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
        public virtual void Write(int offset, ref byte value, uint len)
        {
            if (offset + len > Size)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            Unsafe.CopyBlockUnaligned(ref Data[offset], ref value, (uint)len);
        }
    }
}
