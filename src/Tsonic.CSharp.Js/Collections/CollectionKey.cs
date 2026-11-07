using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

public readonly struct CollectionKey<T> : ICollectionKey<T, CollectionKey<T>>
{
    private CollectionKey(T value)
    {
        Value = JSKeyEquality.canonicalizeKeyedCollectionKey(value);
    }

    public T Value { get; }

    public static CollectionKey<T> Create(T value) => new(value);

    public bool Equals(CollectionKey<T> other) => JSKeyEquality.sameValueZero(Value, other.Value);

    public override bool Equals(object? other) => other is CollectionKey<T> key && Equals(key);

    public override int GetHashCode() => JSKeyEquality.keyHash(Value);
}

// Union arm comparison (UnionCollectionKey). Arms of different Source key categories are never equal (Q77); arms of
// one category compare by SameValueZero, across numeric carriers by exact numeric value (NativeNumericKey).
internal static class CollectionKeyComparison
{
    internal static bool Equal<TLeft, CLeft, TRight, CRight>(TLeft left, TRight right)
        where CLeft : struct, ICollectionKeyCategory
        where CRight : struct, ICollectionKeyCategory
    {
        if (CLeft.Category != CRight.Category) return false;
        if (typeof(TLeft) == typeof(TRight))
            return JSKeyEquality.sameValueZero(left, Unsafe.As<TRight, TLeft>(ref right));
        if (NativeNumericKey.TryCreate(left, out var leftKey) && NativeNumericKey.TryCreate(right, out var rightKey))
            return leftKey.Equals(rightKey);
        if (!typeof(TLeft).IsValueType && !typeof(TRight).IsValueType)
            return JSKeyEquality.sameValueZero<object?>(left, right);
        return false;
    }

    internal static int Hash<T, C>(T value) where C : struct, ICollectionKeyCategory =>
        HashCode.Combine(C.Category, NativeNumericKey.TryCreate(value, out var key) ? key.GetHashCode() : JSKeyEquality.keyHash(value));
}
