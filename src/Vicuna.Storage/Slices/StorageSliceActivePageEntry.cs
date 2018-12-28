using Vicuna.Storage.Pages;

namespace Vicuna.Storage.Slices
{
    public class StorageSliceActivePageEntry
    {
        public int Index { get; set; }

        public Page Page { get; set; }

        public StorageSliceActivePageEntry(int index, Page page)
        {
            Index = index;
            Page = page;
        }

        public unsafe PageHeader GetPageHeader()
        {
            fixed (byte* pagePointer = Page.Buffer)
            {
                return *(PageHeader*)pagePointer;
            }
        }
    }
}
