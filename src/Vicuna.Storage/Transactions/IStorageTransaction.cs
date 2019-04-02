using System.Collections.Generic;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Transactions
{
    /// <summary>
    /// </summary>
    public interface IStorageTransaction
    {
        /// <summary>
        /// </summary>
        void Commit();

        /// <summary>
        /// </summary>
        void Rollback();

        /// <summary>
        /// </summary>
        TransactionState State { get; }

        /// <summary>
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        Page GetPage(PageIdentity identity);

        /// <summary>
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        Page ModifyPage(PageIdentity identity);

        /// <summary>
        /// </summary>
        /// <param name="identity"></param>
        void FreePage(PageIdentity identity);

        /// <summary>
        /// </summary>
        /// <param name="identities"></param>
        void FreePage(IEnumerable<PageIdentity> identities);

        /// <summary>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        PageIdentity AllocatePage(int token);

        /// <summary>
        /// </summary>
        /// <param name="token"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        PageIdentity[] AllocatePage(int token, uint count);
    }
}
