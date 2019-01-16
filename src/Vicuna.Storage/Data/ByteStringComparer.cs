using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Data
{
    public unsafe class ByteStringComparer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(ByteString x, ByteString y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            fixed (byte* xp = x.ByteChars, yp = y.ByteChars) return CompareTo(xp, yp, x.Length, y.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(byte[] x, byte[] y)
        {
            if (x == y)
            {
                return 0;
            }

            fixed (byte* xp = x, yp = y) return CompareTo(xp, yp, x.Length, y.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CompareTo(byte* x, byte* y, int xLength, int yLength)
        {
            var lxp = x;
            var lyp = y;
            var length = Math.Min(xLength, yLength);

            for (var i = 0; i < length / 8; i++)
            {
                if (*(long*)lxp != *(long*)lyp)
                {
                    if (*(int*)lxp == *(int*)lyp)
                    {
                        lxp += 4;
                        lyp += 4;
                    }

                    return (lxp[0] - lyp[0]) | (lxp[1] - lxp[1]) | (lxp[2] - lyp[2]) | (lxp[3] - lyp[3]);
                }

                lxp += 8;
                lyp += 8;
            }

            if ((length & 0x04) != 0)
            {
                if (*(int*)lxp != *(int*)lyp)
                {
                    return (lxp[0] - lyp[0]) | (lxp[1] - lxp[1]) | (lxp[2] - lyp[2]) | (lxp[3] - lyp[3]);
                }

                lxp += 4;
                lyp += 4;
            }

            if ((length & 0x02) != 0)
            {
                if (*(short*)lxp != *(short*)lyp)
                {
                    return (lxp[0] - lyp[0]) | (lxp[1] - lxp[1]);
                }

                lxp += 2;
                lyp += 2;
            }

            if ((length & 0x01) != 0)
            {
                var value = *lxp - *lyp;
                if (value != 0)
                {
                    return value;
                }
            }

            return xLength - yLength;
        }
    }
}
