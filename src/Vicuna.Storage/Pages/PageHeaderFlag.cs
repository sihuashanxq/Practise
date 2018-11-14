namespace Vicuna.Storage.Pages
{
    public enum PageHeaderFlag : byte
    {
        Data = 1,

        Overflow = 2,

        FixedSizeTree = 4,

        VariableSizeTree = 8,

        Segment = 16,

        Free = 32,
    }
}
