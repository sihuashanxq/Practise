using System;
using System.Collections.Generic;
using Vicuna.Storage.Trees.Extensions;

namespace Vicuna.Storage.Trees
{
    //LeafNode
    //1byte 类型(溢出,非溢出)|KeySize|Pointer(如果时溢出,就是指向溢出节点Head的指针,否则就是当前值)
    public class TreeNodePage : TreeNodePageInfo
    {
        /// <summary>
        /// 是否已脏
        /// </summary>
        public bool IsDirty { get; internal set; }

        /// <summary>
        /// Key
        /// </summary>
        public List<ByteString> Keys { get; }

        /// <summary>
        /// Key's Value(用于表示指向下一级的指针)
        /// </summary>
        public List<long> Values { get; }

        /// <summary>
        /// Key是否溢出(多个相同Key,此时Value指向多个Key组成的链表的Head)
        /// </summary>
        public List<TreeNodeValueFlag> ValueFlags { get; }

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

        public TreeNodePage()
        {
            Values = new List<long>();
            Keys = new List<ByteString>();
        }

        public void Load(Span<byte> span)
        {
            if (span.Length < HeaderSizeOf)
            {
                throw new InvalidOperationException();
            }

            //read spec order
            var byteReader = new ByteReader(span);
          
            IsDirty = false;
            NodeType = (TreeNodeType)byteReader.ReadByte();
            PageId = byteReader.ReadInt64();
            PrePageId = byteReader.ReadInt64();
            NextPageId = byteReader.ReadInt64();
            PageSize = byteReader.ReadUInt16();
            FreeSize = byteReader.ReadUInt16();
            KeyLength = byteReader.ReadUInt16();
            ValueLength = byteReader.ReadUInt16();
            CurrentCapacity = byteReader.ReadUInt16();
            CheckSum = byteReader.ReadInt();
            Resvered = byteReader.Read(ResveredLength).ToByteString();

            for (var i = 0; i < CurrentCapacity; i++)
            {
                Keys.Add(byteReader.Read(KeyLength).ToByteString());
                Values.Add(byteReader.ReadInt64());
                ValueFlags.Add((TreeNodeValueFlag)byteReader.ReadByte());
            }

            //父节点多一组指针
            if (IsBranch)
            {
                Values.Add(byteReader.ReadInt64());
                ValueFlags.Add((TreeNodeValueFlag)byteReader.ReadByte());
            }
        }
    }
}
