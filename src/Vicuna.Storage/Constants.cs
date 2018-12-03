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

        public const short PageHeaderSize = 64;

        public const short PageSize = 1024 * 16;

        public const long InitFileSize = 1024 * 1024 * 2;

        public const long StorageSliceSize = PageSize * 1024;

        public const long StorageSegmentSize = StorageSliceSize * 512L;

        public const long StorageSpaceSize = StorageSegmentSize * 512L;
    }
}
