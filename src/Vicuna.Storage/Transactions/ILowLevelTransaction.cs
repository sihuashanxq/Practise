using Vicuna.Storage.Transactions.Impl;
using Vicuna.Storage.Paging;
using System.Collections.Generic;

namespace Vicuna.Storage.Transactions
{
    public interface ILowLevelTransaction
    {
        void Commit();

        void Rollback();

        TransactionState State { get; }

        Page GetPage(int pagerId, long pageNumber);

        Page GetPage(PageIdentity identity);

        Page ModifyPage(PageIdentity identity);

        PageIdentity AllocatePage(int pagerId);

        List<PageIdentity> AllocatePage(int pagerId, uint count);
    }
}
