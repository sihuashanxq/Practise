using System;
using System.IO;

namespace Vicuna.Storage.Pages
{
    public class StorageFile
    {
        private readonly FileStream _fileStream;

        public long Length => _fileStream.Length;

        public StorageFile(FileStream fileStream)
        {
            _fileStream = fileStream;
        }

        public byte[] Read(long offset, int count)
        {
            var readBytes = new byte[count];

            if (offset < _fileStream.Length)
            {
                _fileStream.Seek(offset, SeekOrigin.Begin);
                _fileStream.Read(readBytes);
            }

            return readBytes;
        }

        public void Write(long offset, byte[] bytes)
        {
            if (_fileStream.Length < offset)
            {
                _fileStream.SetLength(offset + 16 * 1024 * 1024);
            }

            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Write(bytes);
        }

        public void Flush()
        {
            _fileStream.Flush();
        }

        public void Dispose()
        {
            _fileStream.Dispose();
            GC.SuppressFinalize(this);
        }

        ~StorageFile()
        {
            _fileStream.Dispose();
        }
    }
}