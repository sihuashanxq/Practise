using System;

namespace Vicuna.Storage.Stores
{
    /// <summary>
    /// </summary>
    public interface IFileStore : IDisposable
    {
        /// <summary>
        /// </summary>
        int Id { get; }

        /// <summary>
        /// </summary>
        long Length { get; }

        /// <summary>
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// </summary>
        void Sync();

        /// <summary>
        /// </summary>
        /// <param name="length"></param>
        void SetLength(long length);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        byte[] Read(long pos, int len);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        void Read(long pos, Span<byte> data);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        void Write(long pos, byte[] data);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        void Write(long pos, Span<byte> data);

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        void Write(long pos, byte[] data, int offset, int len);
    }
}
