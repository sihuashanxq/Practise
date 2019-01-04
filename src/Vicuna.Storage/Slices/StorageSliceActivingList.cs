using System;
using System.Collections;
using System.Collections.Generic;
using Vicuna.Storage.Slices;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage
{
    public class StorageSliceActivingList : IEnumerable<StorageSliceActivingNode>
    {
        private long _headPageNumber;

        private StorageSliceActivingNode _head;

        private readonly StorageLevelTransaction _tx;

        internal StorageSliceActivingNode Head => _head ?? (_head = GetHeadNode(_headPageNumber));

        /// <summary>
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="headPageNumber"></param>
        public StorageSliceActivingList(StorageLevelTransaction tx, long headPageNumber = -1)
        {
            _tx = tx;
            _headPageNumber = headPageNumber;
        }

        public void Insert(StorageSlice slice)
        {
            if (Head.IsFull)
            {
                _head = CreateNode(Head);
                _headPageNumber = _head.PageNumber;
            }

            var sliceHeadPage = slice.SliceHeadPage;
            var sliceUsage = new StorageSliceUsage(sliceHeadPage.PageNumber, sliceHeadPage.UsedLength, (short)sliceHeadPage.FreePageCount);

            sliceHeadPage.ActivedNodeIndex = _head.Insert(sliceUsage);
            sliceHeadPage.ActivedNodePageNumber = _head.PageNumber;
        }

        public void Delete(StorageSlice slice)
        {
            if (slice == null)
            {
                throw new NullReferenceException(nameof(slice));
            }

            var sliceHeadPage = slice.SliceHeadPage;
            if (sliceHeadPage.ActivedNodePageNumber == -1)
            {
                return;
            }

            var node = GetNode(sliceHeadPage.ActivedNodePageNumber);
            if (node == null)
            {
                throw new NullReferenceException(nameof(node));
            }

            node.Delete(sliceHeadPage.ActivedNodeIndex);
        }

        public void Update(StorageSlice slice)
        {
            if (slice == null)
            {
                throw new NullReferenceException(nameof(slice));
            }

            var sliceHeadPage = slice.SliceHeadPage;
            if (sliceHeadPage.ActivedNodePageNumber == -1)
            {
                return;
            }

            var node = GetNode(slice.SliceHeadPage.ActivedNodePageNumber);
            if (node == null)
            {
                throw new NullReferenceException(nameof(node));
            }

            node.Update(new StorageSliceUsageEntry()
            {
                OwnerIndex = slice.SliceHeadPage.ActivedNodeIndex,
                OwnerPageNumber = slice.SliceHeadPage.ActivedNodePageNumber,
                Usage = new StorageSliceUsage(sliceHeadPage.PageNumber, sliceHeadPage.UsedLength, (short)sliceHeadPage.FreePageCount)
            });
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
                PrePageNumber = -1,
                NextPageNumber = -1
            };

            if (nextNode != null)
            {
                nextNode.PrePageNumber = newNode.PageNumber;
                newNode.NextPageNumber = nextNode.PageNumber;
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
                if (_current == null)
                {
                    return false;
                }

                if (_current.NextPageNumber != -1)
                {
                    var pageContent = _tx.GetPage(_current.NextPageNumber);
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

