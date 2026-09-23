using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Float32Array : TypedArray<Float32Array, float>
    {
        public Float32Array(double length) : base(length) { }
        public Float32Array(int length) : base(length) { }
        public static Float32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Float32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, float>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Float32Array(IEnumerable<double> values) : base(values) { }
        public Float32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override float ToElement<T>(T value) => float.CreateTruncating(value);
        protected override double FromElement(float value) => value;
        protected override Float32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Float32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
