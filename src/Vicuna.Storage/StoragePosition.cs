using System.Runtime.InteropServices;

namespace Vicuna.Storage
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 10)]
    public struct StoragePosition
    {
        [FieldOffset(0)]
        public int DiskNumber;

        [FieldOffset(4)]
        public uint PageNumber;

        [FieldOffset(8)]
        public ushort PageOffset;
    }
}

