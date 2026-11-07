using System;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

public readonly struct UnionCollectionKey<T1, T2> : ICollectionKey<Union<T1, T2>, UnionCollectionKey<T1, T2>>
{
    private readonly Union<T1, T2> _value;
    private UnionCollectionKey(Union<T1, T2> value) { _value = value; }
    public static UnionCollectionKey<T1, T2> Create(Union<T1, T2> value) => new(value);
    public Union<T1, T2> Value =>
        _value.Is1() ? Union<T1, T2>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3> : ICollectionKey<Union<T1, T2, T3>, UnionCollectionKey<T1, T2, T3>>
{
    private readonly Union<T1, T2, T3> _value;
    private UnionCollectionKey(Union<T1, T2, T3> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3> Create(Union<T1, T2, T3> value) => new(value);
    public Union<T1, T2, T3> Value =>
        _value.Is1() ? Union<T1, T2, T3>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3, T4> : ICollectionKey<Union<T1, T2, T3, T4>, UnionCollectionKey<T1, T2, T3, T4>>
{
    private readonly Union<T1, T2, T3, T4> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3, T4> Create(Union<T1, T2, T3, T4> value) => new(value);
    public Union<T1, T2, T3, T4> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3, T4> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As1(), other._value.As4())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As2(), other._value.As4())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As3(), other._value.As4())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As4(), other._value.As4())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3, T4> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash(_value.As4()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3, T4, T5> : ICollectionKey<Union<T1, T2, T3, T4, T5>, UnionCollectionKey<T1, T2, T3, T4, T5>>
{
    private readonly Union<T1, T2, T3, T4, T5> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3, T4, T5> Create(Union<T1, T2, T3, T4, T5> value) => new(value);
    public Union<T1, T2, T3, T4, T5> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3, T4, T5> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As1(), other._value.As5())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As2(), other._value.As5())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As3(), other._value.As5())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As4(), other._value.As5())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As5(), other._value.As5())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3, T4, T5> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash(_value.As5()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3, T4, T5, T6> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6>, UnionCollectionKey<T1, T2, T3, T4, T5, T6>>
{
    private readonly Union<T1, T2, T3, T4, T5, T6> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3, T4, T5, T6> Create(Union<T1, T2, T3, T4, T5, T6> value) => new(value);
    public Union<T1, T2, T3, T4, T5, T6> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5, T6>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5, T6>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5, T6>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5, T6>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5, T6>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        _value.Is6() ? Union<T1, T2, T3, T4, T5, T6>.From6(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As6())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3, T4, T5, T6> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As1(), other._value.As6())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As2(), other._value.As6())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As3(), other._value.As6())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As4(), other._value.As6())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As5(), other._value.As6())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As6(), other._value.As6())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3, T4, T5, T6> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash(_value.As6()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6, T7>, UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7>>
{
    private readonly Union<T1, T2, T3, T4, T5, T6, T7> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6, T7> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7> Create(Union<T1, T2, T3, T4, T5, T6, T7> value) => new(value);
    public Union<T1, T2, T3, T4, T5, T6, T7> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5, T6, T7>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5, T6, T7>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5, T6, T7>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5, T6, T7>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5, T6, T7>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        _value.Is6() ? Union<T1, T2, T3, T4, T5, T6, T7>.From6(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As6())) :
        _value.Is7() ? Union<T1, T2, T3, T4, T5, T6, T7>.From7(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As7())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As1(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As1(), other._value.As7())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As2(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As2(), other._value.As7())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As3(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As3(), other._value.As7())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As4(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As4(), other._value.As7())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As5(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As5(), other._value.As7())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As6(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As6(), other._value.As7())) :
        _value.Is7() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As7(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As7(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As7(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As7(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As7(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As7(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As7(), other._value.As7())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.Hash(_value.As7()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7, T8> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6, T7, T8>, UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7, T8>>
{
    private readonly Union<T1, T2, T3, T4, T5, T6, T7, T8> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6, T7, T8> value) { _value = value; }
    public static UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7, T8> Create(Union<T1, T2, T3, T4, T5, T6, T7, T8> value) => new(value);
    public Union<T1, T2, T3, T4, T5, T6, T7, T8> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        _value.Is6() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From6(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As6())) :
        _value.Is7() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From7(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As7())) :
        _value.Is8() ? Union<T1, T2, T3, T4, T5, T6, T7, T8>.From8(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As8())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7, T8> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As1(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As1(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As1(), other._value.As8())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As2(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As2(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As2(), other._value.As8())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As3(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As3(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As3(), other._value.As8())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As4(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As4(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As4(), other._value.As8())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As5(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As5(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As5(), other._value.As8())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As6(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As6(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As6(), other._value.As8())) :
        _value.Is7() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As7(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As7(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As7(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As7(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As7(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As7(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As7(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As7(), other._value.As8())) :
        _value.Is8() ? (other._value.Is1() && CollectionKeyComparison.Equal(_value.As8(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal(_value.As8(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal(_value.As8(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal(_value.As8(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal(_value.As8(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal(_value.As8(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal(_value.As8(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal(_value.As8(), other._value.As8())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, T2, T3, T4, T5, T6, T7, T8> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.Hash(_value.As7()) :
        _value.Is8() ? CollectionKeyComparison.Hash(_value.As8()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}
