using System.Collections.Generic;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Stores.Paging
{
    public interface IStorePageManager
    {
        /// <summary>
        /// </summary>
        IStorePagerProvider Provider { get; }

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
        void Free(PageNumberInfo number);

        /// <summary>
        /// </summary>
        /// <param name="pages"></param>
        void Free(IEnumerable<PageNumberInfo> pages);

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
