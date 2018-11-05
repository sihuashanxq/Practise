using System;

namespace Vicuna.Storage.Trees.Extensions
{
    public static class SpanExtensions
    {
        public static bool ToBoolean(this Span<byte> span)
        {
            return BitConverter.ToBoolean(span);
        }

        public static short ToInt16(this Span<byte> span)
        {
            return BitConverter.ToInt16(span);
        }

        public static ushort ToUInt16(this Span<byte> span)
        {
            return BitConverter.ToUInt16(span);
        }

        public static long ToInt64(this Span<byte> span)
        {
            return BitConverter.ToInt64(span);
        }

        public static int ToInt32(this Span<byte> span)
        {
            return BitConverter.ToInt32(span);
        }
    }
}
