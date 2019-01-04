using System;
using Vicuna.Storage.Pages;

namespace Vicuna.Storage
{
    public class PageSlice
    {
        public Page Page { get; }

        public short Offset { get; }

        public short Length { get; }

        public PageSlice(Page page, short offset, short length)
        {
            Page = page;
            Offset = offset;
            Length = length;
        }

        public void Write(byte[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            if (buffer.Length > Length)
            {
                throw new IndexOutOfRangeException();
            }

            Array.Copy(buffer, 0, Page.Buffer, Offset, buffer.Length);
        }
    }
}
