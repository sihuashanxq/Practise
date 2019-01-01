using System;
using Vicuna.Storage.Data;

namespace Vicuna.Storage.Pages
{
    /// <summary>
    /// </summary>
    public unsafe class Page
    {
        public byte[] Buffer;

        public Page(byte[] buffer)
        {
            Buffer = buffer;
        }

        public Page(long pageOffset)
        {
            Buffer = new byte[PageSize];

            fixed (byte* buffer = Buffer)
            {
                var header = (PageHeader*)buffer;
                if (header->ModifiedCount == 0)
                {
                    header->PageOffset = pageOffset;
                    header->PrePageOffset = -1;
                    header->NextPageOffset = -1;
                    header->ItemCount = 0;
                    header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                    header->PageSize = Constants.PageSize;
                    header->Flag = PageHeaderFlag.None;
                    header->UsedLength = Constants.PageHeaderSize;
                    header->LastUsedOffset = Constants.PageHeaderSize;
                }
            }
        }

        public long PageOffset
        {
            get => this.GetPageOffset();
            set => this.SetPageOffset(value);
        }

        public long PrePageOffset
        {
            get => this.GetPrePageOffset();
            set => this.SetPrePageOffset(value);
        }

        public long NextPageOffset
        {
            get => this.GetNextPageOffset();
            set => this.SetNextPageOffset(value);
        }

        public short PageSize => Constants.PageSize;

        public short FreeSize
        {
            get => this.GetFreeLength();
            set => this.SetFreeLength(value);
        }

        public short LastUsedOffset
        {
            get => this.GetLastUsedOffset();
            set => this.SetLastUsedOffset(value);
        }

        public short ItemCount
        {
            get => this.GetItemCount();
            set => this.SetItemCount(value);
        }

        public long ModifiedCount
        {
            get => this.GetModifiedCount();
            set => this.SetModifiedCount(value);
        }

        public int UsedLength
        {
            get => this.GetUsedLength();
            set => this.SetUsedLength(value);
        }

        public short FreeEntryOffset
        {
            get => this.GetFreeEntryOffset();
            set => this.SetFreeEntryOffset(value);
        }

        public short FreeEntryLength
        {
            get => this.GetFreeEntryOffset();
            set => this.SetFreeEntryOffset(value);
        }

        public PageHeaderFlag Flag
        {
            get => this.GetFlag();
            set => this.SetFlag(value);
        }

        public Page Clone()
        {
            var newBuffer = new byte[PageSize];
            Array.Copy(Buffer, newBuffer, PageSize);
            return new Page(newBuffer);
        }

        public PageHeader GetPageHeader()
        {
            fixed (byte* buffer = Buffer)
            {
                return *(PageHeader*)buffer;
            }
        }

        public FreeDataRecordEntry GetFreeDataRecordEntry(int offset)
        {
            fixed (byte* buffer = Buffer)
            {
                return *(FreeDataRecordEntry*)&buffer[offset];
            }
        }
    }
}
