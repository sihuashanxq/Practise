using System.IO;
using System.IO.MemoryMappedFiles;

namespace Vicuna.Storage.Pages.MMap
{
    public class MemoryMappedFilePageManager : PageManager
    {
        private readonly FileStream _fileStream;

        public MemoryMappedFilePageManager(FileStream fileStream)
        {
            _fileStream = fileStream;
            Pager = CreatePager(Constants.InitFileSize);
        }

        protected sealed override Pager CreatePager(long length)
        {
            if (length < _fileStream.Length)
            {
                length = _fileStream.Length;
            }

            _fileStream.SetLength(length);

            var mappedFile = MemoryMappedFile.CreateFromFile(_fileStream, null, length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
            var mmapedFileInfo = new MemoryMappedFileInfo()
            {
                Size = length,
                MappedFile = mappedFile,
                Stream = mappedFile.CreateViewStream(0, length, MemoryMappedFileAccess.CopyOnWrite)
            };

            return new MemoryMappedFilePager(Pager?.MaxAllocatedPage ?? 0, mmapedFileInfo);
        }
    }
}
