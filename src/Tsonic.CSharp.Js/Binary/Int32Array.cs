using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public sealed class Int32Array : TypedArray<Int32Array, int>
    {
        public Int32Array(int length) : base(length) { }
        public static Int32Array From<TSource>(IReadOnlyList<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int32Array(source.Count);
            result.set(source);
            return result;
        }
        public static Int32Array From<TSource>(IEnumerable<TSource> source)
            where TSource : INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var values = System.Linq.Enumerable.ToArray(source);
            return From<TSource>((IReadOnlyList<TSource>)values);
        }
        public static Int32Array From<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            var result = new Int32Array(source.NativeLength);
            result.set(source);
            return result;
        }
        public void set<TSourceArray, TSource>(TypedArray<TSourceArray, TSource> source, int offset = 0)
            where TSourceArray : TypedArray<TSourceArray, TSource>
            where TSource : unmanaged, INumberBase<TSource>
        {
            System.ArgumentNullException.ThrowIfNull(source);
            TypedArrayCopy.Copy<TSource, int>(source.NativeElements, NativeDestination(source.NativeLength, offset));
        }
        public Int32Array(IEnumerable<int> values) : base(values) { }
        public Int32Array(ArrayBuffer buffer, int byteOffset = 0, int? length = null) : base(buffer, byteOffset, length) { }
        protected override int ToElement<T>(T value) => unchecked((int)NativeInteger.Bits32(value));
        protected override Int32Array CreateView(ArrayBuffer buffer, int byteOffset, int length) => new(buffer, byteOffset, length);
    }
}
