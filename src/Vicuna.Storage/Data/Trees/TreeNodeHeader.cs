using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data.Trees
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public struct TreeNodeHeader
    {
        public const ushort SizeOf = 12;

        public const ushort SlotSize = sizeof(ushort);

        [FieldOffset(0)]
        public bool IsDeleted;

        [FieldOffset(1)]
        public ushort KeySize;

        [FieldOffset(3)]
        public uint DataSize;

        [FieldOffset(3)]
        public long PageNumber;

        [FieldOffset(11)]
        public TreeNodeHeaderFlags NodeFlags;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetSize()
        {
            switch (NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                    return (ushort)(SizeOf + SlotSize + KeySize + DataSize + TreeNodeTransactionHeader.SizeOf);
                case TreeNodeHeaderFlags.DataRef:
                    return (ushort)(SizeOf + SlotSize + KeySize + DataSize);
                default:
                    return (ushort)(SizeOf + SlotSize + KeySize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetValueOffset(ushort nodePosition)
        {
            switch (NodeFlags)
            {
                case TreeNodeHeaderFlags.Data:
                    return (ushort)(SizeOf + nodePosition + KeySize + TreeNodeTransactionHeader.SizeOf);
                case TreeNodeHeaderFlags.DataRef:
                    return (ushort)(SizeOf + nodePosition + KeySize);
                default:
                    return (ushort)(SizeOf + nodePosition + KeySize);
            }
        }
    }

    public enum TreeNodeHeaderFlags : byte
    {
        Data = 1,

        DataRef = 2,

        PageRef = 3
    }

    public class FixedPageNumbersTree
    {

    }
}
