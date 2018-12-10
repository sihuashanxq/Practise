namespace Vicuna.Storage.Pages
{
    /// <summary>
    /// </summary>
    public unsafe class Page
    {
        public Page(byte[] buffer)
        {
            Buffer = buffer;
            Header = GetHeader(buffer);
        }

        /// <summary>
        /// 页内容
        /// </summary>
        internal byte[] Buffer;

        /// <summary>
        /// 页头
        /// </summary>
        internal PageHeader Header;

        /// <summary>
        /// 页面Id
        /// </summary>
        public long PagePos
        {
            get => Header.PagePos;
            set => Header.PagePos = value;
        }

        /// <summary>
        /// 前一个页Id
        /// </summary>
        public long PrePagePos
        {
            get => Header.PrePagePos;
            set => Header.PrePagePos = value;
        }

        /// <summary>
        /// 后一个页Id
        /// </summary>
        public long NextPagePos
        {
            get => Header.NextPagePos;
            set => Header.NextPagePos = value;
        }

        /// <summary>
        /// 页大小
        /// </summary>
        public short PageSize => Header.PageSize;

        /// <summary>
        /// 页空闲字节数
        /// </summary>
        public short FreeSize
        {
            get => Header.FreeSize;
            set => Header.FreeSize = value;
        }

        /// <summary>
        /// </summary>
        public short LastUsed
        {
            get => Header.LastUsedPos;
            set => Header.LastUsedPos = value;
        }

        /// <summary>
        /// </summary>
        public short ItemCount
        {
            get => Header.ItemCount;
            set => Header.ItemCount = value;
        }

        /// <summary>
        /// </summary>
        public long ModifiedCount
        {
            get => Header.ModifiedCount;
            set => Header.ModifiedCount = value;
        }

        /// <summary>
        /// 页标志
        /// </summary>
        public PageHeaderFlag Flag
        {
            get => (PageHeaderFlag)Header.Flag;
            set => Header.Flag = (byte)value;
        }

        public unsafe void FlushPageHeader()
        {
            fixed (byte* pointer = Buffer)
            {
                *(PageHeader*)pointer = Header;
            }
        }

        private static unsafe PageHeader GetHeader(byte[] buffer)
        {
            fixed (byte* pointer = buffer)
            {
                return *(PageHeader*)pointer;
            }
        }
    }
}
