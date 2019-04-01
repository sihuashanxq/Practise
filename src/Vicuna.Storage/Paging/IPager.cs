namespace Vicuna.Storage.Paging
{
    /// <summary>
    /// </summary>
    public interface IPager
    {
        /// <summary>
        /// </summary>
        int Id { get; }

        /// <summary>
        /// </summary>
        long Count { get; }

        /// <summary>
        /// </summary>
        IPageAllocator Allocator { get; }

        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        void AddPage(uint count);

        /// <summary>
        /// </summary>
        /// <param name="flushToDisk"></param>
        void Flush(bool flushToDisk);

        /// <summary>
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        byte[] GetPageData(long pageNumber);

        /// <summary>
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="data"></param>
        void SetPageData(long pageNumber, byte[] data);
    }
}
