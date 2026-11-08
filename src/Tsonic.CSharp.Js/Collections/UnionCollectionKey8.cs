using System;

namespace Tsonic.CSharp.Js;

public readonly struct UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3, TValue4, TCategory4, TValue5, TCategory5, TValue6, TCategory6, TValue7, TCategory7, TValue8, TCategory8> : ICollectionKey<Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>, UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3, TValue4, TCategory4, TValue5, TCategory5, TValue6, TCategory6, TValue7, TCategory7, TValue8, TCategory8>>
    where TCategory1 : struct, ISourceValueCategory<TValue1>
    where TCategory2 : struct, ISourceValueCategory<TValue2>
    where TCategory3 : struct, ISourceValueCategory<TValue3>
    where TCategory4 : struct, ISourceValueCategory<TValue4>
    where TCategory5 : struct, ISourceValueCategory<TValue5>
    where TCategory6 : struct, ISourceValueCategory<TValue6>
    where TCategory7 : struct, ISourceValueCategory<TValue7>
    where TCategory8 : struct, ISourceValueCategory<TValue8>
{
    private readonly Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> _value;
    private UnionCollectionKey(Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> value) { _value = value; }
    public static UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3, TValue4, TCategory4, TValue5, TCategory5, TValue6, TCategory6, TValue7, TCategory7, TValue8, TCategory8> Create(Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> value) =>
        value.Is1() || value.Is2() || value.Is3() || value.Is4() || value.Is5() || value.Is6() || value.Is7() || value.Is8() ? new(value) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");
    public Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8> Value =>
        _value.Is1() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From1(CollectionKeyComparison.Canonicalize<TValue1, TCategory1>(_value.As1())) :
        _value.Is2() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From2(CollectionKeyComparison.Canonicalize<TValue2, TCategory2>(_value.As2())) :
        _value.Is3() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From3(CollectionKeyComparison.Canonicalize<TValue3, TCategory3>(_value.As3())) :
        _value.Is4() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From4(CollectionKeyComparison.Canonicalize<TValue4, TCategory4>(_value.As4())) :
        _value.Is5() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From5(CollectionKeyComparison.Canonicalize<TValue5, TCategory5>(_value.As5())) :
        _value.Is6() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From6(CollectionKeyComparison.Canonicalize<TValue6, TCategory6>(_value.As6())) :
        _value.Is7() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From7(CollectionKeyComparison.Canonicalize<TValue7, TCategory7>(_value.As7())) :
        _value.Is8() ? Union<TValue1, TValue2, TValue3, TValue4, TValue5, TValue6, TValue7, TValue8>.From8(CollectionKeyComparison.Canonicalize<TValue8, TCategory8>(_value.As8())) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");

    public bool IsNaN =>
        _value.Is1() ? CollectionKeyComparison.IsNaN<TValue1, TCategory1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.IsNaN<TValue2, TCategory2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.IsNaN<TValue3, TCategory3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.IsNaN<TValue4, TCategory4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.IsNaN<TValue5, TCategory5>(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.IsNaN<TValue6, TCategory6>(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.IsNaN<TValue7, TCategory7>(_value.As7()) :
        _value.Is8() ? CollectionKeyComparison.IsNaN<TValue8, TCategory8>(_value.As8()) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");

    public bool Equals(UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3, TValue4, TCategory4, TValue5, TCategory5, TValue6, TCategory6, TValue7, TCategory7, TValue8, TCategory8> other)
    {
        if (_value.Is1())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue1, TCategory1>(_value.As1(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue2, TCategory2>(_value.As1(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue3, TCategory3>(_value.As1(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue4, TCategory4>(_value.As1(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue5, TCategory5>(_value.As1(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue6, TCategory6>(_value.As1(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue7, TCategory7>(_value.As1(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue1, TCategory1, TValue8, TCategory8>(_value.As1(), other._value.As8());
        if (_value.Is2())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue1, TCategory1>(_value.As2(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue2, TCategory2>(_value.As2(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue3, TCategory3>(_value.As2(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue4, TCategory4>(_value.As2(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue5, TCategory5>(_value.As2(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue6, TCategory6>(_value.As2(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue7, TCategory7>(_value.As2(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue2, TCategory2, TValue8, TCategory8>(_value.As2(), other._value.As8());
        if (_value.Is3())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue1, TCategory1>(_value.As3(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue2, TCategory2>(_value.As3(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue3, TCategory3>(_value.As3(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue4, TCategory4>(_value.As3(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue5, TCategory5>(_value.As3(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue6, TCategory6>(_value.As3(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue7, TCategory7>(_value.As3(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue3, TCategory3, TValue8, TCategory8>(_value.As3(), other._value.As8());
        if (_value.Is4())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue1, TCategory1>(_value.As4(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue2, TCategory2>(_value.As4(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue3, TCategory3>(_value.As4(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue4, TCategory4>(_value.As4(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue5, TCategory5>(_value.As4(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue6, TCategory6>(_value.As4(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue7, TCategory7>(_value.As4(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue4, TCategory4, TValue8, TCategory8>(_value.As4(), other._value.As8());
        if (_value.Is5())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue1, TCategory1>(_value.As5(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue2, TCategory2>(_value.As5(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue3, TCategory3>(_value.As5(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue4, TCategory4>(_value.As5(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue5, TCategory5>(_value.As5(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue6, TCategory6>(_value.As5(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue7, TCategory7>(_value.As5(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue5, TCategory5, TValue8, TCategory8>(_value.As5(), other._value.As8());
        if (_value.Is6())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue1, TCategory1>(_value.As6(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue2, TCategory2>(_value.As6(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue3, TCategory3>(_value.As6(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue4, TCategory4>(_value.As6(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue5, TCategory5>(_value.As6(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue6, TCategory6>(_value.As6(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue7, TCategory7>(_value.As6(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue6, TCategory6, TValue8, TCategory8>(_value.As6(), other._value.As8());
        if (_value.Is7())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue1, TCategory1>(_value.As7(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue2, TCategory2>(_value.As7(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue3, TCategory3>(_value.As7(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue4, TCategory4>(_value.As7(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue5, TCategory5>(_value.As7(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue6, TCategory6>(_value.As7(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue7, TCategory7>(_value.As7(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue7, TCategory7, TValue8, TCategory8>(_value.As7(), other._value.As8());
        if (_value.Is8())
            return other._value.Is1() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue1, TCategory1>(_value.As8(), other._value.As1()) ||
                other._value.Is2() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue2, TCategory2>(_value.As8(), other._value.As2()) ||
                other._value.Is3() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue3, TCategory3>(_value.As8(), other._value.As3()) ||
                other._value.Is4() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue4, TCategory4>(_value.As8(), other._value.As4()) ||
                other._value.Is5() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue5, TCategory5>(_value.As8(), other._value.As5()) ||
                other._value.Is6() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue6, TCategory6>(_value.As8(), other._value.As6()) ||
                other._value.Is7() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue7, TCategory7>(_value.As8(), other._value.As7()) ||
                other._value.Is8() && CollectionKeyComparison.Equal<TValue8, TCategory8, TValue8, TCategory8>(_value.As8(), other._value.As8());
        throw new InvalidOperationException("A collection key requires an initialized native union.");
    }

    public override bool Equals(object? other) => other is UnionCollectionKey<TValue1, TCategory1, TValue2, TCategory2, TValue3, TCategory3, TValue4, TCategory4, TValue5, TCategory5, TValue6, TCategory6, TValue7, TCategory7, TValue8, TCategory8> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<TValue1, TCategory1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<TValue2, TCategory2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<TValue3, TCategory3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<TValue4, TCategory4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash<TValue5, TCategory5>(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash<TValue6, TCategory6>(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.Hash<TValue7, TCategory7>(_value.As7()) :
        _value.Is8() ? CollectionKeyComparison.Hash<TValue8, TCategory8>(_value.As8()) :
        throw new InvalidOperationException("A collection key requires an initialized native union.");
}
