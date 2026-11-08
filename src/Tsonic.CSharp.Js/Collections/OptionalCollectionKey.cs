using System;

namespace Tsonic.CSharp.Js;

public readonly struct OptionalCollectionKey<TPayload, TStorage, TOps, TKey> : ICollectionKey<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>
    where TOps : struct, IOptionalStorage<TPayload, TStorage>
    where TKey : struct, ICollectionKey<TPayload, TKey>
{
    private readonly TStorage _value;
    private OptionalCollectionKey(TStorage value) { _value = value; }
    public static OptionalCollectionKey<TPayload, TStorage, TOps, TKey> Create(TStorage value) => new(value);
    public TStorage Value => TOps.Is2(_value) ? TOps.From2(TKey.Create(TOps.As2(_value)).Value) : TOps.From1(null);
    public bool IsNaN => TOps.Is2(_value) && TKey.Create(TOps.As2(_value)).IsNaN;
    public bool Equals(OptionalCollectionKey<TPayload, TStorage, TOps, TKey> other) => TOps.Is2(_value)
        ? TOps.Is2(other._value) && TKey.Create(TOps.As2(_value)).Equals(TKey.Create(TOps.As2(other._value)))
        : TOps.Is1(other._value);
    public override bool Equals(object? other) => other is OptionalCollectionKey<TPayload, TStorage, TOps, TKey> key && Equals(key);
    public override int GetHashCode() => TOps.Is2(_value) ? TKey.Create(TOps.As2(_value)).GetHashCode() : 0;
}
