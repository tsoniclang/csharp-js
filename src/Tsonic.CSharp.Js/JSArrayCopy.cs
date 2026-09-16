using System;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public static partial class JSArrayStatics
    {
        public static JSArray<T> fromDense<T>(JSArray<T> source)
        {
            var result = JSArray<T>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(source.readDenseAt(index));
            }
            return result;
        }

        public static JSArray<TResult> fromDense<T, TResult>(JSArray<T> source, Func<T, int, TResult> map)
        {
            var result = JSArray<TResult>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(map(source.readDenseAt(index), index));
            }
            return result;
        }

        public static JSArray<T> fromOptional<T>(JSArray<T> source)
        {
            var result = JSArray<T>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(source.tryGetAt(index, out var value) ? value : default!);
            }
            return result;
        }

        public static JSArray<TResult> fromOptional<T, TResult>(JSArray<T> source, Func<T, int, TResult> map)
        {
            var result = JSArray<TResult>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(map(source.tryGetAt(index, out var value) ? value : default!, index));
            }
            return result;
        }

        public static JSArray<Undefined> fromUndefined(JSArray<Undefined> source)
        {
            var result = JSArray<Undefined>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(Undefined.value);
            }
            return result;
        }

        public static JSArray<TResult> fromUndefined<TResult>(JSArray<Undefined> source, Func<Undefined, int, TResult> map)
        {
            var result = JSArray<TResult>.createWithCapacity(source.length);
            for (var index = 0; index < source.length; index++)
            {
                result.push(map(Undefined.value, index));
            }
            return result;
        }
    }

    public partial class JSArray<T>
    {
        internal T readDenseAt(int index)
        {
            var slot = _slots[index];
            if (!slot.IsPresent)
            {
                throw new InvalidOperationException("checked array density invariant violated");
            }
            return slot.Value;
        }
    }
}
