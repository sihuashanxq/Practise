using Vicuna.Storage.Paging;

namespace Vicuna.Storage
{
    public static class Constants
    {
        public const int FixedPagerIdOffset = 0 + sizeof(PageHeaderFlags);

        public const int FixedPageFlagsOffset = 0;

        public const int FixedPageNumberOffset = 0 + sizeof(PageHeaderFlags) + sizeof(int);

        public const int Kb = 1024;

        public const int PageSize = Kb * 16;

        public const int PageHeaderSize = 96;
    }
}
