using System.Collections.Generic;

namespace Vicuna.Storage.Trees
{
    public class TreeNodePageInfo
    {
        /// <summary>
        /// 是否已脏
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// 1byte
        /// </summary>
        public TreeNodeType NodeType { get; set; }

        /// <summary>
        /// 8byte
        /// </summary>
        public long PageId { get; set; }

        /// <summary>
        /// 8byte
        /// </summary>
        public long PrePageId { get; set; }

        /// <summary>
        /// 8byte
        /// </summary>
        public long NextPageId { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort PageSize { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort FreeSize { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort KeySize { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort ValueSize { get; set; }

        /// <summary>
        /// 4byte
        /// </summary>
        public int OverflowSize { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort Capacity { get; set; }

        /// <summary>
        /// 4byte
        /// </summary>
        public int CheckSum { get; set; }

        /// <summary>
        /// 21 bytes
        /// </summary>
        public ByteString Resvered { get; set; }

        /// <summary>
        /// 键
        /// </summary>
        public List<ByteString> NodeKeys { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public List<StoragePosition> NodeValues { get; set; }

        /// <summary>
        /// </summary>
        public const ushort HeaderSizeOf = 64;

        public const ushort ResveredLength = 21;
    }
}
