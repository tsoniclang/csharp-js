using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Js
{
    public abstract class TypedArray<TArray, TElement> : IEnumerable<TElement>, IArrayLike<double>
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

        public int byteOffset => _byteOffset;

        public int byteLength => checked(ElementCount * ElementSize);

        public int length => ElementCount;

        int IArrayLike<double>.Length => length;

        bool IArrayLike<double>.TryGet(double index, out double value)
        {
            var selected = ElementIndex(index);
            value = selected < 0 ? default : FromElement(Elements[selected]);
            return selected >= 0;
        }

        public int BYTES_PER_ELEMENT => ElementSize;

        public double this[double index]
        {
            get
            {
                var selected = ElementIndex(index);
                return selected < 0 ? 0 : FromElement(Elements[selected]);
            }
            set
            {
                Set(index, value);
            }
        }

        public TValue Set<TIndex, TValue>(TIndex index, TValue value)
            where TIndex : INumberBase<TIndex>
            where TValue : INumberBase<TValue>
        {
            var selected = ElementIndex(index);
            if (selected >= 0) Elements[selected] = ToElement(value);
            return value;
        }

        public TElement Get<TIndex>(TIndex index) where TIndex : INumberBase<TIndex>
        {
            var selected = ElementIndex(index);
            return selected < 0 ? default : Elements[selected];
        }

        public TResult Update<TResult, TIndex>(TIndex index, bool increment, bool prefix)
            where TResult : INumberBase<TResult>
            where TIndex : INumberBase<TIndex>
        {
            var selected = ElementIndex(index);
            var before = selected < 0 ? TResult.Zero : TResult.CreateChecked(Elements[selected]);
            var after = increment ? before + TResult.One : before - TResult.One;
            if (selected >= 0) Elements[selected] = ToElement(after);
            return prefix ? after : before;
        }

        public TElement? at(double index)
        {
            var selected = NativeInteger.Index(index);
            if (selected < 0)
            {
                selected += ElementCount;
            }
            return selected < 0 || selected >= ElementCount
                ? null
                : Elements[checked((int)selected)];
        }

        public TArray fill<TValue>(TValue value, double start = 0, double? end = null) where TValue : INumberBase<TValue>
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

        public void set<TSource>(IReadOnlyList<TSource> source, double offset = 0)
            where TSource : INumberBase<TSource>
        {
            ArgumentNullException.ThrowIfNull(source);
            var destination = NativeDestination(source.Count, offset);
            if (source is TSource[] array)
            {
                CopyValues(array, destination);
            }
            else if (source is List<TSource> list)
            {
                CopyValues(CollectionsMarshal.AsSpan(list), destination);
            }
            else
            {
                for (var index = 0; index < destination.Length; index++)
                    destination[index] = ToElement(source[index]);
            }
        }

        private void CopyValues<TSource>(ReadOnlySpan<TSource> source, Span<TElement> destination)
            where TSource : INumberBase<TSource>
        {
            for (var index = 0; index < source.Length; index++)
                destination[index] = ToElement(source[index]);
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

        public int indexOf<TValue>(TValue value) where TValue : INumberBase<TValue> => indexOf(value, 0);

        public int indexOf<TValue, TIndex>(TValue value, TIndex fromIndex)
            where TValue : INumberBase<TValue> where TIndex : INumberBase<TIndex>
        {
            if (!TypedArrayNumbers.TrySearch<TValue, TElement>(value, out var search)) return -1;
            var start = NormalizeStart(fromIndex);
            for (var index = start; index < ElementCount; index++)
            {
                if (Elements[index] == search)
                {
                    return index;
                }
            }
            return -1;
        }

        public bool includes<TValue>(TValue value) where TValue : INumberBase<TValue> => includes(value, 0);

        public bool includes<TValue, TIndex>(TValue value, TIndex fromIndex)
            where TValue : INumberBase<TValue> where TIndex : INumberBase<TIndex>
        {
            if (!TypedArrayNumbers.TrySearch<TValue, TElement>(value, out var search)) return false;
            var start = NormalizeStart(fromIndex);
            if (TElement.IsNaN(search))
            {
                for (var index = start; index < ElementCount; index++)
                    if (TElement.IsNaN(Elements[index])) return true;
                return false;
            }
            for (var index = start; index < ElementCount; index++)
            {
                if (Elements[index] == search)
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

        public TArray sort(Func<TElement, TElement, double>? compareFn = null)
        {
            var values = Elements.ToArray();
            Comparison<TElement> comparison = compareFn is null
                ? TypedArrayNumbers.Compare<TElement>
                : (left, right) =>
                {
                    var order = compareFn(left, right);
                    return double.IsNaN(order) ? 0 : System.Math.Sign(order);
                };
            System.Array.Sort(values, comparison);
            values.AsSpan().CopyTo(Elements);
            return (TArray)this;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (var index = 0; index < ElementCount; index++)
            {
                yield return Elements[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract TElement ToElement<TValue>(TValue value) where TValue : INumberBase<TValue>;

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

        private int NormalizeStart<TIndex>(TIndex index) where TIndex : INumberBase<TIndex>
        {
            var integer = TIndex.IsNaN(index) ? 0 : long.CreateSaturating(index);
            var selected = integer < 0 ? ElementCount + integer : integer;
            return checked((int)System.Math.Clamp(selected, 0, ElementCount));
        }

        private int ElementIndex<TIndex>(TIndex index) where TIndex : INumberBase<TIndex>
        {
            if (!TIndex.IsInteger(index) || TIndex.IsNegative(index) && !TIndex.IsZero(index)) return -1;
            var selected = int.CreateSaturating(index);
            return selected < ElementCount ? selected : -1;
        }

        protected static int CheckedLength(double value)
        {
            return NativeInteger.IndexLength(value);
        }
    }

    internal static class TypedArrayNumbers
    {
        public static bool TrySearch<TValue, TElement>(TValue value, out TElement result)
            where TValue : INumberBase<TValue>
            where TElement : unmanaged, INumberBase<TElement>
        {
            result = TElement.CreateSaturating(value);
            if (TValue.IsNaN(value)) return TElement.IsNaN(result);
            if (TValue.IsFinite(value) && !TElement.IsFinite(result)) return false;
            if ((typeof(TElement) == typeof(float) || typeof(TElement) == typeof(double)) &&
                (typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(byte) ||
                 typeof(TValue) == typeof(short) || typeof(TValue) == typeof(ushort) ||
                 typeof(TValue) == typeof(int) || typeof(TValue) == typeof(uint) ||
                 typeof(TValue) == typeof(long) || typeof(TValue) == typeof(ulong) ||
                 typeof(TValue) == typeof(nint) || typeof(TValue) == typeof(nuint) ||
                 typeof(TValue) == typeof(Int128) || typeof(TValue) == typeof(UInt128)))
            {
                var number = double.CreateTruncating(result);
                if (TValue.IsNegative(value))
                    return number >= -170141183460469231731687303715884105728.0 &&
                        Int128.CreateTruncating(number) == Int128.CreateTruncating(value);
                return number < 340282366920938463463374607431768211456.0 &&
                    UInt128.CreateTruncating(number) == UInt128.CreateTruncating(value);
            }
            return TValue.CreateSaturating(result) == value;
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

        public static int Compare<TElement>(TElement left, TElement right)
            where TElement : unmanaged, INumberBase<TElement>
        {
            if (TElement.IsNaN(left)) return TElement.IsNaN(right) ? 0 : 1;
            if (TElement.IsNaN(right)) return -1;
            if (TElement.IsZero(left) && TElement.IsZero(right))
            {
                return TElement.IsNegative(right).CompareTo(TElement.IsNegative(left));
            }
            return Comparer<TElement>.Default.Compare(left, right);
        }
    }
}
