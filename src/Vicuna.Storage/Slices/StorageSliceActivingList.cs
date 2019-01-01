using System;
using System.Collections;
using System.Collections.Generic;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public class StorageSliceActivingList : IEnumerable<StorageSliceActivingNode>
    {
        private readonly StorageLevelTransaction _tx;

        private StorageSliceActivingNode _head;

        private StorageSliceActivingNode _tail;

        public StorageSliceActivingList(StorageLevelTransaction tx)
        {
            _tx = tx;
        }

        public StorageSliceActivingList(StorageLevelTransaction tx, long headNodePageOffset, long tailNodePageOffset)
        {
            _tx = tx;

            if (headNodePageOffset == -1)
            {
                
            }
        }

        public StorageSliceSpaceEntry Insert(long slicePageOffset, int usedLength)
        {
            var node = null as StorageSliceActivingNode;
            var usage = new SpaceUsage(slicePageOffset, usedLength);

            if (!_head.IsFull)
            {
                return new StorageSliceSpaceEntry()
                {
                    Index = _head.Insert(usage),
                    OwnerOffset = _head.PageOffset,
                    Usage = usage
                };
            }

            if (!_tx.AllocatePage(out var newNodePage))
            {
                throw new InvalidOperationException("alloc new page failed!");
            }

            var newNode = new StorageSliceActivingNode(newNodePage)
            {
                PrePageOffset = -1,
                NextPageOffset = _head.NextPageOffset,
            };

            _head.PrePageOffset = _tail.PageOffset;
            _tail.NextPageOffset = _head.PageOffset;
            _head = newNode;

            return new StorageSliceSpaceEntry()
            {
                Index = newNode.Insert(usage),
                OwnerOffset = newNode.PageOffset,
                Usage = usage
            };
        }

        public void Delete(StorageSliceSpaceEntry entry)
        {
            if (entry == null)
            {
                throw new NullReferenceException(nameof(entry));
            }

            if (entry.Index < 0)
            {
                throw new IndexOutOfRangeException(nameof(entry.Index));
            }

            var ownerPage = _tx.GetPageToModify(entry.OwnerOffset);
            if (ownerPage == null)
            {
                throw new NullReferenceException(nameof(ownerPage));
            }

            var node = new StorageSliceActivingNode(ownerPage);

            node.Delete(entry.Index);
        }

        public void Update(StorageSliceSpaceEntry entry)
        {
            if (entry == null)
            {
                throw new NullReferenceException(nameof(entry));
            }

            if (entry.Index < 0)
            {
                throw new IndexOutOfRangeException(nameof(entry.Index));
            }

            var ownerPage = _tx.GetPageToModify(entry.OwnerOffset);
            if (ownerPage == null)
            {
                throw new NullReferenceException(nameof(ownerPage));
            }

            var node = new StorageSliceActivingNode(ownerPage);

            node.Update(entry);
        }

        public IEnumerator<StorageSliceActivingNode> GetEnumerator()
        {
            return new SpaceUsageLinkedEnumerator(_tx, _head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<StorageSliceActivingNode>)this).GetEnumerator();
        }

        public class SpaceUsageLinkedEnumerator : IEnumerator<StorageSliceActivingNode>
        {
            private StorageLevelTransaction _tx;

            private StorageSliceActivingNode _head;

            private StorageSliceActivingNode _current;

            public StorageSliceActivingNode Current => throw new NotImplementedException();

            object IEnumerator.Current => Current;

            public SpaceUsageLinkedEnumerator(StorageLevelTransaction tx, StorageSliceActivingNode head)
            {
                _tx = tx;
                _head = head;
                _current = head;
            }

            public bool MoveNext()
            {
                if (_current.NextPageOffset != -1)
                {
                    var pageContent = _tx.GetPage(_current.NextPageOffset);
                    if (pageContent == null)
                    {
                        return false;
                    }

                    _current = new StorageSliceActivingNode(pageContent);
                    return true;
                }

                _current = null;
                return false;
            }

            public void Reset()
            {
                _current = _head;
            }

            public void Dispose()
            {

            }
        }
    }
}

