using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Int16Array : TypedArray<Int16Array, short>
    {
        public Int16Array(double length) : base(length) { }
        public Int16Array(int length) : base(length) { }
        public static Int16Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int16Array(source.Count);
            result.set(source);
            return result;
        }
        public static Int16Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int16Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, short>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Int16Array(IEnumerable<double> values) : base(values) { }
        public Int16Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override short ToElement<T>(T value) => unchecked((short)NativeInteger.Bits32(value));
        protected override double FromElement(short value) => value;
        protected override Int16Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int16Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
