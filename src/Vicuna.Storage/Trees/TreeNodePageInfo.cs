using System;

namespace Vicuna.Storage.Trees
{
    public class TreeNodePageInfo
    {
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
        public ushort KeyLength { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort ValueLength { get; set; }

        /// <summary>
        /// 2byte
        /// </summary>
        public ushort CurrentCapacity { get; set; }

        /// <summary>
        /// 41 bytes
        /// </summary>
        public ByteString Resvered { get; set; }

        /// <summary>
        /// 4byte
        /// </summary>
        public int CheckSum { get; set; }

        /// <summary>
        /// </summary>
        public const ushort HeaderSizeOf = 64;

        public const ushort ResveredLength = 41;
    }
}
