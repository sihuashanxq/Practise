using System;
using System.Collections.Concurrent;
using Vicuna.Storage.Data.Trees;
using Vicuna.Storage.Transactions;

namespace Vicuna.Storage.Data.Tables
{
    public class Table
    {
        private readonly TableSchema _schema;

        private Tree _primaryTree;

        private readonly ConcurrentDictionary<EncodingByteString, Tree> _trees;

        public EncodingByteString Name => _schema.Name;

        public bool IsDefinedSecondaryIndex => _schema.Indexes.Count > 1;

        public Table(TableSchema schema)
        {
            _schema = schema;
            _trees = new ConcurrentDictionary<EncodingByteString, Tree>();
        }

        public Tree GetTree(EncodingByteString name, ILowLevelTransaction tx)
        {
            return _trees.GetOrAdd(name, k => tx.OpenTree(name));
        }

        public Tree GetPrimaryTree(ILowLevelTransaction tx)
        {
            if (_primaryTree == null)
            {
                _primaryTree = GetTree(_schema.Primary.Name, tx);
            }

            return _primaryTree;
        }

        public void Insert(TableRawDataEntry entry, ILowLevelTransaction tx)
        {
            if (entry.PrimaryKey == null || entry.PrimaryKey.Length == 0)
            {
                throw new ArgumentException("primary key can not be empty!");
            }

            var primaryTree = GetPrimaryTree(tx);
            if (primaryTree == null)
            {
                throw new InvalidOperationException($"load table's primary tree faield,table name:{Name}");
            }

            //if (!IsDefinedSecondaryIndex)
            //{
            //    primaryTree.AddEntry(entry, tx);
            //    return;
            //}

            ////插入聚集索引
            //primaryTree.AddEntry(entry, tx);
            //插入二级索引
            InsertSecondaryIndexEntries(entry, tx);
        }

        public void Delete(TableRawDataEntry entry, ILowLevelTransaction tx)
        {
            if (entry.PrimaryKey == null || entry.PrimaryKey.Length == 0)
            {
                throw new ArgumentException("primary key can not be empty!");
            }

            var primaryTree = GetPrimaryTree(tx);
            if (primaryTree == null)
            {
                throw new InvalidOperationException($"load table's primary tree faield,table name:{Name}");
            }
        }

        public void Update(TableRawDataEntry entry, ILowLevelTransaction tx)
        {
            if (entry.PrimaryKey == null || entry.PrimaryKey.Length == 0)
            {
                throw new ArgumentException("primary key can not be empty!");
            }

            var primaryTree = GetPrimaryTree(tx);
            if (primaryTree == null)
            {
                throw new InvalidOperationException($"load table's primary tree faield,table name:{Name}");
            }
        }

        /// <summary>
        /// 插入二级索引项
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="tx"></param>
        private void InsertSecondaryIndexEntries(TableRawDataEntry entry, ILowLevelTransaction tx)
        {
            throw new NotImplementedException();
        }
    }

    public class TableRawDataEntry
    {
        public EncodingByteString PrimaryKey;

        public EncodingByteString Document;
    }
}
