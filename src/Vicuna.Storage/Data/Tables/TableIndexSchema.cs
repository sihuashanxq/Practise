namespace Vicuna.Storage.Data.Tables
{
    public class TableIndexSchema
    {
        public EncodingByteString Name { get; }

        public EncodingByteString[] Patterns { get; }

        public TableIndexSchema(EncodingByteString name, EncodingByteString[] patterns)
        {
            Name = name;
            Patterns = patterns;
        }
    }
}
