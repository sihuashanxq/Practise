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

        public Page(long pageNumber)
        {
            Buffer = new byte[PageSize];

            fixed (byte* buffer = Buffer)
            {
                var header = (PageHeader*)buffer;
                if (header->ModifiedCount == 0)
                {
                    header->PageNumber = pageNumber;
                    header->PrePageNumber = -1;
                    header->NextPageNumber = -1;
                    header->ItemCount = 0;
                    header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                    header->PageSize = Constants.PageSize;
                    header->Flag = PageFlags.None;
                    header->UsedLength = Constants.PageHeaderSize;
                    header->LastUsedIndex = Constants.PageHeaderSize;
                }
            }
        }

        public long PageNumber
        {
            get => this.GetPageNumber();
            set => this.SetPageNumber(value);
        }

        public long PrePageNumber
        {
            get => this.GetPrePageNumber();
            set => this.SetPrePageNumber(value);
        }

        public long NextPageNumber
        {
            get => this.GetNextPageNumber();
            set => this.SetNextPageNumber(value);
        }

        public short PageSize => Constants.PageSize;

        public short FreeSize
        {
            get => this.GetFreeLength();
            set => this.SetFreeLength(value);
        }

        public short LastUsedIndex
        {
            get => this.GetLastUsedIndex();
            set => this.SetLastUsedIndex(value);
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

        public short FreeEntryIndex
        {
            get => this.GetFreeEntryIndex();
            set => this.SetFreeEntryIndex(value);
        }

        public short FreeEntryLength
        {
            get => this.GetFreeEntryIndex();
            set => this.SetFreeEntryIndex(value);
        }

        public PageFlags Flag
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
    }
}
