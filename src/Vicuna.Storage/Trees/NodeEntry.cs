using System.Runtime.InteropServices;

namespace Vicuna.Storage.Trees
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct NodeEntryInfo
    {
        [FieldOffset(0)]
        public long PageId;

        [FieldOffset(16)]
        public long OverFlowPageId;

        [FieldOffset(24)]
        public fixed byte Key[16];

        [FieldOffset(40)]
        public fixed byte Value[16];
    }

    public unsafe class NodeEntry
    {
        private readonly NodeEntryInfo* _nodeEntryInfo;

        public NodeEntry(byte* data)
        {
            _nodeEntryInfo = (NodeEntryInfo*)data;
        }
    }
}
