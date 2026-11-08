using System;
using System.Numerics;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeCollectionKeyEqualityTests
{
    [Fact]
    public void EqualNumberUnionArmsShareSetAndMapKeysWithoutChangingNativeUnionEquality()
    {
        var signed = Union<int, uint>.From1(1);
        var unsigned = Union<int, uint>.From2(1U);
        Assert.False(signed.Equals(unsigned));

        var values = new Set<Union<int, uint>, NativeCollectionKey<Union<int, uint>>>();
        values.add(signed).add(unsigned);
        Assert.Equal(1, values.size);
        Assert.True(values.has(unsigned));
        Assert.True(Assert.Single(values.values()).Is1());
        Assert.True(values.delete(unsigned));
        Assert.Equal(0, values.size);

        var mapping = new Map<Union<int, uint>, int, NativeCollectionKey<Union<int, uint>>>();
        mapping.set(signed, 10).set(unsigned, 20);
        Assert.Equal(1, mapping.size);
        Assert.True(mapping.tryGet(signed, out var result));
        Assert.Equal(20, result);
        Assert.True(mapping.delete(unsigned));
        Assert.Equal(0, mapping.size);
    }

    [Fact]
    public void ExactCrossCarrierNumericValuesHaveTheSameCollectionKeyAndHash()
    {
        var pairs = new (object Left, object Right)[]
        {
            (1, 1U),
            (1L, BigInteger.One),
            (UInt128.MaxValue, (BigInteger)UInt128.MaxValue),
            (Int128.MinValue, (BigInteger)Int128.MinValue),
            (0.5m, 0.5),
            (0.125m, (Half)0.125),
            (decimal.MaxValue, (BigInteger)decimal.MaxValue),
            (BigInteger.One << 200, System.Math.ScaleB(1.0, 200)),
            (-(BigInteger.One << 200), -System.Math.ScaleB(1.0, 200)),
            (new BigInteger(double.MaxValue), double.MaxValue),
            (double.PositiveInfinity, float.PositiveInfinity),
            (double.NegativeInfinity, Half.NegativeInfinity),
            (double.NaN, Half.NaN),
            (-0.0, 0m),
        };
        foreach (var (left, right) in pairs)
        {
            var values = new Set<object, NativeCollectionKey<object>>();
            values.add(left).add(right);
            Assert.Equal(1, values.size);
            Assert.True(values.has(left));
            Assert.True(values.has(right));
            Assert.True(values.delete(right));
            Assert.Equal(0, values.size);
        }
    }

    [Fact]
    public void UnequalExactNativeValuesDoNotRoundIntoOneKey()
    {
        var pairs = new (object Left, object Right)[]
        {
            (9007199254740993L, 9007199254740992.0),
            (long.MaxValue, (double)long.MaxValue),
            (UInt128.MaxValue, (double)UInt128.MaxValue),
            (0.1m, 0.1),
            ((BigInteger.One << 200) + 1, System.Math.ScaleB(1.0, 200)),
            ((BigInteger.One << 200) + 1, (BigInteger.One << 200) + 2),
            (decimal.MaxValue, (BigInteger)decimal.MaxValue + 1),
        };
        foreach (var (left, right) in pairs)
        {
            var values = new Set<object, NativeCollectionKey<object>>();
            values.add(left).add(right);
            Assert.Equal(2, values.size);
            Assert.True(values.has(left));
            Assert.True(values.has(right));
        }
    }

    [Fact]
    public void StrictSearchRejectsNaNAcrossClosedUnionArms()
    {
        var values = new JSArray<Union<double, float>>(new[] { Union<double, float>.From1(double.NaN) });
        Assert.True(values.includes(Union<double, float>.From2(float.NaN)));
        Assert.Equal(-1, values.indexOf(Union<double, float>.From2(float.NaN)));
    }

    [Fact]
    public void RawNativeKeyEquivalenceAndHashDoNotMixSourceCategoryRules()
    {
        object[] values = [7L, 7d, TsValue.fromsource<long, SourceNumberCategory<long>>(7), TsValue.fromsource<long, SourceBigIntCategory<long>>(7)];
        foreach (var left in values)
            foreach (var right in values)
            {
                var first = NativeCollectionKey<object>.Create(left);
                var second = NativeCollectionKey<object>.Create(right);
                Assert.True(first.Equals(second));
                Assert.Equal(first.GetHashCode(), second.GetHashCode());
            }
        var keys = new Set<object, NativeCollectionKey<object>>(values);
        Assert.Equal(1, keys.size);
        var array = JSArray<object>.of(values);
        Assert.Equal(0, array.indexOf(values[3]));
        Assert.Equal(3, array.lastIndexOf(values[0]));
    }
}
