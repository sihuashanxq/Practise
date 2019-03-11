namespace Vicuna.Storage.Transactions
{
    public class StorageLevelTransaction2
    {
        public long Id { get; protected set; }

        public long AllocatePage()
        {
            return -1;
        }

        public long[] AllocatePage(int numberOfPage)
        {
            var pageNumbers = new long[numberOfPage];

            for (var i = 0; i < numberOfPage; i++)
            {
                pageNumbers[i] = AllocatePage();
            }

            return pageNumbers;
        }
    }
}
