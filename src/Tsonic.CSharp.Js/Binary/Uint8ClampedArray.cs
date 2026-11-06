using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint8ClampedArray : TypedArray<Uint8ClampedArray, byte>
    {
        public Uint8ClampedArray(int length) : base(length) { }
        public static Uint8ClampedArray From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint8ClampedArray(source.Count);
            result.set(source);
            return result;
        }
        public static Uint8ClampedArray From<TSource>(IEnumerable<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var values = System.Linq.Enumerable.ToArray(source);
            return From<TSource>((IReadOnlyList<TSource>)values);
        }
        public static Uint8ClampedArray From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint8ClampedArray(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, int offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Clamped<TSource>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint8ClampedArray(IEnumerable<byte> values) : base(values) { }
        public Uint8ClampedArray(ArrayBuffer buffer, int byteOffset = 0, int? length = null) : base(buffer, byteOffset, length) { }
        protected override byte ToElement<T>(T value) => T.IsInteger(value)
            ? byte.CreateSaturating(value) : TypedArrayNumbers.ToUint8Clamp(double.CreateChecked(value));
        protected override Uint8ClampedArray CreateView(ArrayBuffer buffer, int byteOffset, int length) => new(buffer, byteOffset, length);
    }
}
