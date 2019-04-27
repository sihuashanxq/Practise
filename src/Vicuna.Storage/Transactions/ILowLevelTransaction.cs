using Vicuna.Storage.Paging;

namespace Vicuna.Storage.Transactions
{
    public interface ILowLevelTransaction
    {
        void Commit();

        void Rollback();

        TransactionState State { get; }

        Page GetPage(PageNumberInfo number);

        Page ModifyPage(PageNumberInfo number);

        Page AllocatePage(int storeId);

        Page[] AllocatePage(int storeId, uint count);
    }
}
