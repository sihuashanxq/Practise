using System;
using System.Collections.Generic;
using System.Text;

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
            fixed (byte* buffer = @this.Buffer) return (PageHeaderFlag)((PageHeader*)buffer)->Flag;
        }

        public static void SetFlag(this Page @this, byte value)
        {
            fixed (byte* buffer = @this.Buffer) ((PageHeader*)buffer)->Flag = value;
        }
    }
}
