using System;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

// A native union key pairs every arm Ti with its Source key category Ci (an ICollectionKeyCategory marker). Two arms
// are the same key only when their categories are equal and their values are SameValueZero-equal (Q77: number and
// bigint stay distinct, 5n != 5). The categories are type arguments, so the comparison folds at JIT time.

public readonly struct UnionCollectionKey<T1, C1, T2, C2> : ICollectionKey<Union<T1, T2>, UnionCollectionKey<T1, C1, T2, C2>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2> _value;
    private UnionCollectionKey(Union<T1, T2> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2> Create(Union<T1, T2> value) => new(value);
    public Union<T1, T2> Value =>
        _value.Is1() ? Union<T1, T2>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3> : ICollectionKey<Union<T1, T2, T3>, UnionCollectionKey<T1, C1, T2, C2, T3, C3>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3> _value;
    private UnionCollectionKey(Union<T1, T2, T3> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3> Create(Union<T1, T2, T3> value) => new(value);
    public Union<T1, T2, T3> Value =>
        _value.Is1() ? Union<T1, T2, T3>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4> : ICollectionKey<Union<T1, T2, T3, T4>, UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
    where C4 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3, T4> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4> Create(Union<T1, T2, T3, T4> value) => new(value);
    public Union<T1, T2, T3, T4> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T1, C1, T4, C4>(_value.As1(), other._value.As4())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T2, C2, T4, C4>(_value.As2(), other._value.As4())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T3, C3, T4, C4>(_value.As3(), other._value.As4())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal<T4, C4, T1, C1>(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T4, C4, T2, C2>(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T4, C4, T3, C3>(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T4, C4, T4, C4>(_value.As4(), other._value.As4())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<T4, C4>(_value.As4()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5> : ICollectionKey<Union<T1, T2, T3, T4, T5>, UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
    where C4 : struct, ICollectionKeyCategory
    where C5 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3, T4, T5> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5> Create(Union<T1, T2, T3, T4, T5> value) => new(value);
    public Union<T1, T2, T3, T4, T5> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T1, C1, T4, C4>(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T1, C1, T5, C5>(_value.As1(), other._value.As5())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T2, C2, T4, C4>(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T2, C2, T5, C5>(_value.As2(), other._value.As5())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T3, C3, T4, C4>(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T3, C3, T5, C5>(_value.As3(), other._value.As5())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal<T4, C4, T1, C1>(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T4, C4, T2, C2>(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T4, C4, T3, C3>(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T4, C4, T4, C4>(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T4, C4, T5, C5>(_value.As4(), other._value.As5())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal<T5, C5, T1, C1>(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T5, C5, T2, C2>(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T5, C5, T3, C3>(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T5, C5, T4, C4>(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T5, C5, T5, C5>(_value.As5(), other._value.As5())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<T4, C4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash<T5, C5>(_value.As5()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6>, UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
    where C4 : struct, ICollectionKeyCategory
    where C5 : struct, ICollectionKeyCategory
    where C6 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3, T4, T5, T6> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6> Create(Union<T1, T2, T3, T4, T5, T6> value) => new(value);
    public Union<T1, T2, T3, T4, T5, T6> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5, T6>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5, T6>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5, T6>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5, T6>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5, T6>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        _value.Is6() ? Union<T1, T2, T3, T4, T5, T6>.From6(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As6())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T1, C1, T4, C4>(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T1, C1, T5, C5>(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T1, C1, T6, C6>(_value.As1(), other._value.As6())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T2, C2, T4, C4>(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T2, C2, T5, C5>(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T2, C2, T6, C6>(_value.As2(), other._value.As6())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T3, C3, T4, C4>(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T3, C3, T5, C5>(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T3, C3, T6, C6>(_value.As3(), other._value.As6())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal<T4, C4, T1, C1>(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T4, C4, T2, C2>(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T4, C4, T3, C3>(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T4, C4, T4, C4>(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T4, C4, T5, C5>(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T4, C4, T6, C6>(_value.As4(), other._value.As6())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal<T5, C5, T1, C1>(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T5, C5, T2, C2>(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T5, C5, T3, C3>(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T5, C5, T4, C4>(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T5, C5, T5, C5>(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T5, C5, T6, C6>(_value.As5(), other._value.As6())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal<T6, C6, T1, C1>(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T6, C6, T2, C2>(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T6, C6, T3, C3>(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T6, C6, T4, C4>(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T6, C6, T5, C5>(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T6, C6, T6, C6>(_value.As6(), other._value.As6())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<T4, C4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash<T5, C5>(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash<T6, C6>(_value.As6()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6, T7>, UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
    where C4 : struct, ICollectionKeyCategory
    where C5 : struct, ICollectionKeyCategory
    where C6 : struct, ICollectionKeyCategory
    where C7 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3, T4, T5, T6, T7> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6, T7> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7> Create(Union<T1, T2, T3, T4, T5, T6, T7> value) => new(value);
    public Union<T1, T2, T3, T4, T5, T6, T7> Value =>
        _value.Is1() ? Union<T1, T2, T3, T4, T5, T6, T7>.From1(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As1())) :
        _value.Is2() ? Union<T1, T2, T3, T4, T5, T6, T7>.From2(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As2())) :
        _value.Is3() ? Union<T1, T2, T3, T4, T5, T6, T7>.From3(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As3())) :
        _value.Is4() ? Union<T1, T2, T3, T4, T5, T6, T7>.From4(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As4())) :
        _value.Is5() ? Union<T1, T2, T3, T4, T5, T6, T7>.From5(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As5())) :
        _value.Is6() ? Union<T1, T2, T3, T4, T5, T6, T7>.From6(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As6())) :
        _value.Is7() ? Union<T1, T2, T3, T4, T5, T6, T7>.From7(JSKeyEquality.canonicalizeKeyedCollectionKey(_value.As7())) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T1, C1, T4, C4>(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T1, C1, T5, C5>(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T1, C1, T6, C6>(_value.As1(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T1, C1, T7, C7>(_value.As1(), other._value.As7())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T2, C2, T4, C4>(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T2, C2, T5, C5>(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T2, C2, T6, C6>(_value.As2(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T2, C2, T7, C7>(_value.As2(), other._value.As7())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T3, C3, T4, C4>(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T3, C3, T5, C5>(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T3, C3, T6, C6>(_value.As3(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T3, C3, T7, C7>(_value.As3(), other._value.As7())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal<T4, C4, T1, C1>(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T4, C4, T2, C2>(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T4, C4, T3, C3>(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T4, C4, T4, C4>(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T4, C4, T5, C5>(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T4, C4, T6, C6>(_value.As4(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T4, C4, T7, C7>(_value.As4(), other._value.As7())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal<T5, C5, T1, C1>(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T5, C5, T2, C2>(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T5, C5, T3, C3>(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T5, C5, T4, C4>(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T5, C5, T5, C5>(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T5, C5, T6, C6>(_value.As5(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T5, C5, T7, C7>(_value.As5(), other._value.As7())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal<T6, C6, T1, C1>(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T6, C6, T2, C2>(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T6, C6, T3, C3>(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T6, C6, T4, C4>(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T6, C6, T5, C5>(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T6, C6, T6, C6>(_value.As6(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T6, C6, T7, C7>(_value.As6(), other._value.As7())) :
        _value.Is7() ? (other._value.Is1() && CollectionKeyComparison.Equal<T7, C7, T1, C1>(_value.As7(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T7, C7, T2, C2>(_value.As7(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T7, C7, T3, C3>(_value.As7(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T7, C7, T4, C4>(_value.As7(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T7, C7, T5, C5>(_value.As7(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T7, C7, T6, C6>(_value.As7(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T7, C7, T7, C7>(_value.As7(), other._value.As7())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<T4, C4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash<T5, C5>(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash<T6, C6>(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.Hash<T7, C7>(_value.As7()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}

public readonly struct UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7, T8, C8> : ICollectionKey<Union<T1, T2, T3, T4, T5, T6, T7, T8>, UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7, T8, C8>>
    where C1 : struct, ICollectionKeyCategory
    where C2 : struct, ICollectionKeyCategory
    where C3 : struct, ICollectionKeyCategory
    where C4 : struct, ICollectionKeyCategory
    where C5 : struct, ICollectionKeyCategory
    where C6 : struct, ICollectionKeyCategory
    where C7 : struct, ICollectionKeyCategory
    where C8 : struct, ICollectionKeyCategory
{
    private readonly Union<T1, T2, T3, T4, T5, T6, T7, T8> _value;
    private UnionCollectionKey(Union<T1, T2, T3, T4, T5, T6, T7, T8> value) { _value = value; }
    public static UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7, T8, C8> Create(Union<T1, T2, T3, T4, T5, T6, T7, T8> value) => new(value);
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

    public bool Equals(UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7, T8, C8> other) =>
        _value.Is1() ? (other._value.Is1() && CollectionKeyComparison.Equal<T1, C1, T1, C1>(_value.As1(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T1, C1, T2, C2>(_value.As1(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T1, C1, T3, C3>(_value.As1(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T1, C1, T4, C4>(_value.As1(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T1, C1, T5, C5>(_value.As1(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T1, C1, T6, C6>(_value.As1(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T1, C1, T7, C7>(_value.As1(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T1, C1, T8, C8>(_value.As1(), other._value.As8())) :
        _value.Is2() ? (other._value.Is1() && CollectionKeyComparison.Equal<T2, C2, T1, C1>(_value.As2(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T2, C2, T2, C2>(_value.As2(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T2, C2, T3, C3>(_value.As2(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T2, C2, T4, C4>(_value.As2(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T2, C2, T5, C5>(_value.As2(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T2, C2, T6, C6>(_value.As2(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T2, C2, T7, C7>(_value.As2(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T2, C2, T8, C8>(_value.As2(), other._value.As8())) :
        _value.Is3() ? (other._value.Is1() && CollectionKeyComparison.Equal<T3, C3, T1, C1>(_value.As3(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T3, C3, T2, C2>(_value.As3(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T3, C3, T3, C3>(_value.As3(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T3, C3, T4, C4>(_value.As3(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T3, C3, T5, C5>(_value.As3(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T3, C3, T6, C6>(_value.As3(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T3, C3, T7, C7>(_value.As3(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T3, C3, T8, C8>(_value.As3(), other._value.As8())) :
        _value.Is4() ? (other._value.Is1() && CollectionKeyComparison.Equal<T4, C4, T1, C1>(_value.As4(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T4, C4, T2, C2>(_value.As4(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T4, C4, T3, C3>(_value.As4(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T4, C4, T4, C4>(_value.As4(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T4, C4, T5, C5>(_value.As4(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T4, C4, T6, C6>(_value.As4(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T4, C4, T7, C7>(_value.As4(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T4, C4, T8, C8>(_value.As4(), other._value.As8())) :
        _value.Is5() ? (other._value.Is1() && CollectionKeyComparison.Equal<T5, C5, T1, C1>(_value.As5(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T5, C5, T2, C2>(_value.As5(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T5, C5, T3, C3>(_value.As5(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T5, C5, T4, C4>(_value.As5(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T5, C5, T5, C5>(_value.As5(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T5, C5, T6, C6>(_value.As5(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T5, C5, T7, C7>(_value.As5(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T5, C5, T8, C8>(_value.As5(), other._value.As8())) :
        _value.Is6() ? (other._value.Is1() && CollectionKeyComparison.Equal<T6, C6, T1, C1>(_value.As6(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T6, C6, T2, C2>(_value.As6(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T6, C6, T3, C3>(_value.As6(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T6, C6, T4, C4>(_value.As6(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T6, C6, T5, C5>(_value.As6(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T6, C6, T6, C6>(_value.As6(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T6, C6, T7, C7>(_value.As6(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T6, C6, T8, C8>(_value.As6(), other._value.As8())) :
        _value.Is7() ? (other._value.Is1() && CollectionKeyComparison.Equal<T7, C7, T1, C1>(_value.As7(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T7, C7, T2, C2>(_value.As7(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T7, C7, T3, C3>(_value.As7(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T7, C7, T4, C4>(_value.As7(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T7, C7, T5, C5>(_value.As7(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T7, C7, T6, C6>(_value.As7(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T7, C7, T7, C7>(_value.As7(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T7, C7, T8, C8>(_value.As7(), other._value.As8())) :
        _value.Is8() ? (other._value.Is1() && CollectionKeyComparison.Equal<T8, C8, T1, C1>(_value.As8(), other._value.As1()) ||
            other._value.Is2() && CollectionKeyComparison.Equal<T8, C8, T2, C2>(_value.As8(), other._value.As2()) ||
            other._value.Is3() && CollectionKeyComparison.Equal<T8, C8, T3, C3>(_value.As8(), other._value.As3()) ||
            other._value.Is4() && CollectionKeyComparison.Equal<T8, C8, T4, C4>(_value.As8(), other._value.As4()) ||
            other._value.Is5() && CollectionKeyComparison.Equal<T8, C8, T5, C5>(_value.As8(), other._value.As5()) ||
            other._value.Is6() && CollectionKeyComparison.Equal<T8, C8, T6, C6>(_value.As8(), other._value.As6()) ||
            other._value.Is7() && CollectionKeyComparison.Equal<T8, C8, T7, C7>(_value.As8(), other._value.As7()) ||
            other._value.Is8() && CollectionKeyComparison.Equal<T8, C8, T8, C8>(_value.As8(), other._value.As8())) :
        false;
    public override bool Equals(object? other) => other is UnionCollectionKey<T1, C1, T2, C2, T3, C3, T4, C4, T5, C5, T6, C6, T7, C7, T8, C8> key && Equals(key);
    public override int GetHashCode() =>
        _value.Is1() ? CollectionKeyComparison.Hash<T1, C1>(_value.As1()) :
        _value.Is2() ? CollectionKeyComparison.Hash<T2, C2>(_value.As2()) :
        _value.Is3() ? CollectionKeyComparison.Hash<T3, C3>(_value.As3()) :
        _value.Is4() ? CollectionKeyComparison.Hash<T4, C4>(_value.As4()) :
        _value.Is5() ? CollectionKeyComparison.Hash<T5, C5>(_value.As5()) :
        _value.Is6() ? CollectionKeyComparison.Hash<T6, C6>(_value.As6()) :
        _value.Is7() ? CollectionKeyComparison.Hash<T7, C7>(_value.As7()) :
        _value.Is8() ? CollectionKeyComparison.Hash<T8, C8>(_value.As8()) :
        throw new InvalidOperationException("A native union key requires an initialized arm.");
}
