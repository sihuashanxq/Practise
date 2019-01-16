using System;
using System.Runtime.CompilerServices;

namespace Vicuna.Storage.Data.Trees
{
    public unsafe class TreePage
    {
        public byte[] Data;

        public ref TreePageHeader Header => ref Unsafe.As<byte, TreePageHeader>(ref Data[0]);

        public void Insert(ByteString key, ByteString value, int index)
        {
            if (index <= Header.ItemCount - 1)
            {
                MoveKeyValueEntriesLeftToRight(index);
            }

            SetKeyValue(key, value, index);
            Header.ItemCount++;
        }

        public void Update(ByteString key, ByteString value, int index)
        {
            SetKeyValue(key, value, index);
        }

        public void Remove(ByteString key, int index)
        {
            if (index < Header.ItemCount - 1)
            {
                MoveKeyValueEntriesRightToLeft(index + 1);
            }

            SetKeyValue(new ByteString(Header.KeySize), new ByteString(Header.ValueSize), Header.ItemCount - 1);
            Header.ItemCount--;
        }

        public bool Search(ByteString key, out int index)
        {
            if (Header.ItemCount == 0)
            {
                index = 0;
                return false;
            }

            var first = 0;
            var last = Header.ItemCount - 1;
            var minKey = GetKey(0);
            var maxKey = GetKey(Header.ItemCount - 1);

            //<first
            if (minKey.CompareTo(key) == 1)
            {
                index = 0;
                return false;
            }

            //>last
            if (maxKey.CompareTo(key) == -1)
            {
                index = Header.ItemCount;
                return false;
            }

            //binary search
            while (first < last)
            {
                var mid = first + (last - first) / 2;
                var midKey = GetKey(mid);
                var flag = midKey.CompareTo(key);
                if (flag == 0)
                {
                    index = Header.NodeType == TreeNodeType.Leaf ? mid : mid + 1;
                    return true;
                }

                if (flag == 1)
                {
                    last = mid;
                    continue;
                }

                first = mid + 1;
            }

            var lastKey = GetKey(last);
            if (lastKey == null)
            {
                throw new NullReferenceException($"page number:{Header.PageNumber},last index :{last},search key {key.ToString()}");
            }

            if (lastKey.CompareTo(key) == 0)
            {
                index = Header.NodeType == TreeNodeType.Leaf ? last : last + 1;
                return true;
            }

            //must be >
            index = last;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteString GetKey(int index)
        {
            var key = new ByteString(Header.KeySize);
            var keyOffset = GetKeyOffset(index);

            Unsafe.CopyBlockUnaligned(ref key.Ptr, ref Data[keyOffset], Header.KeySize);

            return key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByteString GetValue(int index)
        {
            var value = new ByteString(Header.ValueSize);
            var keyOffset = GetKeyOffset(index);

            Unsafe.CopyBlockUnaligned(ref value.Ptr, ref Data[keyOffset + Header.KeySize], Header.ValueSize);

            return value;
        }

        public (ByteString, ByteString) GetKeyValue(int index)
        {
            var key = new ByteString(Header.KeySize);
            var value = new ByteString(Header.ValueSize);
            var keyOffset = GetKeyOffset(index);

            Unsafe.CopyBlockUnaligned(ref key.Ptr, ref Data[keyOffset], Header.KeySize);
            Unsafe.CopyBlockUnaligned(ref value.Ptr, ref Data[keyOffset + Header.KeySize], Header.ValueSize);

            return (key, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetKeyValue(ByteString key, ByteString value, int keyOffset)
        {
            Unsafe.CopyBlockUnaligned(ref Data[keyOffset], ref key.Ptr, Header.KeySize);
            Unsafe.CopyBlockUnaligned(ref Data[keyOffset + Header.KeySize], ref value.Ptr, Header.ValueSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyOffset(int index)
        {
            if (index >= Header.ItemCount || index < 0)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            var offset = Constants.PageHeaderSize + index * (Header.KeySize + Header.ValueSize);
            if (offset < Constants.PageHeaderSize || offset + Header.KeySize + Header.ValueSize > Constants.PageFooterOffset)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }

            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveKeyValueEntriesLeftToRight(int index)
        {
            var offset = GetKeyOffset(index);
            var size = (Header.ItemCount - index) * (Header.KeySize + Header.ValueSize);
            var buffer = new byte[size];

            var dPtr = Unsafe.As<byte, byte>(ref Data[offset]);
            var bPtr = Unsafe.As<byte, byte>(ref buffer[0]);

            Unsafe.CopyBlockUnaligned(ref bPtr, ref dPtr, (ushort)size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dPtr, Header.KeySize + Header.ValueSize), ref bPtr, (ushort)size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveKeyValueEntriesRightToLeft(int index)
        {
            var offset = GetKeyOffset(index + 1);
            var size = (Header.ItemCount - index) * (Header.KeySize + Header.ValueSize);
            var buffer = new byte[size];

            var dPtr = Unsafe.As<byte, byte>(ref Data[offset]);
            var bPtr = Unsafe.As<byte, byte>(ref buffer[0]);

            Unsafe.CopyBlockUnaligned(ref bPtr, ref dPtr, (ushort)size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Subtract(ref dPtr, Header.KeySize + Header.ValueSize), ref bPtr, (ushort)size);
        }
    }
}
