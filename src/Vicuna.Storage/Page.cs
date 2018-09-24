namespace Vicuna.Storage
{
    /// <summary>
    /// </summary>
    public unsafe struct Page
    {
        private readonly PageInfo* _pageInfo;

        public Page(byte* pageInfo)
        {
            _pageInfo = (PageInfo*)pageInfo;
        }

        /// <summary>
        /// 页面Id
        /// </summary>
        public long PageId
        {
            get => _pageInfo->PageId;
            set => _pageInfo->PageId = value;
        }

        /// <summary>
        /// 上一页Id
        /// </summary>
        public long PrePageId
        {
            get => _pageInfo->PrePageId;
            set => _pageInfo->PrePageId = value;
        }

        /// <summary>
        /// 下一页Id 
        /// </summary>
        public long NextPageId
        {
            get => _pageInfo->NextPageId;
            set => _pageInfo->NextPageId = value;
        }

        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize
        {
            get => _pageInfo->PageSize;
            set => _pageInfo->PageSize = value;
        }

        /// <summary>
        /// 页面空闲字节数
        /// </summary>
        public short FreeSize
        {
            get => _pageInfo->FreeSize;
            set => _pageInfo->FreeSize = value;
        }

        /// <summary>
        /// 校验和
        /// </summary>
        public int CheckSum
        {
            get => _pageInfo->CheckSum;
            set => _pageInfo->CheckSum = value;
        }

        /// <summary>
        /// 页面内容指针
        /// </summary>
        public byte* PageContent => _pageInfo->PageContent + PageInfo.ConentOffset;
    }
}
