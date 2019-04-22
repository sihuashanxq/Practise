using System;
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
        public static TreePage ModifyPage(this ILowLevelTransaction tx, TreePage page)
        {
            var identity = new PageIdentity(page.Header.PagerId, page.Header.PageNumber);
            var newPage = tx.ModifyPage(identity);
            if (newPage == null)
            {
                throw new InvalidOperationException($"modify page failed,{identity}");
            }

            return new TreePage(newPage.Data);
        }
    }
}
