using System;
using Vicuna.Storage.Trees.Extensions;

namespace Vicuna.Storage.Trees
{
    public ref struct ByteReader
    {
        public int Position;

        public Span<byte> Bytes;

        public ByteReader(Span<byte> bytes)
        {
            Bytes = bytes;
            Position = 0;
        }

        public Span<byte> Read(int count)
        {
            Position += count;
            return Bytes.Slice(Position - count, count);
        }

        public byte ReadByte()
        {
            return Bytes[Position++];
        }

        public short ReadInt16()
        {
            return Read(Constants.ShortSize).ToInt16();
        }

        public ushort ReadUInt16()
        {
            return Read(Constants.ShortSize).ToUInt16();
        }

        public int ReadInt()
        {
            return Read(Constants.IntSize).ToInt32();
        }

        public long ReadInt64()
        {
            return Read(Constants.LongSize).ToInt64();
        }

        public bool ReadBoolean()
        {
            return Read(Constants.BoolSize).ToBoolean();
        }
    }
}
