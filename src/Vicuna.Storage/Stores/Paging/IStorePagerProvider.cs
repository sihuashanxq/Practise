namespace Vicuna.Storage.Stores.Paging
{
    public interface IStorePagerProvider
    {
        IStorePager GetPager(int storeId);
    }
}
