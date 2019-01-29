using System;

namespace Vicuna.Storage.Pages
{
    [Flags]
    public enum PageFlags : byte
    {
        None = 0,

        Data = 1,

        Tree = 2,

        Overflow = 4,

        Slice = 8,

        SliceSpace = 16
    }
}
