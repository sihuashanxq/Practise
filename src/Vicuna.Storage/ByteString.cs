using System;
using System.Text;

namespace Vicuna.Storage
{
    public class ByteString
    {
        public byte[] Bytes { get; }

        public ByteString(byte[] bytes)
        {
            Bytes = bytes;
        }

        public ByteString(Span<byte> span)
        {
            Bytes = span.ToArray();
        }

        public int Length => Bytes?.Length ?? 0;

        public override string ToString()
        {
            if (Bytes == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(Bytes);
        }
    }
}
