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

        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="headPageNumber"></param>
        public StorageSliceActivingList(StorageLevelTransaction tx, long headPageNumber = -1)
        {
            _tx = tx;
            _head = GetHeadNode(headPageNumber);
        }

        public void Insert(StorageSliceSpaceEntry entry)
        {
            if (_head.IsFull)
            {
                _head = CreateNode(_head);
            }

            entry.OwnerOffset = _head.PageOffset;
            entry.Index = _head.Insert(entry.Usage);
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

            var node = GetNode(entry.OwnerOffset);

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

            var node = GetNode(entry.OwnerOffset);

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

        private StorageSliceActivingNode GetNode(long pageNumber)
        {
            if (pageNumber < 0)
            {
                throw new IndexOutOfRangeException(nameof(pageNumber));
            }

            var nodePage = _tx.GetPageToModify(pageNumber);
            if (nodePage == null)
            {
                throw new InvalidOperationException($"load node page failed:{pageNumber}!");
            }

            return new StorageSliceActivingNode(nodePage);
        }

        private StorageSliceActivingNode GetHeadNode(long pageNumber = -1)
        {
            return pageNumber == -1 ? CreateNode(null) : GetNode(pageNumber);
        }

        private StorageSliceActivingNode CreateNode(StorageSliceActivingNode nextNode)
        {
            if (!_tx.AllocatePage(out var nodePage))
            {
                throw new InvalidOperationException("create node failed!");
            }

            var newNode = new StorageSliceActivingNode(nodePage)
            {
                PrePageOffset = -1,
                NextPageOffset = -1
            };

            if (nextNode != null)
            {
                nextNode.PrePageOffset = newNode.PageOffset;
                newNode.NextPageOffset = nextNode.PageOffset;
            }

            return newNode;
        }

        private class SpaceUsageLinkedEnumerator : IEnumerator<StorageSliceActivingNode>
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

