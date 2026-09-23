using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Int32Array : TypedArray<Int32Array, int>
    {
        public Int32Array(double length) : base(length) { }
        public Int32Array(int length) : base(length) { }
        public static Int32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, int>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Int32Array(IEnumerable<double> values) : base(values) { }
        public Int32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override int ToElement(double value) => unchecked((int)NativeInteger.Bits32(value));
        protected override double FromElement(int value) => value;
        protected override Int32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
