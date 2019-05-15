using System;
using System.Collections;
using System.Text;

namespace Vicuna.Storage.Data
{
    public class EncodingByteString : IEnumerable, IComparable<EncodingByteString>, IComparable<byte[]>, IComparable<string>
    {
        private static readonly byte[] _empty = new byte[0];

        private string _str;

        private byte[] _values;

        private readonly Encoding _encoding;

        internal byte[] Values
        {
            get
            {
                if (_values != null)
                {
                    return _values;
                }

                if (_str == null || _str == null)
                {
                    return _values = _empty;
                }

                return _values = _encoding.GetBytes(_str);
            }
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index > Values.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return Values[index];
            }
        }

        public int Length => Values.Length;

        public EncodingByteString(string str) : this(str, Encoding.UTF8)
        {

        }

        public EncodingByteString(string str, Encoding encoding)
        {
            _str = str;
            _encoding = encoding;
        }

        public EncodingByteString(Span<byte> chars) : this(chars, Encoding.UTF8)
        {

        }

        public EncodingByteString(Span<byte> chars, Encoding encoding) : this(chars.ToArray(), encoding)
        {

        }

        private EncodingByteString(byte[] chars, Encoding encoding)
        {
            _values = chars;
            _encoding = encoding;
        }

        public ReadOnlySpan<byte> Slice(int index)
        {
            if (index < 0 || index > Values.Length)
            {
                throw new ArgumentOutOfRangeException($"size:{Values.Length},index:{index}");
            }

            return Values.AsSpan().Slice(index);
        }

        public ReadOnlySpan<byte> Slice(int index, int len)
        {
            if (index < 0 || index + len > Values.Length)
            {
                throw new ArgumentOutOfRangeException($"size:{Values.Length},index:{index},len:{len}");
            }

            return Values.AsSpan().Slice(index, len);
        }

        public EncodingByteString Substring(int index, int len)
        {
            return Slice(index, len);
        }

        public override int GetHashCode()
        {
            return ToString()?.GetHashCode() ?? base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var str = obj as EncodingByteString;
            if (str is null)
            {
                return false;
            }

            return ToString() == str.ToString();
        }

        public override string ToString()
        {
            if (_str != null)
            {
                return _str;
            }

            return _str = _encoding.GetString(Values);
        }

        public IEnumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public int CompareTo(EncodingByteString other)
        {
            return Slice(0).SequenceCompareTo(other.Slice(0));
        }

        public int CompareTo(byte[] other)
        {
            return Slice(0).SequenceCompareTo(other);
        }

        public int CompareTo(string other)
        {
            return ToString().CompareTo(other);
        }

        public static implicit operator EncodingByteString(string str)
        {
            return new EncodingByteString(str);
        }

        public static implicit operator EncodingByteString(byte[] chars)
        {
            return new EncodingByteString(chars);
        }

        public static implicit operator EncodingByteString(Span<byte> chars)
        {
            return new EncodingByteString(chars);
        }

        public static implicit operator EncodingByteString(ReadOnlySpan<byte> chars)
        {
            return new EncodingByteString(chars.ToArray());
        }

        public static EncodingByteString operator +(EncodingByteString str1, string str2)
        {
            return new EncodingByteString(str1.ToString() + str2, str1._encoding);
        }

        public static EncodingByteString operator +(EncodingByteString str, byte[] bytes)
        {
            return str + (Span<byte>)bytes;
        }

        public static EncodingByteString operator +(EncodingByteString str, Span<byte> bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return str;
            }

            if (str == null || str.Length == 0)
            {
                return new EncodingByteString(bytes);
            }

            var chars = new byte[str.Length + bytes.Length];

            str.Slice(0).CopyTo(chars);
            bytes.CopyTo(chars.AsSpan().Slice(str.Length));

            return new EncodingByteString(bytes, str._encoding);
        }

        public static EncodingByteString operator +(EncodingByteString left, EncodingByteString right)
        {
            if (left == null)
            {
                return right;
            }

            if (right == null)
            {
                return left;
            }

            return left + right._values;
        }
    }

    public ref struct ValueEncodingByteString
    {
        private static readonly byte[] _empty = new byte[0];

        private string _str;

        private Span<byte> _values;

        private readonly Encoding _encoding;

        internal Span<byte> Values
        {
            get
            {
                if (_values.Length != 0)
                {
                    return _values;
                }

                if (_str == null || _str.Length == 0)
                {
                    return _values;
                }

                return _values = _encoding.GetBytes(_str);
            }
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index > Values.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return Values[index];
            }
        }

        public int Length => Values.Length;

        public ValueEncodingByteString(string str) : this(str, Encoding.UTF8)
        {

        }

        public ValueEncodingByteString(string str, Encoding encoding)
        {
            _str = str;
            _values = _empty;
            _encoding = encoding;
        }

        public ValueEncodingByteString(Span<byte> chars) : this(chars, Encoding.UTF8)
        {

        }

        public ValueEncodingByteString(Span<byte> chars, Encoding encoding)
        {
            _str = null;
            _values = chars;
            _encoding = encoding;
        }

        private ValueEncodingByteString(byte[] chars, Encoding encoding)
        {
            _str = null;
            _values = chars;
            _encoding = encoding;
        }

        public ReadOnlySpan<byte> Slice(int index)
        {
            if (index < 0 || index > Values.Length)
            {
                throw new ArgumentOutOfRangeException($"size:{Values.Length},index:{index}");
            }

            return Values.Slice(index);
        }

        public ReadOnlySpan<byte> Slice(int index, int len)
        {
            if (index < 0 || index + len > Values.Length)
            {
                throw new ArgumentOutOfRangeException($"size:{Values.Length},index:{index},len:{len}");
            }

            return Values.Slice(index, len);
        }

        public EncodingByteString Substring(int index, int len)
        {
            return Slice(index, len);
        }

        public Span<byte>.Enumerator GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public int CompareTo(EncodingByteString other)
        {
            return _values.SequenceCompareTo(other.Values);
        }

        public int CompareTo(ValueEncodingByteString other)
        {
            return _values.SequenceCompareTo(other._values);
        }

        public int CompareTo(byte[] other)
        {
            return Slice(0).SequenceCompareTo(other);
        }

        public override string ToString()
        {
            if (_str != null)
            {
                return _str;
            }

            return _str = _encoding.GetString(_values);
        }

        public static implicit operator ValueEncodingByteString(string str)
        {
            return new ValueEncodingByteString(str);
        }

        public static implicit operator ValueEncodingByteString(byte[] chars)
        {
            return new ValueEncodingByteString(chars);
        }

        public static implicit operator ValueEncodingByteString(Span<byte> chars)
        {
            return new ValueEncodingByteString(chars);
        }

        public static ValueEncodingByteString operator +(ValueEncodingByteString str1, string str2)
        {
            return new ValueEncodingByteString(str1.ToString() + str2, str1._encoding);
        }

        public static ValueEncodingByteString operator +(ValueEncodingByteString str, byte[] bytes)
        {
            return str + (Span<byte>)bytes;
        }

        public static ValueEncodingByteString operator +(ValueEncodingByteString str1, ValueEncodingByteString str2)
        {
            return str1 + str2.Values;
        }

        public static ValueEncodingByteString operator +(ValueEncodingByteString str, Span<byte> bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return str;
            }

            Span<byte> chars = new byte[str.Length + bytes.Length];

            str.Slice(0).CopyTo(chars);
            bytes.CopyTo(chars.Slice(str.Length));

            return new ValueEncodingByteString(chars, str._encoding);
        }
    }
}
