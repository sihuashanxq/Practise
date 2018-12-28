namespace Vicuna.Storage.Pages
{
    public static class StoragePageManagerExtensions
    {
        public static Page GetPage(this StoragePaginationManager @this,long pageOffset)
        {
            return new Page(@this.GetPageContent(pageOffset));
        }
    }
}