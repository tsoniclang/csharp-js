using System;
using System.Linq;
using System.Numerics;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("Native allocation cost")]
public sealed class TypedCollectionKeyTests
{
    [Fact]
    public void SupersededKeyAndContainerAritiesDoNotSurviveAsCompatibilityPaths()
    {
        var assembly = typeof(Set<,>).Assembly;
        Assert.Null(assembly.GetType("Tsonic.CSharp.Js.Set`1"));
        Assert.Null(assembly.GetType("Tsonic.CSharp.Js.Map`2"));
        Assert.Null(assembly.GetType("Tsonic.CSharp.Js.CollectionKey`1"));
        Assert.Null(assembly.GetType("Tsonic.CSharp.Js.OptionalCollectionKey`2"));
    }

    [Fact]
    public void SameCategoryArmsCompareByExactPayloadWithoutChangingNativeUnionIdentity()
    {
        var first = Union<int, uint>.From1(7);
        var second = Union<int, uint>.From2(7);
        Assert.False(first.Equals(second));
        AssertEquivalent<Union<int, uint>, UnionCollectionKey<int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>>>(first, second);
        var coincident = new Set<Union<long, long>, UnionCollectionKey<long, SourceNumberCategory<long>, long, SourceNumberCategory<long>>>();
        coincident.add(Union<long, long>.From1(7)).add(Union<long, long>.From2(7));
        Assert.Equal(1, coincident.size);
        var categories = new Set<Union<long, long>, UnionCollectionKey<long, SourceNumberCategory<long>, long, SourceBigIntCategory<long>>>();
        categories.add(Union<long, long>.From1(7)).add(Union<long, long>.From2(7));
        Assert.Equal(2, categories.size);
    }

    [Fact]
    public void NativeOrderedValuePayloadsStayAtomicAcrossTypedAndBroadKeys()
    {
        var first = Union<int, int>.From1(7);
        var second = Union<int, int>.From2(7);
        var typed = new Set<Union<int, int>, SourceCollectionKey<Union<int, int>, SourceObjectCategory<Union<int, int>>>>();
        typed.add(first).add(second).add(first);
        Assert.Equal(2, typed.size);
        var broadFirst = TsValue.fromsource<Union<int, int>, SourceObjectCategory<Union<int, int>>>(first);
        var broadSecond = TsValue.fromsource<Union<int, int>, SourceObjectCategory<Union<int, int>>>(second);
        var broad = new Set<TsValue, SourceCollectionKey<TsValue, SourceStoredValueCategory>>();
        broad.add(broadFirst).add(broadSecond).add(TsValue.fromsource<Union<int, int>, SourceObjectCategory<Union<int, int>>>(first));
        Assert.Equal(2, broad.size);
        var mixed = new Set<Union<TsValue, Union<int, int>>, UnionCollectionKey<TsValue, SourceStoredValueCategory, Union<int, int>, SourceObjectCategory<Union<int, int>>>>();
        mixed.add(Union<TsValue, Union<int, int>>.From1(broadFirst));
        mixed.add(Union<TsValue, Union<int, int>>.From2(first));
        mixed.add(Union<TsValue, Union<int, int>>.From2(second));
        Assert.Equal(2, mixed.size);
        AssertEquivalent<Union<TsValue, Union<int, int>>, UnionCollectionKey<TsValue, SourceStoredValueCategory, Union<int, int>, SourceObjectCategory<Union<int, int>>>>(
            Union<TsValue, Union<int, int>>.From1(broadFirst), Union<TsValue, Union<int, int>>.From2(first));
    }

    [Fact]
    public void EveryImmediateNativeArityHasExactSymmetricKeysAndHashes()
    {
        AssertEquivalent<Union<sbyte, byte>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>>>(
            Union<sbyte, byte>.From1((sbyte)7),
            Union<sbyte, byte>.From2((byte)7));
        AssertEquivalent<Union<sbyte, byte, short>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>>>(
            Union<sbyte, byte, short>.From1((sbyte)7),
            Union<sbyte, byte, short>.From2((byte)7),
            Union<sbyte, byte, short>.From3((short)7));
        AssertEquivalent<Union<sbyte, byte, short, ushort>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>, ushort, SourceNumberCategory<ushort>>>(
            Union<sbyte, byte, short, ushort>.From1((sbyte)7),
            Union<sbyte, byte, short, ushort>.From2((byte)7),
            Union<sbyte, byte, short, ushort>.From3((short)7),
            Union<sbyte, byte, short, ushort>.From4((ushort)7));
        AssertEquivalent<Union<sbyte, byte, short, ushort, int>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>, ushort, SourceNumberCategory<ushort>, int, SourceNumberCategory<int>>>(
            Union<sbyte, byte, short, ushort, int>.From1((sbyte)7),
            Union<sbyte, byte, short, ushort, int>.From2((byte)7),
            Union<sbyte, byte, short, ushort, int>.From3((short)7),
            Union<sbyte, byte, short, ushort, int>.From4((ushort)7),
            Union<sbyte, byte, short, ushort, int>.From5((int)7));
        AssertEquivalent<Union<sbyte, byte, short, ushort, int, uint>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>, ushort, SourceNumberCategory<ushort>, int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>>>(
            Union<sbyte, byte, short, ushort, int, uint>.From1((sbyte)7),
            Union<sbyte, byte, short, ushort, int, uint>.From2((byte)7),
            Union<sbyte, byte, short, ushort, int, uint>.From3((short)7),
            Union<sbyte, byte, short, ushort, int, uint>.From4((ushort)7),
            Union<sbyte, byte, short, ushort, int, uint>.From5((int)7),
            Union<sbyte, byte, short, ushort, int, uint>.From6((uint)7));
        AssertEquivalent<Union<sbyte, byte, short, ushort, int, uint, long>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>, ushort, SourceNumberCategory<ushort>, int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>, long, SourceNumberCategory<long>>>(
            Union<sbyte, byte, short, ushort, int, uint, long>.From1((sbyte)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From2((byte)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From3((short)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From4((ushort)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From5((int)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From6((uint)7),
            Union<sbyte, byte, short, ushort, int, uint, long>.From7((long)7));
        AssertEquivalent<Union<sbyte, byte, short, ushort, int, uint, long, ulong>, UnionCollectionKey<sbyte, SourceNumberCategory<sbyte>, byte, SourceNumberCategory<byte>, short, SourceNumberCategory<short>, ushort, SourceNumberCategory<ushort>, int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>, long, SourceNumberCategory<long>, ulong, SourceNumberCategory<ulong>>>(
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From1((sbyte)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From2((byte)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From3((short)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From4((ushort)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From5((int)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From6((uint)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From7((long)7),
            Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From8((ulong)7));
    }

    [Fact]
    public void ExactNumericCategoriesDoNotRoundAndBigIntegerDecimalParityIsSymmetric()
    {
        var decimals = new Set<Union<decimal, double>, UnionCollectionKey<decimal, SourceNumberCategory<decimal>, double, SourceNumberCategory<double>>>();
        decimals.add(Union<decimal, double>.From1(0.5m)).add(Union<decimal, double>.From2(0.5));
        Assert.Equal(1, decimals.size);
        decimals.add(Union<decimal, double>.From1(0.1m)).add(Union<decimal, double>.From2(0.1));
        Assert.Equal(3, decimals.size);
        var integers = new Set<Union<long, BigInteger>, UnionCollectionKey<long, SourceBigIntCategory<long>, BigInteger, SourceBigIntCategory<BigInteger>>>();
        integers.add(Union<long, BigInteger>.From1(long.MaxValue)).add(Union<long, BigInteger>.From2(new BigInteger(long.MaxValue)));
        Assert.Equal(1, integers.size);
        var large = BigInteger.One << 300;
        integers.add(Union<long, BigInteger>.From2(large)).add(Union<long, BigInteger>.From2(large + 1));
        Assert.Equal(3, integers.size);
        var wide = new Set<Union<long, double>, UnionCollectionKey<long, SourceNumberCategory<long>, double, SourceNumberCategory<double>>>();
        wide.add(Union<long, double>.From1(9007199254740993)).add(Union<long, double>.From2(9007199254740992d));
        Assert.Equal(2, wide.size);
    }

    [Fact]
    public void BroadStoredCategoriesKeepOneAbsenceAndSameCategoryCoincidentValues()
    {
        var keys = new Set<TsValue, SourceCollectionKey<TsValue, SourceStoredValueCategory>>();
        var number = TsValue.fromsource<long, SourceNumberCategory<long>>(7);
        var integer = TsValue.fromsource<long, SourceBigIntCategory<long>>(7);
        keys.add(number).add(integer).add(TsValue.fromsource<double, SourceNumberCategory<double>>(7));
        keys.add(default).add(TsValue.undefined());
        keys.add(TsValue.fromsource<int, SourceNumberCategory<int>>(0));
        keys.add(TsValue.fromsource<bool, SourceBooleanCategory<bool>>(false));
        keys.add(TsValue.fromsource<string, SourceStringCategory<string>>(""));
        Assert.Equal(6, keys.size);
        Assert.True(keys.has(number));
        Assert.True(keys.has(integer));
        var mixed = new Set<Union<TsValue, int>, UnionCollectionKey<TsValue, SourceStoredValueCategory, int, SourceNumberCategory<int>>>();
        mixed.add(Union<TsValue, int>.From1(number)).add(Union<TsValue, int>.From2(7));
        Assert.Equal(1, mixed.size);
        var optional = new Set<int?, OptionalCollectionKey<int, int?, ValueOptionalStorage<int>, NativeCollectionKey<int>>>();
        optional.add(null).add(0).add(1).add(null);
        Assert.Equal(3, optional.size);
    }

    [Fact]
    public void GenericOptionalKeysComposeOnlyTheExistingStorageOperations()
    {
        CheckOptionalKey<int, int?, ValueOptionalStorage<int>, SourceCollectionKey<int, SourceNumberCategory<int>>>(0, 1);
        CheckOptionalKey<long, long?, ValueOptionalStorage<long>, SourceCollectionKey<long, SourceNumberCategory<long>>>(0, long.MaxValue);
        CheckOptionalKey<bool, bool?, ValueOptionalStorage<bool>, SourceCollectionKey<bool, SourceBooleanCategory<bool>>>(false, true);
        CheckOptionalKey<string, string?, ReferenceOptionalStorage<string>, SourceCollectionKey<string, SourceStringCategory<string>>>("", "present");
        CheckOptionalKey<TsValue, TsValue, JsValueOptionalStorage, SourceCollectionKey<TsValue, SourceStoredValueCategory>>(
            TsValue.fromsource<long, SourceNumberCategory<long>>(7), TsValue.fromsource<long, SourceBigIntCategory<long>>(7));
    }

    [Fact]
    public void AuthenticatedObjectCarrierKeysUseOnePayloadHashWithoutReclassifyingSourceNumbers()
    {
        object[] values = [7L, 7d, TsValue.fromsource<long, SourceNumberCategory<long>>(7), TsValue.fromsource<double, SourceNumberCategory<double>>(7)];
        AssertEquivalent<object, SourceCollectionKey<object, SourceNumberCategory<object>>>(values);
        AssertEquivalent<Union<object, long>, UnionCollectionKey<object, SourceNumberCategory<object>, long, SourceNumberCategory<long>>>(
            Union<object, long>.From1(values[0]), Union<object, long>.From1(values[2]), Union<object, long>.From2(7));
        var larger = BigInteger.One << 200 | 1;
        AssertEquivalent<Union<object, BigInteger>, UnionCollectionKey<object, SourceBigIntCategory<object>, BigInteger, SourceBigIntCategory<BigInteger>>>(
            Union<object, BigInteger>.From1(larger), Union<object, BigInteger>.From2(larger));
        var malformed = SourceCollectionKey<object, SourceNumberCategory<object>>.Create(TsValue.fromsource<long, SourceBigIntCategory<long>>(7));
        Assert.Throws<ArgumentException>(() => malformed.GetHashCode());
        Assert.Throws<ArgumentException>(() => malformed.Equals(SourceCollectionKey<object, SourceNumberCategory<object>>.Create(7L)));
    }

    [Fact]
    public void ReferenceAndSymbolKeysUseIdentityAcrossStaticArmsAndNeverDelegateValueEquality()
    {
        var value = new DerivedKey();
        var keys = new Set<Union<BaseKey, DerivedKey>, UnionCollectionKey<BaseKey, SourceObjectCategory<BaseKey>, DerivedKey, SourceObjectCategory<DerivedKey>>>();
        keys.add(Union<BaseKey, DerivedKey>.From1(value)).add(Union<BaseKey, DerivedKey>.From2(value));
        keys.add(Union<BaseKey, DerivedKey>.From2(new DerivedKey()));
        Assert.Equal(2, keys.size);
        var firstSymbol = Symbol.create("same");
        var secondSymbol = Symbol.create("same");
        var symbols = new Set<Symbol, SourceCollectionKey<Symbol, SourceSymbolCategory<Symbol>>>();
        symbols.add(firstSymbol).add(secondSymbol).add(firstSymbol);
        Assert.Equal(2, symbols.size);
        var first = new Action(NoOperation);
        var second = new Action(NoOperation);
        Assert.True(first.Equals(second));
        var functions = new Set<Action, SourceCollectionKey<Action, SourceFunctionCategory<Action>>>();
        functions.add(first).add(second);
        Assert.Equal(2, functions.size);
    }

    [Fact]
    public void StaticZeroNaNAndOrderedMutationKeepNativePayloadAndOneCanonicalZero()
    {
        var keys = new Set<Union<double, float>, UnionCollectionKey<double, SourceNumberCategory<double>, float, SourceNumberCategory<float>>>();
        keys.add(Union<double, float>.From2(-0f)).add(Union<double, float>.From1(0d));
        keys.add(Union<double, float>.From1(double.NaN)).add(Union<double, float>.From2(float.NaN));
        Assert.Equal(2, keys.size);
        Assert.Equal(0, BitConverter.SingleToInt32Bits(keys.values().First().As2()));
        var mapping = new Map<int, int, NativeCollectionKey<int>>();
        mapping.set(1, 10).set(2, 20);
        var seen = new System.Collections.Generic.List<int>();
        mapping.forEach((value, key) =>
        {
            seen.Add(value);
            if (key == 1) { mapping.delete(1); mapping.delete(2); mapping.set(3, 30); }
            else if (key == 3) { mapping.clear(); mapping.set(4, 40); }
        });
        Assert.Equal(new[] { 10, 30, 40 }, seen);
    }

    [Fact]
    public void BroadCanonicalZeroReusesItsNativeBoxWithoutMutatingTheOriginalValue()
    {
        var original = TsValue.fromsource<double, SourceNumberCategory<double>>(-0d);
        var key = SourceCollectionKey<TsValue, SourceStoredValueCategory>.Create(original);
        var first = key.Value;
        Assert.Equal(SourceValueCategory.Number, first.Category);
        Assert.Equal(0L, BitConverter.DoubleToInt64Bits(Assert.IsType<double>(first.unwrap())));
        Assert.Equal(BitConverter.DoubleToInt64Bits(-0d), BitConverter.DoubleToInt64Bits(Assert.IsType<double>(original.unwrap())));
        Assert.Same(first.unwrap(), key.Value.unwrap());
        var native = NativeCollectionKey<object>.Create(-0d);
        Assert.Same(first.unwrap(), native.Value);
        var outputs = new object?[1000];
        for (var index = 0; index < outputs.Length; index++) outputs[index] = key.Value.unwrap();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < outputs.Length; index++) outputs[index] = key.Value.unwrap();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.All(outputs, value => Assert.Same(first.unwrap(), value));
    }

    [Fact]
    public void StaticUnionHotLookupOverwriteAndDeletionDoNotBoxOrAllocate()
    {
        var signed = Union<int, uint>.From1(7);
        var unsigned = Union<int, uint>.From2(7);
        var set = new Set<Union<int, uint>, UnionCollectionKey<int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>>>();
        var map = new Map<Union<int, uint>, int, UnionCollectionKey<int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>>>();
        set.add(signed);
        map.set(signed, 9);
        for (var index = 0; index < 10000; index++)
        {
            set.has(unsigned);
            map.tryGet(unsigned, out _);
            map.set(unsigned, 9);
            set.delete(unsigned);
            set.add(signed);
        }
        var matches = 0;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++)
        {
            if (set.has(unsigned)) matches++;
            if (map.tryGet(unsigned, out var value)) matches += value;
            map.set(unsigned, 9);
            set.delete(unsigned);
            set.add(signed);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(100000, matches);
    }

    [Fact]
    public void HomogeneousSourceNumberStorageRetainsTheNativeScalarFastPath()
    {
        var map = new Map<long, long, SourceCollectionKey<long, SourceNumberCategory<long>>>();
        map.set(long.MaxValue, long.MinValue);
        Assert.Equal(long.MaxValue.GetHashCode(), SourceCollectionKey<long, SourceNumberCategory<long>>.Create(long.MaxValue).GetHashCode());
        for (var index = 0; index < 10000; index++)
        {
            map.has(long.MaxValue);
            map.set(long.MaxValue, long.MinValue);
            map.tryGet(long.MaxValue, out _);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var found = 0;
        for (var index = 0; index < 10000; index++)
        {
            if (map.has(long.MaxValue)) found++;
            map.set(long.MaxValue, long.MinValue);
            if (map.tryGet(long.MaxValue, out var value) && value == long.MinValue) found++;
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(20000, found);
        Assert.Throws<InvalidOperationException>(() => UnionCollectionKey<int, SourceNumberCategory<int>, uint, SourceNumberCategory<uint>>.Create(default));
    }

    private static void AssertEquivalent<TValue, TKey>(params TValue[] values) where TKey : struct, ICollectionKey<TValue, TKey>
    {
        var set = new Set<TValue, TKey>(values);
        var map = new Map<TValue, int, TKey>();
        foreach (var value in values) map.set(value, 7);
        Assert.Equal(1, set.size);
        Assert.Equal(1, map.size);
        foreach (var left in values)
            foreach (var right in values)
            {
                Assert.True(TKey.Create(left).Equals(TKey.Create(right)));
                Assert.True(TKey.Create(right).Equals(TKey.Create(left)));
                Assert.Equal(TKey.Create(left).GetHashCode(), TKey.Create(right).GetHashCode());
                Assert.True(set.has(right));
                Assert.True(map.tryGet(right, out var selected));
                Assert.Equal(7, selected);
            }
        Assert.True(set.delete(values[^1]));
        Assert.Equal(0, set.size);
    }

    private static void CheckOptionalKey<TPayload, TStorage, TOps, TKey>(TPayload first, TPayload second)
        where TOps : struct, IOptionalStorage<TPayload, TStorage>
        where TKey : struct, ICollectionKey<TPayload, TKey>
    {
        var absent = TOps.From1(null);
        var firstStorage = TOps.From2(first);
        var secondStorage = TOps.From2(second);
        var keys = new Set<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>();
        keys.add(absent).add(firstStorage).add(secondStorage).add(absent);
        Assert.Equal(3, keys.size);
        Assert.True(keys.has(absent));
        Assert.True(keys.has(firstStorage));
        Assert.True(keys.has(secondStorage));
        Assert.True(keys.delete(absent));
        Assert.Equal(2, keys.size);
        Assert.All(keys.values(), value => Assert.True(TOps.Is2(value)));
    }

    private static void NoOperation() { }
    private class BaseKey
    {
        public override bool Equals(object? value) => value is BaseKey;
        public override int GetHashCode() => 7;
    }
    private sealed class DerivedKey : BaseKey { }
}
