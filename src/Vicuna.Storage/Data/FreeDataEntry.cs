using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
    public struct FreeDataEntry
    {
        [FieldOffset(0)]
        public ushort Size;

        [FieldOffset(2)]
        public ushort Next;

        public FreeDataEntry(ushort size, ushort next)
        {
            Size = size;
            Next = next;
        }
    }
}