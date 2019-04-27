using System.Collections.Generic;

namespace Vicuna.Storage.Paging
{
    public interface IPageManager
    {
        /// <summary>
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        PageNumberInfo Allocate(int storeId);

        /// <summary>
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        PageNumberInfo[] Allocate(int storeId, uint count);

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        void FreePage(PageNumberInfo number);

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        void FreePage(IEnumerable<PageNumberInfo> pages);

        /// <summary>
        /// </summary>
        /// <param name="pageIdentity"></param>
        /// <returns></returns>
        Page GetPage(PageNumberInfo number);

        /// <summary>
        /// </summary>
        /// <param name="number"></param>
        /// <param name="data"></param>
        void SetPage(PageNumberInfo number, Page page);
    }
}
