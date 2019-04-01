using System;
using System.Collections.Generic;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Trees
{
    public class TreePageCursor
    {
        public int Index { get; internal set; }

        public List<TreePageEntry> Pages { get; internal set; }

        public TreePageEntry Root => Pages[0] ?? Pages[1];

        public TreePageEntry Current
        {
            get
            {
                if (Index > Pages.Count - 1 || Index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return Pages[Index];
            }
            internal set
            {
                if (Index > Pages.Count - 1 || Index < 0)
                {
                    throw new IndexOutOfRangeException();
                }

                Pages[Index] = value;
            }
        }

        public TreePageCursor()
        {
            Index = -1;
            Pages = new List<TreePageEntry>();
        }

        public TreePageCursor(IEnumerable<TreePageEntry> pages) : this()
        {
            foreach (var item in pages)
            {
                Pages.Add(item);
            }
        }

        public TreePageEntry Pop()
        {
            if (Index > Pages.Count)
            {
                return null;
            }

            var page = Pages[Index];

            Index--;

            return page;
        }

        public void Push(TreePageEntry newPage)
        {
            if (Index >= Pages.Count - 1)
            {
                Index++;
                Pages.Add(newPage);
            }
            else
            {
                Index++;
                Pages.Insert(Index, newPage);
            }
        }

        public TreePageEntry Modify(IStorageTransaction tx)
        {
            var page = tx.ModifyPage(Current.Page.Header.PageNumber);

            return Current = new TreePageEntry(Current.Index, new TreePage(page));
        }

        public void Reset()
        {
            Index = Pages.Count - 1;
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
                Index = cursor.Index;
                Cursor = cursor;
            }

            public void Dispose()
            {
                Cursor.Index = Index;
            }
        }
    }
}
