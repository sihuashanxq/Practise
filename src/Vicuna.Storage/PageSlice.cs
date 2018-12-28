using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class PageSlice
    {
        public Page Page { get; }

        public short Offset { get; }

        public short Length { get; }

        public PageSlice(Page page, short offset, short length)
        {
            Page = page;
            Offset = offset;
            Length = length;
        }
    }
}
