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
            return value is IJSArray;
        }

        public static JSArray<T> from<T>(IEnumerable<T> iterable)
        {
            return JSArray<T>.from(iterable);
        }

        public static JSArray<string> from(string source)
        {
            var chars = JSArray<string>.createWithCapacity(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                chars.push(source[i].ToString());
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
            for (var i = 0; i < source.Length; i++)
            {
                result.push(mapFunc(source[i].ToString(), i));
            }

            return result;
        }

        public static JSArray<TResult> from<TResult>(
            string source,
            System.Func<string, TResult> mapFunc
        )
        {
            var result = JSArray<TResult>.createWithCapacity(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                result.push(mapFunc(source[i].ToString()));
            }

            return result;
        }

        public static JSArray<T> of<T>(params T[] items)
        {
            return JSArray<T>.of(items);
        }
    }
}
