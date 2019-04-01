using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    [StructLayout(LayoutKind.Explicit, Size = SizeOf)]
    public struct PageIdentity
    {
        [FieldOffset(0)]
        public int Token;

        [FieldOffset(4)]
        public long PageNumber;

        public const int SizeOf = 12;

        public static PageIdentity Empty = new PageIdentity(-1, -1);

        public PageIdentity(int token, long pageNumber)
        {
            Token = token;
            PageNumber = pageNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = Token;

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
                return identity.Token == Token && identity.PageNumber == PageNumber;
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
