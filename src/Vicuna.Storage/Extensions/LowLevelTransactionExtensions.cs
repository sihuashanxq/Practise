using System;
using Vicuna.Storage.Data;
using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Paging;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Extensions
{
    public static class LowLevelTransactionExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static Page GetPage(this ILowLevelTransaction tx, int storeId, long pageNumber)
        {
            return tx.GetPage(new PageNumberInfo(storeId, pageNumber));
        }

        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static TreePage ModifyPage(this ILowLevelTransaction tx, TreePage page)
        {
            var newPage = tx.ModifyPage(page.StoreId, page.PageNumber);
            if (newPage == null)
            {
                throw new InvalidOperationException($"modify page failed,pagerid:{page.StoreId},pagenumber:{page.PageNumber}");
            }

            return newPage.AsTree();
        }

        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static OverflowPage ModifyPage(this ILowLevelTransaction tx, OverflowPage page)
        {
            var newPage = tx.ModifyPage(page.StoreId, page.PageNumber);
            if (newPage == null)
            {
                throw new InvalidOperationException($"modify page failed,pagerid:{page.StoreId},pagenumber:{page.PageNumber}");
            }

            return newPage.AsOverflow();
        }

        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static Page ModifyPage(this ILowLevelTransaction tx, int storeId, long pageNumber)
        {
            return tx.ModifyPage(new PageNumberInfo(storeId, pageNumber));
        }

        public static TreePage[] AllocateTrees(this ILowLevelTransaction tx, int pagerId, uint count)
        {
            var pages = tx.AllocatePage(pagerId, count);
            var treePages = new TreePage[count];

            for (var i = 0; i < treePages.Length; i++)
            {
                treePages[i] = pages[i].AsTree();
            }

            return treePages;
        }

        public static OverflowPage[] AllocateOverflows(this ILowLevelTransaction tx, int pagerId, uint count)
        {
            var pages = tx.AllocatePage(pagerId, count);
            var overflows = new OverflowPage[count];

            for (var i = 0; i < overflows.Length; i++)
            {
                if (i == 0)
                {
                    overflows[i] = pages[i].AsOverflow();
                    continue;
                }

                overflows[i] = pages[i].AsOverflow();
                overflows[i - 1].Header.NextPageNumber = overflows[i].PageNumber;
            }

            return overflows;
        }
    }
}
