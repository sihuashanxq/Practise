using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data.Trees
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 56)]
    public struct TreeRootHeader
    {
        [FieldOffset(0)]
        public int StoreId;

        [FieldOffset(4)]
        public long PagerNumber;

        [FieldOffset(12)]
        public long NodeCount;

        [FieldOffset(20)]
        public long PageCount;

        [FieldOffset(28)]
        public long LeafCount;

        [FieldOffset(36)]
        public long BranchCount;

        [FieldOffset(44)]
        public long OverflowCount;

        [FieldOffset(52)]
        public int Depth;
    }
}
