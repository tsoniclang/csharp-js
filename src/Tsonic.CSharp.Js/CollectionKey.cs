using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

internal readonly struct CollectionKey<T> : IEquatable<CollectionKey<T>>
{
    public CollectionKey(T value)
    {
        Value = value;
    }

    public T Value { get; }

    public bool Equals(CollectionKey<T> other) => JSKeyEquality.sameValueZero(Value, other.Value);

    public override bool Equals(object? other) => other is CollectionKey<T> key && Equals(key);

    public override int GetHashCode() => JSKeyEquality.keyHash(Value);
}
