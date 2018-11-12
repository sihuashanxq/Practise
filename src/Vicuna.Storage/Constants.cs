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

        public const int PageHeaderSize = 64;

        public const ushort PageSize = 1024 * 16;

        public const long InitFileSize = 1024 * 1024 * 2;
    }
}
