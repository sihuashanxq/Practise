using System;
using Vicuna.Storage.Data;
using Vicuna.Storage.Data.Trees;
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

        Tree OpenTree(EncodingByteString name);

        Tree CreateTree(EncodingByteString name);

        void WritePageLog(Page page);
    }
}