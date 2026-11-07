using System;

namespace Tsonic.CSharp.Js;

public readonly struct OptionalCollectionKey<T, TKey> : ICollectionKey<T?, OptionalCollectionKey<T, TKey>>
    where T : struct
    where TKey : struct, ICollectionKey<T, TKey>
{
    private readonly T? _value;
    private OptionalCollectionKey(T? value) { _value = value; }
    public static OptionalCollectionKey<T, TKey> Create(T? value) => new(value);
    public T? Value => _value.HasValue ? TKey.Create(_value.Value).Value : null;

    public bool Equals(OptionalCollectionKey<T, TKey> other) => _value.HasValue == other._value.HasValue &&
        (!_value.HasValue || TKey.Create(_value.Value).Equals(TKey.Create(other._value!.Value)));
    public override bool Equals(object? other) => other is OptionalCollectionKey<T, TKey> key && Equals(key);
    public override int GetHashCode() => _value.HasValue ? TKey.Create(_value.Value).GetHashCode() : 0;
}
