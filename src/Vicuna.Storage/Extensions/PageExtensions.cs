using Vicuna.Storage.Data;
using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Extensions
{
    public static class PageExtensions
    {
        public static TreePage AsTree(this Page page)
        {
            return new TreePage(page.Data);
        }

        public static OverflowPage AsOverflow(this Page page)
        {
            return new OverflowPage(page.Data);
        }
    }
}
