using System.Collections.Generic;
using System;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Static JavaScript Array helpers exposed through the global Array object.
    /// Instance methods remain on JSArray&lt;T&gt;.
    /// </summary>
    public static class JSArrayStatics
    {
        public static bool isArray(object? value)
        {
            if (value is System.Array) return true;
            return value is IDynamicArray;
        }

        public static JSArray<T> from<T>(IEnumerable<T> iterable)
        {
            return JSArray<T>.from(iterable);
        }

        public static JSArray<string> from(string source)
        {
            var chars = JSArray<string>.createWithCapacity(source.Length);
            for (var offset = 0; offset < source.Length;)
            {
                chars.push(String.ReadIteratorValue(source, ref offset));
            }

            return chars;
        }

        public static JSArray<TResult> from<TSource, TResult>(
            IEnumerable<TSource> iterable,
            System.Func<TSource, int, TResult> mapFunc
        )
        {
            return JSArray<TResult>.from(iterable, mapFunc);
        }

        public static JSArray<TResult> from<TSource, TResult>(
            IEnumerable<TSource> iterable,
            System.Func<TSource, TResult> mapFunc
        )
        {
            return JSArray<TResult>.from(iterable, mapFunc);
        }

        public static JSArray<TResult> from<TResult>(
            string source,
            System.Func<string, int, TResult> mapFunc
        )
        {
            var result = JSArray<TResult>.createWithCapacity(source.Length);
            for (var offset = 0; offset < source.Length;)
            {
                result.push(mapFunc(String.ReadIteratorValue(source, ref offset), result.length));
            }

            return result;
        }

        public static JSArray<TResult> from<TResult>(
            string source,
            System.Func<string, TResult> mapFunc
        )
        {
            var result = JSArray<TResult>.createWithCapacity(source.Length);
            for (var offset = 0; offset < source.Length;)
            {
                result.push(mapFunc(String.ReadIteratorValue(source, ref offset)));
            }

            return result;
        }

        public static JSArray<T> of<T>(params T[] items)
        {
            return JSArray<T>.of(items);
        }

        public static JSArray<T> withLength<T>(double length)
        {
            return new JSArray<T>(length);
        }
    }
}
