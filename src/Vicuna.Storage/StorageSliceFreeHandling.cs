namespace Vicuna.Storage
{
    public class StorageSliceFreeHandling
    {
        public bool Allocate(out long loc)
        {
            loc = -1;
            return false;
        }

        public void Free(long loc)
        {

        }
    }
}
