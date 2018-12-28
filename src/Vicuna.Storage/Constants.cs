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

        public const int Kb = 1024;

        public const int PageHeaderSize = 64;

        public const int PageSize = Kb * 16;

        public const long InitFileSize = Kb * Kb * Kb * 10L;

        public const int SlicePageCount = Kb;

        public const int StorageSliceSize =  unchecked(SlicePageCount * PageSize * Kb);

        public const int StorageSliceDefaultUsedLength=PageSize+PageHeaderSize*(SlicePageCount-1);

        public const int StorageSliceDefaultFreeLength =StorageSliceSize-StorageSliceDefaultUsedLength;
    }
}
