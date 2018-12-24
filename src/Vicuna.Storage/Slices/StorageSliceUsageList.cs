using System;
using System.Collections;
using System.Collections.Generic;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public class StorageSliceUsageList : IEnumerable<StorageSliceUsageNode>
    {
        private readonly StorageLevelTransaction _tx;

        private StorageSliceUsageNode _head;

        private StorageSliceUsageNode _tail;

        public void Insert(StorageSliceSpaceUsage usage)
        {
            if (_head.IsFull)
            {
                if (!_tx.AllocatePageFromSlice(out var newNodePage))
                {
                    throw new InvalidOperationException("alloc new page failed!");
                }

                var newNode = new StorageSliceUsageNode(newNodePage);

                newNode.InitializeNodePage();
                newNode.Insert(usage);
                newNode.PrePageOffset = -1;
                newNode.NextPageOffset = _head.NextPageOffset;

                _head.PrePageOffset = _tail.PageOffset;
                _tail.NextPageOffset = _head.PageOffset;
                _head = newNode;
            }
            else
            {
                _head.Insert(usage);
            }
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

            var node = new StorageSliceUsageNode(ownerPage);

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

            var node = new StorageSliceUsageNode(ownerPage);

            node.Update(entry);
        }

        public IEnumerator<StorageSliceUsageNode> GetEnumerator()
        {
            return new StorageSliceSpaceUsageLinkedEnumerator(_tx, _head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<StorageSliceUsageNode>)this).GetEnumerator();
        }

        public class StorageSliceSpaceUsageLinkedEnumerator : IEnumerator<StorageSliceUsageNode>
        {
            private StorageLevelTransaction _tx;

            private StorageSliceUsageNode _head;

            private StorageSliceUsageNode _current;

            public StorageSliceUsageNode Current => throw new NotImplementedException();

            object IEnumerator.Current => Current;

            public StorageSliceSpaceUsageLinkedEnumerator(StorageLevelTransaction tx, StorageSliceUsageNode head)
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

                    _current = new StorageSliceUsageNode(pageContent);
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

