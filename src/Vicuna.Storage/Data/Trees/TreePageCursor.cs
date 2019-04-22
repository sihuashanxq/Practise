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

    public class TreePageCursor
    {
        private int _currentIndex;

        private List<TreePageEntry> _entries;

        public TreePageEntry Root => _entries[0] ?? _entries[1];

        public TreePageEntry Current
        {
            get
            {
                if (_currentIndex > _entries.Count - 1 || _currentIndex < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return _entries[_currentIndex];
            }
            internal set
            {
                if (_currentIndex > _entries.Count - 1 || _currentIndex < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                _entries[_currentIndex] = value;
            }
        }

        public TreePageCursor()
        {
            _currentIndex = -1;
            _entries = new List<TreePageEntry>();
        }

        public TreePageCursor(IEnumerable<TreePageEntry> pages) : this()
        {
            foreach (var item in pages)
            {
                _entries.Add(item);
            }
        }

        public TreePageEntry Pop()
        {
            if (_currentIndex > _entries.Count)
            {
                return null;
            }

            var page = _entries[_currentIndex];

            _currentIndex--;

            return page;
        }

        public void Push(TreePageEntry newPage)
        {
            if (_currentIndex >= _entries.Count - 1)
            {
                _currentIndex++;
                _entries.Add(newPage);
            }
            else
            {
                _currentIndex++;
                _entries.Insert(_currentIndex, newPage);
            }
        }

        public void Update(TreePageEntry newPage)
        {
            Current = newPage;
        }

        public void Reset()
        {
            _currentIndex = _entries.Count - 1;
        }

        public IDisposable CreateScope()
        {
            return new TreePageCursorScope(this);
        }

        private struct TreePageCursorScope : IDisposable
        {
            public int Index;

            private TreePageCursor Cursor;

            public TreePageCursorScope(TreePageCursor cursor)
            {
                Index = cursor._currentIndex;
                Cursor = cursor;
            }

            public void Dispose()
            {
                Cursor._currentIndex = Index;
            }
        }
    }
}
