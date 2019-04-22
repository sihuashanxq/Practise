using System;

namespace Vicuna.Storage.Paging
{
    [Flags]
    public enum PageEntryState
    {
        Clean = 1,

        Dirty = 2,

        None = 0
    }
}
