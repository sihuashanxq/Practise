using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    /// <summary>
    /// Common Page Header(File Page Header)
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public unsafe struct PageHeader
    {
        public const int SizeOf = sizeof(byte) + PageIdentity.SizeOf;

        [FieldOffset(0)]
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public PageIdentity Identity;
    }
}
