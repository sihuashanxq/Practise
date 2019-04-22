using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Data
{
    public unsafe class ByteComparer
    {
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
        public static int CompareTo(byte* p1, byte* p2, int len1, int len2)
        {
            var lp1 = p1;
            var lp2 = p2;
            var len = Math.Min(len1, len2);

            for (var i = 0; i < len / 8; i++)
            {
                if (*(long*)lp1 != *(long*)lp2)
                {
                    if (*(int*)lp1 == *(int*)lp2)
                    {
                        lp1 += 4;
                        lp2 += 4;
                    }

                    return CompareTo(lp1, lp2, sizeof(int));
                }

                lp1 += 8;
                lp2 += 8;
            }

            if ((len & 0x04) != 0)
            {
                if (*(int*)lp1 != *(int*)lp2)
                {
                    return CompareTo(lp1, lp2, sizeof(int));
                }

                lp1 += 4;
                lp2 += 4;
            }

            if ((len & 0x02) != 0)
            {
                if (*(short*)lp1 != *(short*)lp2)
                {
                    return CompareTo(lp1, lp2, sizeof(short));
                }

                lp1 += 2;
                lp2 += 2;
            }

            if ((len & 0x01) != 0)
            {
                var flag = *lp1 - *lp2;
                if (flag != 0)
                {
                    return flag;
                }
            }

            return len1 - len2;
        }

        public static int CompareTo(byte* p1, byte* p2, int len)
        {
            for (var n = 0; n < len; n++)
            {
                var flag = p1[n] - p2[n];
                if (flag != 0)
                {
                    return flag;
                }
            }

            return 0;
        }
    }
}
