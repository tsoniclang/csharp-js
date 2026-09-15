using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public static partial class JSArrayStatics
    {
        public static JSArray<T> fromDense<T>(JSArray<T> source)
        {
            return from(DenseValues(source));
        }

        public static JSArray<TResult> fromDense<T, TResult>(JSArray<T> source, Func<T, int, TResult> map)
        {
            return from(DenseValues(source), map);
        }

        public static JSArray<T?> fromOptionalValue<T>(JSArray<T?> source) where T : struct
        {
            return from(OptionalValues(source));
        }

        public static JSArray<TResult> fromOptionalValue<T, TResult>(JSArray<T?> source, Func<T?, int, TResult> map)
            where T : struct
        {
            return from(OptionalValues(source), map);
        }

        public static JSArray<T?> fromOptionalReference<T>(JSArray<T?> source) where T : class
        {
            return from(OptionalReferences(source));
        }

        public static JSArray<TResult> fromOptionalReference<T, TResult>(JSArray<T?> source, Func<T?, int, TResult> map)
            where T : class
        {
            return from(OptionalReferences(source), map);
        }

        public static JSArray<Undefined> fromUndefined(JSArray<Undefined> source)
        {
            return from(UndefinedValues(source));
        }

        public static JSArray<TResult> fromUndefined<TResult>(JSArray<Undefined> source, Func<Undefined, int, TResult> map)
        {
            return from(UndefinedValues(source), map);
        }

        private static IEnumerable<T> DenseValues<T>(JSArray<T> source)
        {
            for (var index = 0; index < source.length; index++)
            {
                if (!source.tryGetAt(index, out var value))
                {
                    throw new InvalidOperationException("checked array density invariant violated");
                }
                yield return value;
            }
        }

        private static IEnumerable<T?> OptionalValues<T>(JSArray<T?> source) where T : struct
        {
            for (var index = 0; index < source.length; index++)
            {
                yield return source.tryGetAt(index, out var value) ? value : null;
            }
        }

        private static IEnumerable<T?> OptionalReferences<T>(JSArray<T?> source) where T : class
        {
            for (var index = 0; index < source.length; index++)
            {
                yield return source.tryGetAt(index, out var value) ? value : null;
            }
        }

        private static IEnumerable<Undefined> UndefinedValues(JSArray<Undefined> source)
        {
            for (var index = 0; index < source.length; index++)
            {
                yield return Undefined.value;
            }
        }
    }
}
