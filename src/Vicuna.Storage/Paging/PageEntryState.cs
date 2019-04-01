using System;

namespace Vicuna.Storage.Paging
{
    [Flags]
    public enum PageEntryState
    {
        Flushed = 1,

        Dirtied = 2,

        None = 0
    }
}
