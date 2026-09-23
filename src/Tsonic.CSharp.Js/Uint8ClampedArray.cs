using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint8ClampedArray : TypedArray<Uint8ClampedArray, byte>
    {
        public Uint8ClampedArray(double length) : base(length) { }
        public Uint8ClampedArray(int length) : base(length) { }
        public static Uint8ClampedArray From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint8ClampedArray(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Clamped<TSource>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint8ClampedArray(IEnumerable<double> values) : base(values) { }
        public Uint8ClampedArray(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override byte ToElement(double value) => TypedArrayNumbers.ToUint8Clamp(value);
        protected override double FromElement(byte value) => value;
        protected override Uint8ClampedArray CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint8ClampedArray CreateCopy(IEnumerable<double> values) => new(values);
    }
}
