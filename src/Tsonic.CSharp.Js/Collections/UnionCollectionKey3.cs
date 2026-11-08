using System;

namespace Tsonic.CSharp.Js;

public readonly struct UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3> : ICollectionKey<Union<TValue1, TValue2, TValue3>, UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3>>
    where TCategory1 : struct, ISourceValueCategory<TValue1>
    where TCategory2 : struct, ISourceValueCategory<TValue2>
    where TCategory3 : struct, ISourceValueCategory<TValue3>
{
    private readonly Union<TValue1, TValue2, TValue3> _value;
    private UnionCollectionKey(Union<TValue1, TValue2, TValue3> value) { _value = value; }
    public static UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3> Create(Union<TValue1, TValue2, TValue3> value) =>
        value.Is1() || value.Is2() || value.Is3() ? new(value) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");
    public Union<TValue1, TValue2, TValue3> Value =>
        _value.Is1() ? Union<TValue1, TValue2, TValue3>.From1(CollectionKeyComparison.Canonicalize<TValue1, TCategory1>(_value.As1())) :
        _value.Is2() ? Union<TValue1, TValue2, TValue3>.From2(CollectionKeyComparison.Canonicalize<TValue2, TCategory2>(_value.As2())) :
        _value.Is3() ? Union<TValue1, TValue2, TValue3>.From3(CollectionKeyComparison.Canonicalize<TValue3, TCategory3>(_value.As3())) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");

    public bool IsNaN =>
        _value.Is1() ? CollectionKeyComparison.IsNaN<TValue1, TCategory1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.IsNaN<TValue2, TCategory2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.IsNaN<TValue3, TCategory3>(_value.As3()) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");

    public bool Equals(UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3> other)
    {
        if (_value.Is1())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue1, TCategory1>(_value.As1(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue2, TCategory2>(_value.As1(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue3, TCategory3>(_value.As1(), other._value.As3());
        if (_value.Is2())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue1, TCategory1>(_value.As2(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue2, TCategory2>(_value.As2(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue3, TCategory3>(_value.As2(), other._value.As3());
        if (_value.Is3())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue1, TCategory1>(_value.As3(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue2, TCategory2>(_value.As3(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue3, TCategory3>(_value.As3(), other._value.As3());
        throw new InvalidOperationException("A collection key requires an initialized native union.");
    }

    public override bool Equals(object? other) => other is UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<TValue1, TCategory1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<TValue2, TCategory2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<TValue3, TCategory3>(_value.As3()) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");
}
