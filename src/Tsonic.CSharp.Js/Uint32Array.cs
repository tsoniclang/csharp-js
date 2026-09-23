using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint32Array : TypedArray<Uint32Array, uint>
    {
        public Uint32Array(double length) : base(length) { }
        public Uint32Array(int length) : base(length) { }
        public static Uint32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Checked<TSource, uint>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint32Array(IEnumerable<double> values) : base(values) { }
        public Uint32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override uint ToElement(double value) => checked((uint)value);
        protected override double FromElement(uint value) => value;
        protected override Uint32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
