using System;
using System.Collections.Generic;
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

        /// <summary>
        /// 空闲长度
        /// </summary>
        public int FreeSize => Upper - Lower;

        public TreeNodePage()
        {
            Keys = new List<ByteString>();
            Values = new List<ByteString>();
        }

        public void Load(byte[] buffer)
        {
            Stream = new TreePageStream(buffer);

            NodeType = (TreeNodeType)Stream.ReadByte();
            PageId = Stream.ReadInt64();
            PrePageId = Stream.ReadInt64();
            NextPageId = Stream.ReadInt64();
            Upper = Stream.ReadUInt16();
            Lower = Stream.ReadUInt16();
            KeySize = Stream.ReadUInt16();
            ValueSize = Stream.ReadUInt16();
            OverflowSize = Stream.ReadInt();
            Capacity = Stream.ReadUInt16();
            CheckSum = Stream.ReadInt();
            Resvered = Stream.Read(ResveredLength).ToByteString();

            for (var i = 0; i < Capacity; i++)
            {
                Keys.Add(Stream.Read(KeySize).ToByteString());
                Values.Add(Stream.Read(ValueSize).ToByteString());
            }

            if (IsBranch)
            {
                //父节点多一组指针
                Values.Add(Stream.Read(ValueSize).ToByteString());
            }
        }

        public byte[] GetNodeData()
        {
            if (Stream == null)
            {
                throw new NullReferenceException(nameof(Stream));
            }

            if (IsBranch)
            {
                return new byte[0];
            }

            return Stream.Read(Constants.PageSize - HeaderSizeOf);
        }
    }
}
