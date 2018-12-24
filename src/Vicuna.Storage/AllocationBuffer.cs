using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class AllocationBuffer
    {
        public byte[] Page { get; }

        public short Offset { get; }

        public short Length { get; }

        public AllocationBuffer(byte[] page, short offset, short length)
        {
            Page = page;
            Offset = offset;
            Length = length;
        }
    }
}
