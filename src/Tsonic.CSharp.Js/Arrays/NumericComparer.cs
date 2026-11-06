using System;
using System.Collections.Generic;
using System.Numerics;

namespace Tsonic.CSharp.Js;

internal readonly struct NumericComparer<T, TResult>(Func<T, T, TResult> compare) : IComparer<T>
    where TResult : INumber<TResult>
{
    public int Compare(T? left, T? right)
    {
        var result = compare(left!, right!);
        return result < TResult.Zero ? -1 : result > TResult.Zero ? 1 : 0;
    }
}

internal readonly struct StableIndexComparer<T, TResult> : IComparer<int>
    where TResult : INumber<TResult>
{
    private readonly T[]? _values;
    private readonly string[]? _keys;
    private readonly NumericComparer<T, TResult> _comparer;

    internal StableIndexComparer(T[] values, NumericComparer<T, TResult> comparer)
    {
        _values = values;
        _keys = null;
        _comparer = comparer;
    }

    internal StableIndexComparer(string[] keys)
    {
        _values = null;
        _keys = keys;
        _comparer = default;
    }

    public int Compare(int left, int right)
    {
        var order = _keys is null
            ? _comparer.Compare(_values![left], _values![right])
            : string.CompareOrdinal(_keys[left], _keys[right]);
        return order == 0 ? left.CompareTo(right) : order;
    }
}

internal readonly struct TypedArrayElementComparer<T> : IComparer<T> where T : unmanaged, INumberBase<T>
{
    public int Compare(T left, T right) => TypedArrayNumbers.Compare(left, right);
}
