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

            fixed (byte* xp = x.Chars, yp = y.Chars) return CompareTo(xp, yp, x.Chars.Length, y.Chars.Length);
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

                    for (var n = 0; n < 4; n++)
                    {
                        var flag = lxp[n] - lyp[n];
                        if (flag != 0)
                        {
                            return flag;
                        }
                    }
                }

                lxp += 8;
                lyp += 8;
            }

            if ((length & 0x04) != 0)
            {
                if (*(int*)lxp != *(int*)lyp)
                {
                    for (var n = 0; n < 4; n++)
                    {
                        var flag = lxp[n] - lyp[n];
                        if (flag != 0)
                        {
                            return flag;
                        }
                    }
                }

                lxp += 4;
                lyp += 4;
            }

            if ((length & 0x02) != 0)
            {
                if (*(short*)lxp != *(short*)lyp)
                {
                    for (var n = 0; n < 2; n++)
                    {
                        var flag = lxp[n] - lyp[n];
                        if (flag != 0)
                        {
                            return flag;
                        }
                    }
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
