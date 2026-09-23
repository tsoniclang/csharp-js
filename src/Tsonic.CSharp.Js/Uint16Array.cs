using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint16Array : TypedArray<Uint16Array, ushort>
    {
        public Uint16Array(double length) : base(length) { }
        public Uint16Array(int length) : base(length) { }
        public static Uint16Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint16Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Checked<TSource, ushort>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint16Array(IEnumerable<double> values) : base(values) { }
        public Uint16Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override ushort ToElement(double value) => checked((ushort)value);
        protected override double FromElement(ushort value) => value;
        protected override Uint16Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint16Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
