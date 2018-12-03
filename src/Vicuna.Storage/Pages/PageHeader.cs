﻿using System.Runtime.InteropServices;

namespace Vicuna.Storage.Pages
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 64)]
    public unsafe struct PageHeader
    {
        [FieldOffset(0)]
        public byte Flag;

        [FieldOffset(1)]
        public long PagePos;

        [FieldOffset(9)]
        public int CheckSum;

        [FieldOffset(13)]
        public short PageSize;

        [FieldOffset(15)]
        public short FreeSize;

        [FieldOffset(17)]
        public short ItemCount;

        [FieldOffset(19)]
        public long PrePagePos;

        [FieldOffset(27)]
        public long NextPagePos;

        [FieldOffset(35)]
        public short LastUsedPos;

        [FieldOffset(37)]
        public fixed byte Reserved[27];
    }
}
