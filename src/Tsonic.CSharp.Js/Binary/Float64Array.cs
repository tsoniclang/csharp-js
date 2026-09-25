using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Float64Array : TypedArray<Float64Array, double>
    {
        public Float64Array(double length) : base(length) { }
        public Float64Array(int length) : base(length) { }
        public static Float64Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float64Array(source.Count);
            result.set(source);
            return result;
        }
        public static Float64Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float64Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, double>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Float64Array(IEnumerable<double> values) : base(values) { }
        public Float64Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override double ToElement<T>(T value) => double.CreateTruncating(value);
        protected override double FromElement(double value) => value;
        protected override Float64Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Float64Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
