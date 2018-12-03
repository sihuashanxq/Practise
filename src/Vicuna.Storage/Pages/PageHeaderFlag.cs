namespace Vicuna.Storage.Pages
{
    public enum PageHeaderFlag : byte
    {
        None = 0,

        Data = 1,

        Overflow = 2,

        Tree = 4,

        SpacePage = 8,

        SlicePage = 16,

        SegmentPage = 32
    }
}
