using System.Runtime.InteropServices;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Data
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 96)]
    public unsafe struct OverflowPageHeader
    {
        [FieldOffset(0)]
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public int StoreId;

        [FieldOffset(5)]
        public long PageNumber;

        [FieldOffset(13)]
        public long NextPageNumber;

        [FieldOffset(21)]
        public ushort UsedLength;

        [FieldOffset(23)]
        public fixed byte Reserved[73];
    }

    public class OverflowPage : PageAccessor
    {
        public const int Capacity = Constants.PageSize - Constants.PageHeaderSize;

        public ref OverflowPageHeader Header
        {
            get => ref Read<OverflowPageHeader>(0);
        }

        public int DataSize
        {
            get => Header.UsedLength - Constants.PageHeaderSize;
        }

        public OverflowPage(byte[] data) : base(data)
        {

        }
    }
}
