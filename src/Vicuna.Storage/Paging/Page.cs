using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Paging
{
    public class Page : PageAccessor
    {
        public ref PageHeader PageHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.As<byte, PageHeader>(ref Data[0]);
        }

        public Page() : this(new byte[Constants.PageSize])
        {

        }

        public Page(byte[] data) : base(data)
        {

        }
    }
}
