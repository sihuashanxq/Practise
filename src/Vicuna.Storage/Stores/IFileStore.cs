using System;

namespace Vicuna.Storage.Stores
{
    public interface IFileStore : IDisposable
    {
        int Id { get; }

        string Name { get; }

        long Length { get; }

        void Sync();

        void Flush();

        void SetLength(long length);

        byte[] ReadBytes(long pos, int len);

        void ReadBytes(long pos, Span<byte> dst);

        void WriteBytes(long pos, Span<byte> src);

        void WriteBytes(long pos, byte[] src);

        void WriteBytes(long pos, byte[] src, int offset, int len);
    }
}
