using System;
using System.Collections.Generic;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class TreeCursor
    {
        private Memory<byte> _key;

        private ILowLevelTransaction _tx;

        public TreePageEntry Entry { get; set; }

        public TreeCursor(Memory<byte> key, ILowLevelTransaction tx, TreePageEntry entry)
        {
            _key = key;
            _tx = tx;
            Entry = entry;
        }
    }
}
