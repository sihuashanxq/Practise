using System.Runtime.CompilerServices;
using Vicuna.Storage.Data;

namespace Vicuna.Storage
{
    internal static class UnSafeByteExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue Get<TValue>(this byte[] @this, int index) where TValue : struct
        {
            return ref @this[index].To<TValue>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this byte[] @this, int index, ByteString value)
        {
            @this[index].Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<TValue>(this byte[] @this, int index, TValue value) where TValue : struct
        {
            @this[index].Set(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this byte[] @this, int index, ByteString value, uint count)
        {
            @this[index].Set(value, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TValue To<TValue>(this ref byte @this) where TValue : struct
        {
            return ref Unsafe.As<byte, TValue>(ref @this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<TValue>(this ref byte @this, TValue value) where TValue : struct
        {
            Unsafe.As<byte, TValue>(ref @this) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref byte @this, ByteString value, uint count)
        {
            value.Ptr.CopyTo(ref @this, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref byte @this, ByteString value)
        {
            @this.Set(value, value.Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteString AsByteString(this ref byte @this, uint count)
        {
            var byteString = new ByteString(count);

            Unsafe.CopyBlockUnaligned(ref byteString.Ptr, ref @this, count);

            return byteString;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this ref byte source, ref byte destination, uint count)
        {
            Unsafe.CopyBlockUnaligned(ref destination, ref source, count);
        }
    }
}
