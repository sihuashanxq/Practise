using System;
using System.IO;
using Vicuna.Storage.Trees;

namespace Vicuna.Storage.Tree
{
    public class StoragePageManager
    {
        public static long PageId;

        public Stream Stream { get; }

        public StoragePageManager(Stream stream)
        {
            Stream = stream;

            if (Stream == null)
            {
                Stream = new MemoryStream();
            }
        }

        public TreeNodePage LoadPage(StoragePosition position)
        {
            var page = new TreeNodePage(this);
            var buffer = new byte[Constants.PageSize];

            Stream.Seek(position.PageNumber * 8192, SeekOrigin.Begin);
            Stream.Read(buffer, 0, Constants.PageSize);
            page.Load(buffer);

            return page;
        }

        public TreeNodePage CreatePage()
        {
            return new TreeNodePage(this)
            {
                IsDirty = true,
                PageId = PageId++,
                PageSize = Constants.PageSize
            };
        }
    }
}
