using System;
using System.IO;

namespace Vicuna.Storage.Stores.Impl
{
    public class FileStore : IFileStore
    {
        private FileStream _file;

        public int Id { get; } = 0;

        public string Name => _file.Name;

        public long Length => _file.Length;

        public FileStore(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"file path is empty!");
            }

            _file = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public FileStore(FileStream file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public void Sync()
        {
            _file.Flush(true);
        }

        public void Flush()
        {
            _file.Flush(false);
        }

        public void SetLength(long length)
        {
            _file.SetLength(length);
        }

        public byte[] ReadBytes(long pos, int len)
        {
            if (pos < 0 || pos + len > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            var dst = new byte[len];

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Read(dst);

            return dst;
        }

        public void ReadBytes(long pos, Span<byte> dst)
        {
            if (pos < 0 || pos + dst.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Read(dst);
        }

        public void WriteBytes(long pos, Span<byte> src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Write(src);
        }

        public void WriteBytes(long pos, byte[] src)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            WriteBytes(pos, src.AsSpan());
        }

        public void WriteBytes(long pos, byte[] src, int offset, int len)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            WriteBytes(pos, src.AsSpan().Slice(offset, len));
        }

        public void Dispose()
        {
            Sync();
        }
    }
}
