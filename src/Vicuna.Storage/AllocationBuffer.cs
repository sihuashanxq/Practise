using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class AllocationBuffer
    {
        public Page Page { get; }

        public short Offset { get; }

        public short Length { get; }

        public AllocationBuffer(Page page, short offset, short length)
        {
            Page = page;
            Offset = offset;
            Length = length;
        }
    }
}
