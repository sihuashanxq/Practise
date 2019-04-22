using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    [StructLayout(LayoutKind.Explicit, Size = SizeOf)]
    public struct PageIdentity
    {
        [FieldOffset(0)]
        public int PagerId;

        [FieldOffset(4)]
        public long PageNumber;

        public const int SizeOf = sizeof(int) + sizeof(long);

        public PageIdentity(int pagerId, long pageNumber)
        {
            PagerId = pagerId;
            PageNumber = pageNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = PagerId;

            hashCode += hashCode * 31 ^ PageNumber.GetHashCode();

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (obj is PageIdentity identity)
            {
                return identity.PagerId == PagerId && identity.PageNumber == PageNumber;
            }

            return false;
        }

        public static bool operator ==(PageIdentity left, PageIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PageIdentity left, PageIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
