using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Float32Array : TypedArray<Float32Array, float>
    {
        public Float32Array(int length) : base(length) { }
        public static Float32Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float32Array(source.Count);
            result.set(source);
            return result;
        }
        public static Float32Array From<TSource>(IEnumerable<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var values = System.Linq.Enumerable.ToArray(source);
            return From<TSource>((IReadOnlyList<TSource>)values);
        }
        public static Float32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, int offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, float>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Float32Array(IEnumerable<float> values) : base(values) { }
        public Float32Array(ArrayBuffer buffer, int byteOffset = 0, int? length = null) : base(buffer, byteOffset, length) { }
        protected override float ToElement<T>(T value) => float.CreateTruncating(value);
        protected override Float32Array CreateView(ArrayBuffer buffer, int byteOffset, int length) => new(buffer, byteOffset, length);
    }
}
