using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public struct PageNumberInfo
    {
        public const int SizeOf = sizeof(int) + sizeof(long);

        [FieldOffset(0)]
        public int StoreId;

        [FieldOffset(4)]
        public long PageNumber;

        public PageNumberInfo(int storeId, long pageNumber)
        {
            StoreId = storeId;
            PageNumber = pageNumber;
        }

        public override int GetHashCode()
        {
            var hashCode = StoreId;

            hashCode += hashCode * 31 ^ PageNumber.GetHashCode();

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (obj is PageNumberInfo number)
            {
                return number.StoreId == StoreId && number.PageNumber == PageNumber;
            }

            return false;
        }

        public static bool operator ==(PageNumberInfo left, PageNumberInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PageNumberInfo left, PageNumberInfo right)
        {
            return !left.Equals(right);
        }
    }
}
