using System;
using System.IO;

namespace Vicuna.Storage.Trees.Extensions
{
    public static class StreamReadWriteExtensions
    {
        public static short ReadInt16(this Stream stream)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            var buffer = new byte[sizeof(short)];

            stream.Read(buffer);

            return BitConverter.ToInt16(buffer);
        }

        public static int ReadInt32(this Stream stream)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            var buffer = new byte[sizeof(int)];

            stream.Read(buffer);

            return BitConverter.ToInt32(buffer);
        }

        public static long ReadInt64(this Stream stream)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            var buffer = new byte[sizeof(long)];

            stream.Read(buffer);

            return BitConverter.ToInt64(buffer);
        }

        public static void WriteChar(this Stream stream, char value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(char));
        }

        public static void WriteBoolean(this Stream stream, bool value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(bool));
        }

        public static void WriteInt16(this Stream stream, short value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(short));
        }

        public static void WriteUInt16(this Stream stream, ushort value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(ushort));
        }

        public static void WriteInt32(this Stream stream, int value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public static void WriteUInt32(this Stream stream, uint value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(uint));
        }

        public static void WriteInt64(this Stream stream, long value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(long));
        }

        public static void WriteUInt64(this Stream stream, ulong value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(ulong));
        }

        public static void WriteSingle(this Stream stream, float value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(float));
        }

        public static void WriteDouble(this Stream stream, double value)
        {
            if (stream == null)
            {
                throw new NullReferenceException(nameof(stream));
            }

            stream.Write(BitConverter.GetBytes(value), 0, sizeof(double));
        }
    }
}
