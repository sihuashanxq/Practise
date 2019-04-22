using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    /// <summary>
    /// Page Header
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public unsafe struct PageMapHeader
    {
        public const int SizeOf = sizeof(PageHeaderFlags) + PageIdentity.SizeOf;

        [FieldOffset(0)]
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public PageIdentity Identity;
    }
}
