using System;
using System.Text;

namespace Vicuna.Storage.Data
{
    public class ByteString : IComparable<ByteString>
    {
        public byte[] Chars { get; }

        public int Length => Chars.Length;

        public ref byte Ptr => ref Chars[0];

        public ref byte this[int index]
        {
            get => ref Chars[index];
        }

        public ByteString(int length)
        {
            Chars = new byte[length];
        }

        public ByteString(byte[] value)
        {
            Chars = value;
        }

        public ByteString(long value)
        {
            Chars = BitConverter.GetBytes(value);
        }

        public int CompareTo(ByteString other)
        {
            return ByteStringComparer.CompareTo(Chars, other.Chars);
        }

        public override string ToString()
        {
            return Chars == null ? string.Empty : Encoding.UTF8.GetString(Chars);
        }

        public long ToInt64()
        {
            if (Chars == null)
            {
                throw new NullReferenceException(nameof(Chars));
            }

            return BitConverter.ToInt64(Chars);
        }
    }
}
