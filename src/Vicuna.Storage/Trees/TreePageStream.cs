using System;
using System.IO;

namespace Vicuna.Storage
{
    public class TreePageStream : MemoryStream
    {
        public TreePageStream(byte[] buffer)
            : base(buffer, true)
        {

        }

        public byte[] Read(int count)
        {
            var buffer = new byte[count];

            Read(buffer, 0, count);

            return buffer;
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(Read(Constants.ShortSize));
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(Read(Constants.ShortSize));
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(Constants.IntSize));
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(Read(Constants.IntSize));
        }

        public long ReadInt64()
        {
            return BitConverter.ToInt64(Read(Constants.LongSize));
        }

        public bool ReadBoolean()
        {
            return BitConverter.ToBoolean(Read(Constants.BoolSize));
        }

        public void WriteInt16(short value)
        {
            Write(BitConverter.GetBytes(value));
        }

        public void WriteUInt16(ushort value)
        {
            Write(BitConverter.GetBytes(value));
        }

        public void WriteInt32(int value)
        {
            Write(BitConverter.GetBytes(value));
        }

        public void WriteUInt32(ushort value)
        {
            Write(BitConverter.GetBytes(value));
        }

        public void WriteInt64(long value)
        {
            Write(BitConverter.GetBytes(value));
        }

        public void WriteUInt64(ulong value)
        {
            Write(BitConverter.GetBytes(value));
        }
    }
}
