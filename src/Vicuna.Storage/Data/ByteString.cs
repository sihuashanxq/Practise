using System;
using System.Text;

namespace Vicuna.Storage.Data
{
    public class ByteString : IComparable<ByteString>
    {
        internal byte[] Chars { get; }

        public ref byte Ptr => ref this[0];

        public uint Size => (uint)Chars.Length;

        public ref byte this[int index] => ref Chars[index];

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
    }
}
