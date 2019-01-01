using System;
using System.Runtime.InteropServices;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Slices
{
    public unsafe class StorageSliceActivingPageEntry
    {
        public int Index { get; }

        public Page Page { get; }

        public StorageSliceActivingPageEntry(int index, Page page)
        {
            Index = index;
            Page = page;
        }

        public PageHeader GetPageHeader()
        {
            fixed (byte* pagePointer = Page.Buffer)
            {
                return *(PageHeader*)pagePointer;
            }
        }
    }

    public unsafe class PageHandle : IDisposable
    {
        private readonly Page _page;

        private readonly void* _pointer;

        private readonly GCHandle _handle;

        public byte* Pointer => (byte*)_pointer;

        public PageHeader* Header => (PageHeader*)_pointer;

        public PageHandle(Page page)
        {
            _page = page;
            _handle = GCHandle.Alloc(_page.Buffer, GCHandleType.Pinned);
            _pointer = _handle.AddrOfPinnedObject().ToPointer();
        }

        public void Dispose()
        {
            _handle.Free();
        }
    }
}
