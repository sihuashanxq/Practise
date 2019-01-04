namespace Vicuna.Storage.Pages
{
    public static class StoragePageManagerExtensions
    {
        public static Page GetPage(this StoragePaginationManager @this,long pageNumber)
        {
            return new Page(@this.GetPageContent(pageNumber));
        }
    }
}