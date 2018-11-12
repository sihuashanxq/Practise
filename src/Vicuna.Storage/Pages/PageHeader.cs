using System.Runtime.InteropServices;

namespace Vicuna.Storage.Pages
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 64)]
    public unsafe struct PageHeader
    {
        /// <summary>
        /// 页标志
        /// </summary>
        [FieldOffset(0)]
        public PageHeaderFlag Flag;

        /// <summary>
        /// 页面Id
        /// </summary>
        [FieldOffset(1)]
        public long PageId;

        /// <summary>
        /// 前一个页Id
        /// </summary>
        [FieldOffset(9)]
        public long PrePageId;

        /// <summary>
        /// 后一个页Id
        /// </summary>
        [FieldOffset(17)]
        public long NextPageId;

        /// <summary>
        /// 页大小
        /// </summary>
        [FieldOffset(24)]
        public ushort PageSize;

        /// <summary>
        /// 页空闲字节数
        /// </summary>
        [FieldOffset(27)]
        public ushort FreeSize;

        /// <summary>
        /// 校验和
        /// </summary>
        [FieldOffset(29)]
        public int CheckSum;

        /// <summary>
        /// 先保留30字节
        /// </summary>
        [FieldOffset(33)]
        public fixed byte Reserved[31];
    }
}
