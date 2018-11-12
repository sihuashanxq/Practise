using System.Threading;

namespace Vicuna.Storage.Pages
{
    public abstract class PageManager
    {
        public Pager Pager { get; set; }

        private readonly object _lockObject = new object();

        public void Lock()
        {
            Monitor.Enter(_lockObject);
        }

        public void UnLock()
        {
            Monitor.Exit(_lockObject);
        }

        public virtual Page Create()
        {
            try
            {
                Lock();

                if (Pager.MaxAllocatedPage + 1 > Pager.Count)
                {
                    ExpandPagerCapacity();
                }

                return Pager.Create();
            }

            finally
            {
                UnLock();
            }
        }

        public virtual Page GetPage(long id)
        {
            try
            {
                Lock();
                return Pager.GetPage(id);
            }
            finally
            {
                UnLock();
            }
        }

        public virtual void FreePage(Page page)
        {
            try
            {
                Lock();
                Pager.FreePage(page);
            }
            finally
            {
                UnLock();
            }
        }

        public void ExpandPagerCapacity()
        {
            Lock();
            Pager.Dispose();
            Pager = CreatePager((long)(Pager.Count * Pager.PageSize * 1.2));
            UnLock();
        }

        protected abstract Pager CreatePager(long length);
    }
}
