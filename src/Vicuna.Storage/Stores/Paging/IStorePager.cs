using Vicuna.Storage.Stores;

namespace Vicuna.Storage.Stores.Paging
{
    /// <summary>
    /// </summary>
    public interface IStorePager : IStorePageAllocator
    {
        /// <summary>
        /// </summary>
        long Count { get; }

        /// <summary>
        /// </summary>
        IFileStore Store { get; }

        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        long AddPage(uint count);

        /// <summary>
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        byte[] GetPage(long pageNumber);

        /// <summary>
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="data"></param>
        void SetPage(long pageNumber, byte[] data);
    }
}
