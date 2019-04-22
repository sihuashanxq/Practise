using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Extensions
{
    public static class PageExtensions
    {
        public static TreePage AsTreePage(this Page page)
        {
            return new TreePage(page.Data);
        }
    }
}
