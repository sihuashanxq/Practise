using System;

namespace Vicuna.Storage.Pages
{
    /// <summary>
    /// </summary>
    public unsafe class Page
    {
        public Memory<byte> Memory { get; }

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
                    header->FreeSize = Constants.PageSize - Constants.PageHeaderSize;
                    header->PageSize = Constants.PageSize;
                    header->ItemCount = 0;
                    header->Flag = (byte)PageHeaderFlag.None;
                    header->UsedLength = Constants.PageHeaderSize;
                    header->LastUsedOffset = Constants.PageHeaderSize;
                }
            }
        }

        public byte[] Buffer;

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

        public short FreeEntryHeadOffset
        {
            get; set;
        }

        public short FreeEntryTailOffset
        {
            get; set;
        }

        public PageHeaderFlag Flag
        {
            get => this.GetFlag();
            set => this.SetFlag((byte)value);
        }

        public Page Clone()
        {
            var newBuffer = new byte[PageSize];
            Array.Copy(Buffer, newBuffer, PageSize);
            return new Page(newBuffer);
        }
    }
}
