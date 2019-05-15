using System.Collections.Generic;
using System.Linq;

namespace Vicuna.Storage.Data.Tables
{
    public class TableSchema
    {
        static readonly EncodingByteString PrimaryName = "primary";

        public EncodingByteString Name { get; }

        public TableIndexSchema Primary => Indexes[PrimaryName];

        public Dictionary<EncodingByteString, TableIndexSchema> Indexes { get; }

        public TableSchema(EncodingByteString name, IEnumerable<TableIndexSchema> indexes)
        {
            Name = name;
            Indexes = indexes.ToDictionary(item => item.Name);
        }
    }
}
