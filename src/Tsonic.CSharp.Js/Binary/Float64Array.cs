using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Float64Array : TypedArray<Float64Array, double>
    {
        public Float64Array(int length) : base(length) { }
        public static Float64Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float64Array(source.Count);
            result.set(source);
            return result;
        }
        public static Float64Array From<TSource>(IEnumerable<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var values = System.Linq.Enumerable.ToArray(source);
            return From<TSource>((IReadOnlyList<TSource>)values);
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
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, int offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, double>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Float64Array(IEnumerable<double> values) : base(values) { }
        public Float64Array(ArrayBuffer buffer, int byteOffset = 0, int? length = null) : base(buffer, byteOffset, length) { }
        protected override double ToElement<T>(T value) => double.CreateTruncating(value);
        protected override Float64Array CreateView(ArrayBuffer buffer, int byteOffset, int length) => new(buffer, byteOffset, length);
    }
}
