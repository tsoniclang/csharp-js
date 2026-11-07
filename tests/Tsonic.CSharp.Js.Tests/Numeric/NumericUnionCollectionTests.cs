using System;
using System.Linq;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("Native allocation cost")]
public class NumericUnionCollectionTests
{

    [Fact]
    public void EveryNativeUnionArityUsesExactSymmetricKeysAcrossAllNumberCarriers()
    {
        var keys1Of2 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory>.Create(Union<sbyte, byte>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory>.Create(Union<sbyte, byte>.From2(5))
        };
        foreach (var key in keys1Of2)
        {
            Assert.True(keys1Of2[0].Equals(key) && key.Equals(keys1Of2[0]));
            Assert.Equal(keys1Of2[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of3 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory>.Create(Union<sbyte, byte, short>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory>.Create(Union<sbyte, byte, short>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory>.Create(Union<sbyte, byte, short>.From3(5))
        };
        foreach (var key in keys1Of3)
        {
            Assert.True(keys1Of3[0].Equals(key) && key.Equals(keys1Of3[0]));
            Assert.Equal(keys1Of3[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of4 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort>.From3(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort>.From4(5))
        };
        foreach (var key in keys1Of4)
        {
            Assert.True(keys1Of4[0].Equals(key) && key.Equals(keys1Of4[0]));
            Assert.Equal(keys1Of4[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of5 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int>.From3(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int>.From4(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int>.From5(5))
        };
        foreach (var key in keys1Of5)
        {
            Assert.True(keys1Of5[0].Equals(key) && key.Equals(keys1Of5[0]));
            Assert.Equal(keys1Of5[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of6 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From3(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From4(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From5(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint>.From6(5U))
        };
        foreach (var key in keys1Of6)
        {
            Assert.True(keys1Of6[0].Equals(key) && key.Equals(keys1Of6[0]));
            Assert.Equal(keys1Of6[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of7 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From3(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From4(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From5(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From6(5U)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long>.From7(5L))
        };
        foreach (var key in keys1Of7)
        {
            Assert.True(keys1Of7[0].Equals(key) && key.Equals(keys1Of7[0]));
            Assert.Equal(keys1Of7[0].GetHashCode(), key.GetHashCode());
        }
        var keys1Of8 = new[] {
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From1(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From2(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From3(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From4(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From5(5)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From6(5U)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From7(5L)),
            UnionCollectionKey<sbyte, NumberKeyCategory, byte, NumberKeyCategory, short, NumberKeyCategory, ushort, NumberKeyCategory, int, NumberKeyCategory, uint, NumberKeyCategory, long, NumberKeyCategory, ulong, NumberKeyCategory>.Create(Union<sbyte, byte, short, ushort, int, uint, long, ulong>.From8(5UL))
        };
        foreach (var key in keys1Of8)
        {
            Assert.True(keys1Of8[0].Equals(key) && key.Equals(keys1Of8[0]));
            Assert.Equal(keys1Of8[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of2 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory>.Create(Union<float, double>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory>.Create(Union<float, double>.From2(5D))
        };
        foreach (var key in keys2Of2)
        {
            Assert.True(keys2Of2[0].Equals(key) && key.Equals(keys2Of2[0]));
            Assert.Equal(keys2Of2[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of3 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory>.Create(Union<float, double, Half>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory>.Create(Union<float, double, Half>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory>.Create(Union<float, double, Half>.From3((Half)5))
        };
        foreach (var key in keys2Of3)
        {
            Assert.True(keys2Of3[0].Equals(key) && key.Equals(keys2Of3[0]));
            Assert.Equal(keys2Of3[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of4 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory>.Create(Union<float, double, Half, decimal>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory>.Create(Union<float, double, Half, decimal>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory>.Create(Union<float, double, Half, decimal>.From3((Half)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory>.Create(Union<float, double, Half, decimal>.From4(5M))
        };
        foreach (var key in keys2Of4)
        {
            Assert.True(keys2Of4[0].Equals(key) && key.Equals(keys2Of4[0]));
            Assert.Equal(keys2Of4[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of5 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128>.From3((Half)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128>.From4(5M)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128>.From5((Int128)5))
        };
        foreach (var key in keys2Of5)
        {
            Assert.True(keys2Of5[0].Equals(key) && key.Equals(keys2Of5[0]));
            Assert.Equal(keys2Of5[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of6 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From3((Half)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From4(5M)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From5((Int128)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128>.From6((UInt128)5))
        };
        foreach (var key in keys2Of6)
        {
            Assert.True(keys2Of6[0].Equals(key) && key.Equals(keys2Of6[0]));
            Assert.Equal(keys2Of6[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of7 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From3((Half)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From4(5M)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From5((Int128)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From6((UInt128)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint>.From7((nint)5))
        };
        foreach (var key in keys2Of7)
        {
            Assert.True(keys2Of7[0].Equals(key) && key.Equals(keys2Of7[0]));
            Assert.Equal(keys2Of7[0].GetHashCode(), key.GetHashCode());
        }
        var keys2Of8 = new[] {
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From1(5F)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From2(5D)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From3((Half)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From4(5M)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From5((Int128)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From6((UInt128)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From7((nint)5)),
            UnionCollectionKey<float, NumberKeyCategory, double, NumberKeyCategory, Half, NumberKeyCategory, decimal, NumberKeyCategory, Int128, NumberKeyCategory, UInt128, NumberKeyCategory, nint, NumberKeyCategory, nuint, NumberKeyCategory>.Create(Union<float, double, Half, decimal, Int128, UInt128, nint, nuint>.From8((nuint)5))
        };
        foreach (var key in keys2Of8)
        {
            Assert.True(keys2Of8[0].Equals(key) && key.Equals(keys2Of8[0]));
            Assert.Equal(keys2Of8[0].GetHashCode(), key.GetHashCode());
        }
    }

    [Fact]
    public void MixedUnionKeysKeepPrimitiveValuesAndReferenceIdentity()
    {
        var shared = new DerivedKey(1);
        var same = UnionCollectionKey<ProjectKey, ObjectKeyCategory, DerivedKey, ObjectKeyCategory>.Create(Union<ProjectKey, DerivedKey>.From1(shared));
        var cross = UnionCollectionKey<ProjectKey, ObjectKeyCategory, DerivedKey, ObjectKeyCategory>.Create(Union<ProjectKey, DerivedKey>.From2(shared));
        var unrelated = UnionCollectionKey<ProjectKey, ObjectKeyCategory, DerivedKey, ObjectKeyCategory>.Create(Union<ProjectKey, DerivedKey>.From2(new DerivedKey(1)));
        Assert.True(same.Equals(cross) && cross.Equals(same));
        Assert.Equal(same.GetHashCode(), cross.GetHashCode());
        Assert.False(same.Equals(unrelated));
        var set = new Set<Union<string, int>, UnionCollectionKey<string, StringKeyCategory, int, NumberKeyCategory>>();
        set.add(Union<string, int>.From1("1")).add(Union<string, int>.From2(1));
        Assert.Equal(2, set.size);
        Assert.True(set.has(Union<string, int>.From1("1")));
        Assert.True(set.has(Union<string, int>.From2(1)));
    }

    [Fact]
    public void OptionalUnionKeysKeepOneAbsenceAndNativeZeroAllocation()
    {
        var signed = Union<int, uint>.From1(17);
        var unsigned = Union<int, uint>.From2(17U);
        var set = new Set<Union<int, uint>?, OptionalCollectionKey<Union<int, uint>, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>>>();
        set.add(null).add(signed).add(unsigned);
        Assert.Equal(2, set.size);
        Assert.Null(set.First());
        for (var index = 0; index < 4096; index++)
        {
            set.has(unsigned);
            set.has(null);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var matched = 0;
        for (var index = 0; index < 65536; index++)
        {
            if (set.has(unsigned)) matched++;
            if (set.has(null)) matched++;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(131072, matched);
        Assert.Equal(0, allocated);
        Assert.Throws<InvalidOperationException>(() =>
            OptionalCollectionKey<Union<int, uint>, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>>.Create(default(Union<int, uint>)).GetHashCode());
    }

    private class ProjectKey
    {
        internal ProjectKey(int value) { Value = value; }
        private int Value { get; }
        public override bool Equals(object? other) => other is ProjectKey key && key.Value == Value;
        public override int GetHashCode() => Value;
    }

    private sealed class DerivedKey : ProjectKey
    {
        internal DerivedKey(int value) : base(value) { }
    }

    [Fact]
    public void SetAndMapUseOneExactCrossArmNumericKey()
    {
        var first = Union<int, uint, double>.From1(1);
        var second = Union<int, uint, double>.From2(1);
        var third = Union<int, uint, double>.From3(1);
        var set = new Set<Union<int, uint, double>, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory, double, NumberKeyCategory>>(new[] { first, second, third });
        Assert.Equal(1, set.size);
        Assert.True(set.has(second));
        Assert.True(set.has(third));
        Assert.True(set.Single().Is1());
        var map = new Map<Union<int, uint, double>, string, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory, double, NumberKeyCategory>>();
        map.set(first, "first");
        map.set(second, "second");
        Assert.Equal(1, map.size);
        Assert.True(map.keys().Single().Is1());
        Assert.True(map.tryGet(third, out var value));
        Assert.Equal("second", value);
        Assert.True(set.delete(third));
        Assert.Equal(0, set.size);
    }

    [Fact]
    public void NativeIntegerMagnitudeNeverRoundsThroughDouble()
    {
        var exact = Union<long, ulong, double>.From1(9_007_199_254_740_993L);
        var same = Union<long, ulong, double>.From2(9_007_199_254_740_993UL);
        var rounded = Union<long, ulong, double>.From3(9_007_199_254_740_992D);
        var set = new Set<Union<long, ulong, double>, UnionCollectionKey<long, BigIntKeyCategory, ulong, BigIntKeyCategory, double, NumberKeyCategory>>(new[] { exact, same, rounded });
        Assert.Equal(2, set.size);
        Assert.True(set.has(exact));
        Assert.True(set.has(rounded));
        Assert.Equal(9_007_199_254_740_993L, set.First().As1());
        var maximum = UnionCollectionKey<UInt128, BigIntKeyCategory, Int128, BigIntKeyCategory>.Create(Union<UInt128, Int128>.From1(UInt128.MaxValue));
        var minimum = UnionCollectionKey<UInt128, BigIntKeyCategory, Int128, BigIntKeyCategory>.Create(Union<UInt128, Int128>.From2(Int128.MinValue));
        Assert.NotEqual(maximum, minimum);
    }

    [Fact]
    public void FloatDecimalNaNZeroAndInfinityHaveExactNativeKeys()
    {
        var set = new Set<Union<double, float, decimal>, UnionCollectionKey<double, NumberKeyCategory, float, NumberKeyCategory, decimal, NumberKeyCategory>>();
        set.add(Union<double, float, decimal>.From1(-0D));
        set.add(Union<double, float, decimal>.From2(0F));
        set.add(Union<double, float, decimal>.From3(0M));
        set.add(Union<double, float, decimal>.From1(double.NaN));
        set.add(Union<double, float, decimal>.From2(float.NaN));
        set.add(Union<double, float, decimal>.From1(double.PositiveInfinity));
        set.add(Union<double, float, decimal>.From2(float.PositiveInfinity));
        set.add(Union<double, float, decimal>.From1(double.NegativeInfinity));
        set.add(Union<double, float, decimal>.From3(0.5M));
        set.add(Union<double, float, decimal>.From1(0.5D));
        set.add(Union<double, float, decimal>.From3(0.1M));
        set.add(Union<double, float, decimal>.From1(0.1D));
        Assert.Equal(7, set.size);
    }

    [Fact]
    public void NumericUnionEntriesCanonicalizeZeroWithoutChangingTheOriginalArm()
    {
        var inputs = new[] { Union<double, float, Half>.From1(-0D),
            Union<double, float, Half>.From2(-0F), Union<double, float, Half>.From3((Half)(-0F)) };
        foreach (var input in inputs)
        {
            var set = new Set<Union<double, float, Half>, UnionCollectionKey<double, NumberKeyCategory, float, NumberKeyCategory, Half, NumberKeyCategory>>(new[] { input });
            var map = new Map<Union<double, float, Half>, int, UnionCollectionKey<double, NumberKeyCategory, float, NumberKeyCategory, Half, NumberKeyCategory>>();
            map.set(input, 1);
            foreach (var retained in new[] { set.Single(), map.keys().Single() })
            {
                Assert.Equal(input.Is1(), retained.Is1());
                Assert.Equal(input.Is2(), retained.Is2());
                Assert.Equal(input.Is3(), retained.Is3());
                if (retained.Is1()) Assert.Equal(0L, BitConverter.DoubleToInt64Bits(retained.As1()));
                if (retained.Is2()) Assert.Equal(0, BitConverter.SingleToInt32Bits(retained.As2()));
                if (retained.Is3()) Assert.Equal((short)0, BitConverter.HalfToInt16Bits(retained.As3()));
            }
        }
    }

    [Fact]
    public void ClosedUnionKeyLookupsDoNotAllocateOrBox()
    {
        var first = Union<int, uint>.From1(17);
        var second = Union<int, uint>.From2(17);
        var set = new Set<Union<int, uint>, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>>(new[] { first });
        for (var index = 0; index < 4096; index++) Assert.True(set.has(second));
        var before = GC.GetAllocatedBytesForCurrentThread();
        var matched = 0;
        for (var index = 0; index < 65536; index++) if (set.has(second)) matched++;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(65536, matched);
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void NumericKeyHashEqualityAndLookupHaveNoAllocatingPhase()
    {
        var first = UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<int, uint>.From1(17));
        var second = UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>.Create(Union<int, uint>.From2(17));
        var set = new Set<Union<int, uint>, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>>(new[] { first.Value });
        var allocations = new long[3];
        var matched = 0;
        for (var phase = 0; phase < allocations.Length; phase++)
        {
            for (var index = 0; index < 4096; index++)
            {
                if (phase == 0) matched ^= second.GetHashCode();
                else if (phase == 1) matched += first.Equals(second) ? 1 : 0;
                else matched += set.has(second.Value) ? 1 : 0;
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 65536; index++)
            {
                if (phase == 0) matched ^= second.GetHashCode();
                else if (phase == 1) matched += first.Equals(second) ? 1 : 0;
                else matched += set.has(second.Value) ? 1 : 0;
            }
            allocations[phase] = GC.GetAllocatedBytesForCurrentThread() - before;
        }
        Console.WriteLine($"Numeric key allocation bytes: hash={allocations[0]}, equality={allocations[1]}, lookup={allocations[2]}, result={matched}");
        Assert.Equal(0, allocations[0]);
        Assert.Equal(0, allocations[1]);
        Assert.Equal(0, allocations[2]);
    }

    [Fact]
    public void ClosedMapKeyLookupsAndUpdatesDoNotAllocateOrBox()
    {
        var original = Union<int, uint>.From1(17);
        var equal = Union<int, uint>.From2(17);
        var map = new Map<Union<int, uint>, int, UnionCollectionKey<int, NumberKeyCategory, uint, NumberKeyCategory>>();
        for (var value = 17; value < 21; value++) map.set(Union<int, uint>.From1(value), value);
        for (var index = 0; index < 4096; index++)
        {
            map.set(equal, index);
            Assert.True(map.tryGet(original, out var actual) && actual == index);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var matched = 0;
        for (var index = 0; index < 65536; index++)
        {
            map.set(equal, index);
            if (map.has(original) && map.tryGet(original, out var actual) && actual == index) matched++;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(65536, matched);
        Assert.Equal(0, allocated);
        Assert.Equal(4, map.size);
        var first = map.entries().First();
        Assert.True(first.key.Is1());
        Assert.Equal(17, first.key.As1());
        Assert.Equal(65535, first.value);
    }

    [Fact]
    public void NumberAndBigIntArmsAreDistinctKeysWhateverTheirCarrier()
    {
        // Q77: 5n and 5 are distinct keys. The arm category decides, not the CLR carrier (a Q1 number may be stored as long).
        var mixed = new Set<Union<long, double>, UnionCollectionKey<long, BigIntKeyCategory, double, NumberKeyCategory>>();
        mixed.add(Union<long, double>.From1(5L)).add(Union<long, double>.From2(5D));
        Assert.Equal(2, mixed.size);
        Assert.True(mixed.has(Union<long, double>.From1(5L)));
        Assert.True(mixed.has(Union<long, double>.From2(5D)));
        var numbers = new Set<Union<long, double>, UnionCollectionKey<long, NumberKeyCategory, double, NumberKeyCategory>>();
        numbers.add(Union<long, double>.From1(5L)).add(Union<long, double>.From2(5D));
        Assert.Equal(1, numbers.size);
        var bigints = new Set<Union<long, Int128>, UnionCollectionKey<long, BigIntKeyCategory, Int128, BigIntKeyCategory>>();
        bigints.add(Union<long, Int128>.From1(5L)).add(Union<long, Int128>.From2((Int128)5));
        Assert.Equal(1, bigints.size);
        var map = new Map<Union<long, double>, string, UnionCollectionKey<long, BigIntKeyCategory, double, NumberKeyCategory>>();
        map.set(Union<long, double>.From1(5L), "bigint");
        map.set(Union<long, double>.From2(5D), "number");
        Assert.Equal(2, map.size);
        Assert.True(map.tryGet(Union<long, double>.From1(5L), out var bigint) && bigint == "bigint");
        Assert.True(map.tryGet(Union<long, double>.From2(5D), out var number) && number == "number");
    }
}
