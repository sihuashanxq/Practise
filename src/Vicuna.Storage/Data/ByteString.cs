using System;
using System.Text;

namespace Vicuna.Storage.Data
{
    public class ByteString : IComparable<ByteString>
    {
        public byte[] ByteChars { get; }

        public int Length => ByteChars.Length;

        public ref byte Ptr => ref ByteChars[0];

        public ByteString(int length)
        {
            ByteChars = new byte[length];
        }

        public ByteString(byte[] byteChars)
        {
            ByteChars = byteChars;
        }

        public int CompareTo(ByteString other)
        {
            return ByteStringComparer.CompareTo(ByteChars, other.ByteChars);
        }

        public override string ToString()
        {
            return ByteChars == null ? string.Empty : Encoding.UTF8.GetString(ByteChars);
        }

        public long ToInt64()
        {
            if (ByteChars == null)
            {
                throw new NullReferenceException(nameof(ByteChars));
            }

            return BitConverter.ToInt64(ByteChars);
        }
    }
}
