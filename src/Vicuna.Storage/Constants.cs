namespace Vicuna.Storage
{
    public static class Constants
    {
        public const long NullPageId = -1;

        public const ushort ByteSize = sizeof(byte);

        public const ushort BoolSize = sizeof(bool);

        public const ushort CharSize = sizeof(char);

        public const ushort ShortSize = sizeof(short);

        public const ushort IntSize = sizeof(int);

        public const ushort LongSize = sizeof(long);

        public const ushort FloatSize = sizeof(float);

        public const ushort DoubleSize = sizeof(double);

        public const long Kb = 1024;

        public const short PageHeaderSize = 64;

        public const short PageSize = (short)Kb * 16;

        public const long InitFileSize = Kb * Kb * 64;

        public const long StroageSlicePageCount = Kb;

        public const long StorageSliceSize = StroageSlicePageCount * PageSize * Kb;

        public const long StorageSegmentSize = StorageSliceSize * 512L;

        public const long StorageSpaceSize = StorageSegmentSize * 512L;

        public const long StorageSliceFreeSize = StorageSliceSize - PageSize - (PageSize - PageHeaderSize) * StroageSlicePageCount - 1;
    }
}
