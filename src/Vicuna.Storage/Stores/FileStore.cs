using System;
using System.IO;

namespace Vicuna.Storage.Stores.Impl
{
    public class FileStore : IFileStore
    {
        private int _id;

        private FileStream _file;

        public int Id => _id;

        public long Length => _file.Length;

        public object SyncRoot => this;

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
            lock (SyncRoot)
            {
                _file.Flush(true);
            }
        }

        public void SetLength(long len)
        {
            lock (SyncRoot)
            {
                _file.SetLength(len);
            }
        }

        public byte[] Read(long pos, int len)
        {
            if (pos < 0 || pos + len > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            var data = new byte[len];

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Read(data);

            return data;
        }

        public void Read(long pos, Span<byte> data)
        {
            if (pos < 0 || pos + data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Read(data);
        }

        public void Write(long pos, Span<byte> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (pos + data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            _file.Seek(pos, SeekOrigin.Begin);
            _file.Write(data);
        }

        public void Write(long pos, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (pos + data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            Write(pos, data.AsSpan());
        }

        public void Write(long pos, byte[] data, int offset, int len)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (pos + data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            Write(pos, data.AsSpan().Slice(offset, len));
        }

        public void Dispose()
        {
            lock (SyncRoot)
            {
                Sync();
            }
        }
    }
}
