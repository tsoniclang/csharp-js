using System;

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

    }

    public partial class JSArray<T>
    {
        internal T readDenseAt(int index)
        {
            return _values[index];
        }
    }
}
