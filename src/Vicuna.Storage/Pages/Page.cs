using System;

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
        public byte[] Buffer { get; internal set; }

        public PageHeader Header { get; internal set; }

        /// <summary>
        /// 校验和
        /// </summary>
        public int CheckSum { get; internal set; }

        /// <summary>
        /// 页面Id
        /// </summary>
        public long PageId { get; internal set; }

        /// <summary>
        /// 前一个页Id
        /// </summary>
        public long PrePageId { get; internal set; }

        /// <summary>
        /// 后一个页Id
        /// </summary>
        public long NextPageId { get; internal set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public short PageSize { get; internal set; }

        /// <summary>
        /// 页空闲字节数
        /// </summary>
        public short FreeSize { get; internal set; }

        /// <summary>
        /// </summary>
        public short LastUsed { get; internal set; }

        /// <summary>
        /// </summary>
        public short ItemCount { get; internal set; }

        /// <summary>
        /// 页标志
        /// </summary>
        public PageHeaderFlag Flag { get; internal set; }

        private static unsafe PageHeader GetHeader(byte[] buffer)
        {
            fixed (byte* pointer = buffer)
            {
                return *(PageHeader*)pointer;
            }
        }
    }
}
