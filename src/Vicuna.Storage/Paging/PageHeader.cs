﻿using System.Runtime.InteropServices;

namespace Vicuna.Storage.Paging
{
    /// <summary>
    /// Page Header
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = SizeOf)]
    public unsafe struct PageHeader
    {
        public const int SizeOf = 13;

        [FieldOffset(0)]
        public PageHeaderFlags Flags;

        [FieldOffset(1)]
        public int PagerId;

        [FieldOffset(5)]
        public long PageNumber;
    }
}
