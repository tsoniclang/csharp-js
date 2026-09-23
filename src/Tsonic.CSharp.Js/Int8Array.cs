using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Int8Array : TypedArray<Int8Array, sbyte>
    {
        public Int8Array(double length) : base(length) { }
        public Int8Array(int length) : base(length) { }
        public static Int8Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int8Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, sbyte>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Int8Array(IEnumerable<double> values) : base(values) { }
        public Int8Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override sbyte ToElement(double value) => unchecked((sbyte)NativeInteger.Bits32(value));
        protected override double FromElement(sbyte value) => value;
        protected override Int8Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int8Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
