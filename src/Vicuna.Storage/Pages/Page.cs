using System;

namespace Vicuna.Storage.Pages
{
    /// <summary>
    /// </summary>
    public class Page
    {
        public Page(Memory<byte> buffer)
        {
            Buffer = buffer;
        }

        /// <summary>
        /// 页内容
        /// </summary>
        public Memory<byte> Buffer { get; }

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
        public ushort PageSize { get; internal set; }

        /// <summary>
        /// 页空闲字节数
        /// </summary>
        public ushort FreeSize { get; internal set; }

        /// <summary>
        /// 页标志
        /// </summary>
        public PageHeaderFlag Flag { get; internal set; }
    }
}
