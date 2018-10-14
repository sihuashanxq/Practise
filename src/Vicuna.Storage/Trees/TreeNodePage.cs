using System.Collections.Generic;
using Vicuna.Storage.Tree;
using Vicuna.Storage.Trees.Extensions;

namespace Vicuna.Storage.Trees
{
    public partial class TreeNodePage : TreeNodePageInfo
    {
        /// <summary>
        /// </summary>
        public TreePageStream Stream { get; set; }

        /// <summary>
        /// 是否叶子节点
        /// </summary>
        public bool IsLeaf => NodeType == TreeNodeType.Leaf;

        /// <summary>
        /// 是否内部节点
        /// </summary>
        public bool IsBranch => NodeType == TreeNodeType.Branch;

        /// <summary>
        /// 是否溢出
        /// </summary>
        public bool IsOverflow => NodeType == TreeNodeType.Overflow;

        public int MaxCapacity => (PageSize - HeaderSizeOf) / (KeySize + ValueSize);

        public List<TreeNodePage> Pages { get; }

        public StoragePageManager StorageManager { get; }

        public TreeNodePage(StoragePageManager storageManager)
        {
            StorageManager = storageManager;
            KeySize = 10;
            ValueSize = 10;
            PageSize = 8192;
            Pages = new List<TreeNodePage>();
            NodeKeys = new List<ByteString>();
            NodeValues = new List<StoragePosition>();
        }

        public void Load(byte[] buffer)
        {
            Stream = new TreePageStream(buffer);

            NodeType = (TreeNodeType)Stream.ReadByte();
            PageId = Stream.ReadInt64();
            PrePageId = Stream.ReadInt64();
            NextPageId = Stream.ReadInt64();
            PageSize = Stream.ReadUInt16();
            FreeSize = Stream.ReadUInt16();
            KeySize = Stream.ReadUInt16();
            ValueSize = Stream.ReadUInt16();
            OverflowSize = Stream.ReadInt();
            Capacity = Stream.ReadUInt16();
            CheckSum = Stream.ReadInt();
            Resvered = Stream.Read(ResveredLength).ToByteString();

            for (var i = 0; i < Capacity; i++)
            {
                NodeKeys.Add(Stream.Read(KeySize).ToByteString());
                NodeValues.Add(new StoragePosition()
                {
                    DiskNumber = Stream.ReadInt(),
                    PageNumber = Stream.ReadUInt(),
                    PageOffset = Stream.ReadUInt16()
                });
            }
        }

        public void Flush(byte[] buffer)
        {
            Stream = new TreePageStream(buffer);

            Stream.WriteByte((byte)NodeType);
            Stream.WriteInt64(PageId);
            Stream.WriteInt64(PrePageId);
            Stream.WriteInt64(NextPageId);
            Stream.WriteUInt16(PageSize);
            Stream.WriteUInt16(PageSize);
            Stream.WriteUInt16(KeySize);
            Stream.WriteUInt16(ValueSize);
            Stream.WriteInt32(OverflowSize);
            Stream.WriteUInt16(Capacity);
            Stream.WriteInt32(CheckSum);
            Stream.Write(Resvered.Bytes, 0, ResveredLength);

            for (var i = 0; i < NodeKeys.Count; i++)
            {
                Stream.Write(NodeKeys[i].Bytes, 0, NodeKeys[i].Length);
                Stream.WriteInt32(NodeValues[i].DiskNumber);
                Stream.WriteUInt32(NodeValues[i].PageNumber);
                Stream.WriteUInt16(NodeValues[i].PageOffset);
            }
        }
    }
}
