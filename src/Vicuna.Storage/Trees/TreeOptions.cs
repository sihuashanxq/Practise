namespace Vicuna.Storage.Trees
{
    /// <summary>
    /// </summary>
    public class TreeOptions
    {
        public ushort KeyLength { get; set; }

        /// <summary>
        /// fixed 8byte
        /// </summary>
        public ushort ValueLength => 8;

        public uint PageSize { get; set; }
    }
}
