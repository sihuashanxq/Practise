namespace Vicuna.Storage.Pages
{
    /// <summary>
    /// </summary>
    public class Page
    {
        public Page(byte[] buffer)
        {
            Buffer = buffer;
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

        public PageHeaderFlag Flag
        {
            get => this.GetFlag();
            set => this.SetFlag((byte)value);
        }
    }
}
