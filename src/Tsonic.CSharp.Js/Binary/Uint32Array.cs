using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint32Array : TypedArray<Uint32Array, uint>
    {
        public Uint32Array(int length) : base(length) { }
        public static Uint32Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint32Array(source.Count);
            result.set(source);
            return result;
        }
        public static Uint32Array From<TSource>(IEnumerable<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var values = System.Linq.Enumerable.ToArray(source);
            return From<TSource>((IReadOnlyList<TSource>)values);
        }
        public static Uint32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Uint32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, int offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, uint>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Uint32Array(IEnumerable<uint> values) : base(values) { }
        public Uint32Array(ArrayBuffer buffer, int byteOffset = 0, int? length = null) : base(buffer, byteOffset, length) { }
        protected override uint ToElement<T>(T value) => unchecked((uint)NativeInteger.Bits32(value));
        protected override Uint32Array CreateView(ArrayBuffer buffer, int byteOffset, int length) => new(buffer, byteOffset, length);
    }
}
