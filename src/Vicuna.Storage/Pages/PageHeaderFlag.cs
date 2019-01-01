namespace Vicuna.Storage.Pages
{
    public enum PageHeaderFlag : byte
    {
        None = 0,

        Data = 1,

        Tree = 2,

        Overflow = 4,

        SliceHead = 8,

        SliceSpaceNode = 16
    }
}
