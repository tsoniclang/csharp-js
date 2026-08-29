using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Js
{
    public abstract class TypedArray<TArray, TElement> : IEnumerable<double>
        where TArray : TypedArray<TArray, TElement>
        where TElement : unmanaged
    {
        private readonly ArrayBuffer _buffer;
        private readonly int _byteOffset;

        protected TypedArray(double length)
            : this(new ArrayBuffer(checked(CheckedLength(length) * ElementSize)), 0, length)
        {
        }

        protected TypedArray(IEnumerable<double> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var elements = values.ToArray();
            _buffer = new ArrayBuffer(checked(elements.Length * ElementSize));
            _byteOffset = 0;
            ElementCount = elements.Length;
            for (var index = 0; index < elements.Length; index++)
            {
                Elements[index] = ToElement(elements[index]);
            }
        }

        protected TypedArray(ArrayBuffer buffer, double byteOffset = 0, double? length = null)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            var offset = CheckedLength(byteOffset);
            if (offset % ElementSize != 0 || offset > buffer.ByteLength)
            {
                throw new RangeError("Typed array byteOffset is outside the buffer or is not element-aligned.");
            }

            var availableBytes = buffer.ByteLength - offset;
            var selectedLength = length is null
                ? availableBytes / ElementSize
                : CheckedLength(length.Value);
            var selectedBytes = checked(selectedLength * ElementSize);
            if (selectedBytes > availableBytes || length is null && availableBytes % ElementSize != 0)
            {
                throw new RangeError("Typed array view exceeds its ArrayBuffer or ends on a partial element.");
            }

            _buffer = buffer;
            _byteOffset = offset;
            ElementCount = selectedLength;
        }

        protected static int ElementSize => Marshal.SizeOf<TElement>();

        protected int ElementCount { get; }

        public ArrayBuffer buffer => _buffer;

        public double byteOffset => _byteOffset;

        public double byteLength => checked(ElementCount * ElementSize);

        public double length => ElementCount;

        public double BYTES_PER_ELEMENT => ElementSize;

        public double this[double index]
        {
            get
            {
                var selected = ElementIndex(index);
                return selected < 0 ? 0 : FromElement(Elements[selected]);
            }
            set
            {
                var selected = ElementIndex(index);
                if (selected >= 0)
                {
                    Elements[selected] = ToElement(value);
                }
            }
        }

        public double? at(double index)
        {
            var selected = TypedArrayNumbers.IntegerOrInfinity(index);
            if (selected < 0 && selected != long.MinValue)
            {
                selected += ElementCount;
            }
            return selected < 0 || selected >= ElementCount
                ? null
                : FromElement(Elements[checked((int)selected)]);
        }

        public TArray fill(double value, double start = 0, double? end = null)
        {
            var range = NormalizeRange(start, end);
            Elements[range.Start..range.End].Fill(ToElement(value));
            return (TArray)this;
        }

        public void set(IEnumerable<double> source, double offset = 0)
        {
            ArgumentNullException.ThrowIfNull(source);
            var selectedOffset = CheckedLength(offset);
            if (selectedOffset > ElementCount)
            {
                throw new RangeError("Typed array set offset is outside the target view.");
            }

            var values = source.ToArray();
            if (values.Length > ElementCount - selectedOffset)
            {
                throw new RangeError("Typed array set source exceeds the target view.");
            }

            for (var index = 0; index < values.Length; index++)
            {
                Elements[selectedOffset + index] = ToElement(values[index]);
            }
        }

        public TArray subarray(double begin = 0, double? end = null)
        {
            var range = NormalizeRange(begin, end);
            return CreateView(
                _buffer,
                checked(_byteOffset + range.Start * ElementSize),
                range.End - range.Start);
        }

        public TArray slice(double begin = 0, double? end = null)
        {
            var range = NormalizeRange(begin, end);
            var values = new double[range.End - range.Start];
            for (var index = range.Start; index < range.End; index++)
            {
                values[index - range.Start] = FromElement(Elements[index]);
            }
            return CreateCopy(values);
        }

        public double indexOf(double value, double fromIndex = 0)
        {
            var start = NormalizeStart(fromIndex);
            for (var index = start; index < ElementCount; index++)
            {
                if (FromElement(Elements[index]) == value)
                {
                    return index;
                }
            }
            return -1;
        }

        public bool includes(double value, double fromIndex = 0)
        {
            var start = NormalizeStart(fromIndex);
            for (var index = start; index < ElementCount; index++)
            {
                var element = FromElement(Elements[index]);
                if (element.Equals(value) || double.IsNaN(element) && double.IsNaN(value))
                {
                    return true;
                }
            }
            return false;
        }

        public string join(string separator = ",") => string.Join(separator, this);

        public TArray reverse()
        {
            Elements.Reverse();
            return (TArray)this;
        }

        public TArray sort(Func<double, double, double>? compareFn = null)
        {
            var values = this.ToArray();
            Comparison<double> comparison = compareFn is null
                ? TypedArrayNumbers.Compare
                : (left, right) => System.Math.Sign(compareFn(left, right));
            System.Array.Sort(values, comparison);
            for (var index = 0; index < values.Length; index++)
            {
                Elements[index] = ToElement(values[index]);
            }
            return (TArray)this;
        }

        public IEnumerator<double> GetEnumerator()
        {
            for (var index = 0; index < ElementCount; index++)
            {
                yield return FromElement(Elements[index]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract TElement ToElement(double value);

        protected abstract double FromElement(TElement value);

        protected abstract TArray CreateView(ArrayBuffer buffer, double byteOffset, double length);

        protected abstract TArray CreateCopy(IEnumerable<double> values);

        private Span<TElement> Elements => MemoryMarshal.Cast<byte, TElement>(
            _buffer.Bytes.AsSpan(_byteOffset, checked(ElementCount * ElementSize)));

        private (int Start, int End) NormalizeRange(double start, double? end)
        {
            var selectedStart = NormalizeStart(start);
            var selectedEnd = NormalizeStart(end ?? ElementCount);
            return selectedEnd < selectedStart
                ? (selectedStart, selectedStart)
                : (selectedStart, selectedEnd);
        }

        private int NormalizeStart(double index)
        {
            var integer = TypedArrayNumbers.IntegerOrInfinity(index);
            if (integer == long.MaxValue)
            {
                return ElementCount;
            }
            if (integer == long.MinValue)
            {
                return 0;
            }
            var selected = integer < 0 ? ElementCount + integer : integer;
            return checked((int)System.Math.Clamp(selected, 0, ElementCount));
        }

        private int ElementIndex(double index)
        {
            if (!double.IsFinite(index) || System.Math.Truncate(index) != index || index < 0 || index >= ElementCount)
            {
                return -1;
            }
            return checked((int)index);
        }

        protected static int CheckedLength(double value)
        {
            if (!double.IsFinite(value))
            {
                throw new RangeError("Typed array length or offset must be finite.");
            }
            var integer = System.Math.Truncate(value);
            if (integer < 0 || integer > int.MaxValue)
            {
                throw new RangeError("Typed array length or offset is outside the supported range.");
            }
            return checked((int)integer);
        }
    }

    internal static class TypedArrayNumbers
    {
        public static long IntegerOrInfinity(double value)
        {
            if (double.IsNaN(value) || value == 0)
            {
                return 0;
            }
            if (double.IsPositiveInfinity(value) || value >= long.MaxValue)
            {
                return long.MaxValue;
            }
            if (double.IsNegativeInfinity(value) || value <= long.MinValue)
            {
                return long.MinValue;
            }
            return checked((long)System.Math.Truncate(value));
        }

        public static ulong ToUnsigned(double value, int width)
        {
            if (!double.IsFinite(value) || value == 0)
            {
                return 0;
            }
            var modulus = System.Math.Pow(2, width);
            var remainder = System.Math.Truncate(value) % modulus;
            if (remainder < 0)
            {
                remainder += modulus;
            }
            return checked((ulong)remainder);
        }

        public static long ToSigned(double value, int width)
        {
            var unsigned = ToUnsigned(value, width);
            var sign = 1UL << (width - 1);
            var modulus = 1UL << width;
            return unsigned >= sign
                ? checked((long)unsigned) - checked((long)modulus)
                : checked((long)unsigned);
        }

        public static byte ToUint8Clamp(double value)
        {
            if (double.IsNaN(value) || value <= 0)
            {
                return 0;
            }
            if (value >= 255)
            {
                return 255;
            }
            return checked((byte)System.Math.Round(value, MidpointRounding.ToEven));
        }

        public static int Compare(double left, double right)
        {
            if (double.IsNaN(left)) return double.IsNaN(right) ? 0 : 1;
            if (double.IsNaN(right)) return -1;
            if (left == 0 && right == 0)
            {
                var leftBits = BitConverter.DoubleToInt64Bits(left);
                var rightBits = BitConverter.DoubleToInt64Bits(right);
                return leftBits.CompareTo(rightBits);
            }
            return left.CompareTo(right);
        }
    }
}
