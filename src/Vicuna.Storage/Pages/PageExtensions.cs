using Vicuna.Storage.Slices;

namespace Vicuna.Storage.Pages
{
    public unsafe static class PageExtensions
    {
        public static long GetPageNumber(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->PageNumber;
        }

        public static void SetPageNumber(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->PageNumber = value;
        }

        public static long GetPrePageNumber(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->PrePageNumber;
        }

        public static void SetPrePageNumber(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->PrePageNumber = value;
        }

        public static long GetNextPageNumber(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->NextPageNumber;
        }

        public static void SetNextPageNumber(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->NextPageNumber = value;
        }

        public static short GetFreeLength(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->FreeSize;
        }

        public static void SetFreeLength(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->FreeSize = value;
        }

        public static short GetLastUsedIndex(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->LastUsedIndex;
        }

        public static void SetLastUsedIndex(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->LastUsedIndex = value;
        }

        public static short GetItemCount(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->ItemCount;
        }

        public static void SetItemCount(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->ItemCount = value;
        }

        public static long GetModifiedCount(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->ModifiedCount;
        }

        public static void SetModifiedCount(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->ModifiedCount = value;
        }

        public static int GetUsedLength(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->UsedLength;
        }

        public static void SetUsedLength(this Page @this, int value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->UsedLength = value;
        }

        public static PageFlags GetFlag(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->Flag;
        }

        public static void SetFlag(this Page @this, PageFlags value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->Flag = value;
        }

        public static short GetFreeEntryIndex(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->FreeEntryIndex;
        }

        public static void SetFreeEntryIndex(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->FreeEntryIndex = value;
        }

        public static short GetFreeEntryLength(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->FreeEntryLength;
        }

        public static void SetFreeEntryLength(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->FreeEntryLength = value;
        }

        public static int GetAcitvedNodeIndex(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                return ((SlicePageHeader*)buffer)->AcitvedNodeIndex;
            }
        }

        public static long GetAcitvedNodePageNumber(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                return ((SlicePageHeader*)buffer)->AcitvedNodePageNumber;
            }
        }

        public static void SetActivedNodeIndex(this Page @this, int vaue)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                ((SlicePageHeader*)buffer)->AcitvedNodeIndex = vaue;
            }
        }

        public static void SetActivedNodePageNumber(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                ((SlicePageHeader*)buffer)->AcitvedNodePageNumber = value;
            }
        }
    }
}
