using Vicuna.Storage.Transactions.Impl;
using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Transactions
{
    public interface IStorageTransaction
    {
        void Commit();

        void Rollback();

        TransactionState State { get; }

        PageEntry GetPage(PageIdentity identity);

        PageEntry ModifyPage(PageIdentity identity);

        PageIdentity AllocatePage(int token);

        PageIdentity[] AllocatePage(int token, uint count);
    }
}
