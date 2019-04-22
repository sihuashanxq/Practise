using System;
namespace Vicuna.Storage.Paging
{
    public class PagerNotFoundException : Exception
    {
        public int PagerId { get; }

        public PagerNotFoundException(int pagerId) : base($"can't find a pager,PagerId:{pagerId} ")
        {
            PagerId = pagerId;
        }
    }
}
