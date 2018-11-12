using System;
using System.IO.MemoryMappedFiles;

namespace Vicuna.Storage.Pages.MMap
{
    public class MemoryMappedFileInfo
    {
        public long Size { get; set; }

        public MemoryMappedFile MappedFile { get; set; }

        public MemoryMappedViewStream Stream { get; set; }

        public void Dispose()
        {
            Stream?.Dispose();
            MappedFile.Dispose();
            GC.SuppressFinalize(this);
        }

        ~MemoryMappedFileInfo()
        {
            Stream?.Dispose();
            MappedFile.Dispose();
        }
    }
}