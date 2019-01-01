using Vicuna.Storage.Slices;

namespace Vicuna.Storage.Pages
{
    public unsafe static class PageExtensions
    {
        public static long GetPageOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->PageOffset;
        }

        public static void SetPageOffset(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->PageOffset = value;
        }

        public static long GetPrePageOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->PrePageOffset;
        }

        public static void SetPrePageOffset(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->PageOffset = value;
        }

        public static long GetNextPageOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->NextPageOffset;
        }

        public static void SetNextPageOffset(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->PageOffset = value;
        }

        public static short GetFreeLength(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->FreeSize;
        }

        public static void SetFreeLength(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->FreeSize = value;
        }

        public static short GetLastUsedOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->LastUsedOffset;
        }

        public static void SetLastUsedOffset(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->LastUsedOffset = value;
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

        public static PageHeaderFlag GetFlag(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->Flag;
        }

        public static void SetFlag(this Page @this, PageHeaderFlag value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->Flag = value;
        }

        public static short GetFreeEntryOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer) return ((PageHeader*)buffer)->FreeEntryOffset;
        }

        public static void SetFreeEntryOffset(this Page @this, short value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->FreeEntryOffset = value;
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
                return ((SlicePageHeader*)buffer)->ActivedNodeIndex;
            }
        }

        public static long GetAcitvedNodeOffset(this Page @this)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                return ((SlicePageHeader*)buffer)->ActivedNodeOffset;
            }
        }

        public static void SetActivedNodeIndex(this Page @this, int vaue)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                ((SlicePageHeader*)buffer)->ActivedNodeIndex = vaue;
            }
        }

        public static void SetActivedNodeOffset(this Page @this, long value)
        {
            fixed (byte* buffer = @this.Buffer)
            {
                ((SlicePageHeader*)buffer)->ActivedNodeOffset = value;
            }
        }
    }
}
