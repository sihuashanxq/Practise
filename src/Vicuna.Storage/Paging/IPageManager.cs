using System.Collections.Generic;

namespace Vicuna.Storage.Paging
{
    public interface IPageManager
    {
        /// <summary>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        PageIdentity Allocate(int token);

        /// <summary>
        /// </summary>
        /// <param name="token"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        PageIdentity[] Allocate(int token, uint count);

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        void FreePage(PageIdentity page);

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        void FreePage(IEnumerable<PageIdentity> pages);

        /// <summary>
        /// </summary>
        /// <param name="pageIdentity"></param>
        /// <returns></returns>
        Page GetPage(PageIdentity identity);

        /// <summary>
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="data"></param>
        void SetPage(PageIdentity identity, Page page);
    }
}
