using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint8Array : TypedArray<Uint8Array, byte>
    {
        public Uint8Array(double length) : base(length) { }
        public Uint8Array(int length) : base(length) { }
        public static Uint8Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint8Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, double offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, byte>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint8Array(IEnumerable<double> values) : base(values) { }
        public Uint8Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        public System.ReadOnlyMemory<byte> AsMemory() => ByteMemory;
        protected override byte ToElement(double value) => unchecked((byte)NativeInteger.Bits32(value));
        protected override double FromElement(byte value) => value;
        protected override Uint8Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint8Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
