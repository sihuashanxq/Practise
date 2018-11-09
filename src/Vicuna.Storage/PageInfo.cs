using System.Runtime.InteropServices;

namespace Vicuna.Storage
{
    /// <summary>
    /// 页面的一些描述信息
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8092)]
    public unsafe struct PageInfo
    {
        public const int ConentOffset = 64;

        /// <summary>
        /// 页面Id(8bytes)
        /// </summary>
        [FieldOffset(0)]
        public long PageId;

        /// <summary>
        /// 前一个页面Id(8bytes)
        /// </summary>
        [FieldOffset(8)]
        public long PrePageId;

        /// <summary>
        /// 后一个页面Id(8bytes)
        /// </summary>
        [FieldOffset(16)]
        public long NextPageId;

        /// <summary>
        /// 页面大小
        /// </summary>
        [FieldOffset(24)]
        public int PageSize;

        /// <summary>
        /// 页面空闲字节数
        /// </summary>
        [FieldOffset(28)]
        public short FreeSize;

        /// <summary>
        /// 校验和
        /// </summary>
        [FieldOffset(30)]
        public int CheckSum;

        /// <summary>
        /// 先保留30字节
        /// </summary>
        [FieldOffset(34)]
        public fixed byte Reserved[30];

        /// <summary>
        /// 页面内容
        /// </summary>
        [FieldOffset(64)]
        public fixed byte PageContent[8028];
    }
}
