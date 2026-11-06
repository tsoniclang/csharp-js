using System;
using System.Numerics;

namespace Tsonic.CSharp.Js;

public partial class JSArray<T>
{
    public JSArray<T> sort()
    {
        var values = _values.ToArray();
        var keys = new string[values.Length];
        for (var index = 0; index < values.Length; index++) keys[index] = Globals.String(values[index]);
        return SortSnapshot(values, new StableIndexComparer<T, int>(keys));
    }

    public JSArray<T> sort<TResult>(Func<T, T, TResult>? compareFunc = null) where TResult : INumber<TResult>
    {
        if (compareFunc is null) return sort();
        var values = _values.ToArray();
        return SortSnapshot(values, new StableIndexComparer<T, TResult>(values, new NumericComparer<T, TResult>(compareFunc)));
    }

    private JSArray<T> SortSnapshot<TResult>(T[] values, StableIndexComparer<T, TResult> comparer)
        where TResult : INumber<TResult>
    {
        var order = new int[values.Length];
        for (var index = 0; index < order.Length; index++) order[index] = index;
        order.AsSpan().Sort(comparer);
        for (var index = 0; index < order.Length; index++)
        {
            if (index < _values.Count) _values[index] = values[order[index]];
            else _values.Add(values[order[index]]);
        }
        return this;
    }

    public JSArray<T> toSorted()
    {
        var result = CopyValues();
        result.sort();
        return result;
    }

    public JSArray<T> toSorted<TResult>(Func<T, T, TResult>? compareFunc = null) where TResult : INumber<TResult>
    {
        var result = CopyValues();
        result.sort(compareFunc);
        return result;
    }
}
