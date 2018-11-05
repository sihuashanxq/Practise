using System;
using System.Collections.Generic;
using System.Linq;
using Vicuna.Storage.Trees.Extensions;

namespace Vicuna.Storage.Trees
{
    public class BTreeNode<TKey> where TKey : IComparable
    {
        public bool IsLeaf { get; set; }

        public int Count { get; set; }

        public bool IsDirty { get; set; }

        public bool IsDeleted { get; set; }

        public List<TKey> Keys { get; set; }

        public List<long> Values { get; set; }

        public long NodeId { get; set; }

        public long PrevNodeId { get; set; }

        public long NextNodeId { get; set; }

        public long ParentNodeId { get; set; }

        public bool IsRoot => !HasParent;

        public bool HasNext => NextNodeId != -1;

        public bool HasPrev => PrevNodeId != -1;

        public bool HasParent => ParentNodeId != -1;

        public TKey MinKey => Keys.FirstOrDefault();

        public TKey MaxKey => Keys.LastOrDefault();

        public BTreeNode(long nodeId) : this(nodeId, 800)
        {

        }

        public BTreeNode(long nodeId, int capacity)
        {
            NodeId = nodeId;
            NextNodeId = -1;
            PrevNodeId = -1;
            ParentNodeId = -1;

            Keys = new List<TKey>();
            Values = new List<long>();
        }

        public void Load(byte[] buffer)
        {
            var Stream = new TreePageStream(buffer);

            IsLeaf = Stream.ReadBoolean();
            PrevNodeId = Stream.ReadInt64();
            NextNodeId = Stream.ReadInt64();
            ParentNodeId = Stream.ReadInt64();
            Count = Stream.ReadInt32();

            for (var i = 0; i < Count; i++)
            {
                Keys.Add((TKey)Convert.ChangeType(Stream.ReadInt32(), typeof(TKey)));
                Values.Add(Stream.ReadInt64());
            }
        }

        public void Flush(byte[] buffer)
        {
            var Stream = new TreePageStream(buffer);
            Stream.WriteByte((byte)(IsLeaf ? 1 : 0));
            Stream.WriteInt64(PrevNodeId);
            Stream.WriteInt64(NextNodeId);
            Stream.WriteInt64(ParentNodeId);
            Stream.WriteInt32(Keys.Count);

            for (var i = 0; i < Keys.Count; i++)
            {
                Stream.WriteInt32((int)Convert.ChangeType(Keys[i], typeof(int)));
                Stream.WriteInt64(Values[i]);
            }
        }
    }
}
