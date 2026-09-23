using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Js
{
    public abstract class TypedArray<TArray, TElement> : IEnumerable<double>, IArrayLike<double>
        where TArray : TypedArray<TArray, TElement>
        where TElement : unmanaged, INumberBase<TElement>
    {
        private readonly ArrayBuffer _buffer;
        private readonly int _byteOffset;

        protected TypedArray(double length)
            : this(CheckedLength(length))
        {
        }

        protected TypedArray(int length)
        {
            if (length < 0) throw new RangeError("Typed array length must be non-negative.");
            _buffer = new ArrayBuffer(checked(length * ElementSize));
            _byteOffset = 0;
            ElementCount = length;
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

        internal ReadOnlySpan<TElement> NativeElements => Elements;

        internal int NativeLength => ElementCount;

        protected Span<TElement> NativeDestination(int sourceLength, double offset)
        {
            var selectedOffset = CheckedLength(offset);
            if (selectedOffset > ElementCount || sourceLength > ElementCount - selectedOffset)
                throw new RangeError("Typed array set source exceeds the target view.");
            return Elements.Slice(selectedOffset, sourceLength);
        }

        public ArrayBuffer buffer => _buffer;

        public double byteOffset => _byteOffset;

        public double byteLength => checked(ElementCount * ElementSize);

        public double length => ElementCount;

        double IArrayLike<double>.Length => length;

        bool IArrayLike<double>.TryGet(double index, out double value)
        {
            var selected = ElementIndex(index);
            value = selected < 0 ? default : FromElement(Elements[selected]);
            return selected >= 0;
        }

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
            var selected = NativeInteger.Index(index);
            if (selected < 0)
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

            if (source is TypedArray<TArray, TElement> typed)
            {
                if (typed.ElementCount > ElementCount - selectedOffset)
                    throw new RangeError("Typed array set source exceeds the target view.");
                typed.Elements.CopyTo(Elements[selectedOffset..]);
                return;
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
            var count = range.End - range.Start;
            var result = CreateView(new ArrayBuffer(checked(count * ElementSize)), 0, count);
            Elements[range.Start..range.End].CopyTo(result.Elements);
            return result;
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

        protected ReadOnlyMemory<byte> ByteMemory => _buffer.Bytes.AsMemory(_byteOffset, checked(ElementCount * ElementSize));

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
            var integer = NativeInteger.Index(index);
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
            return NativeInteger.Length(value);
        }
    }

    internal static class TypedArrayNumbers
    {

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
