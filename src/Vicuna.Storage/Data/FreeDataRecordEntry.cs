using System.Runtime.InteropServices;

namespace Vicuna.Storage.Data
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FreeDataRecordEntry
    {
        [FieldOffset(0)]
        public short Previous;

        [FieldOffset(2)]
        public short Next;

        [FieldOffset(4)]
        public short Size;
    }
}
