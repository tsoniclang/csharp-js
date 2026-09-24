using System;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeNumericCollectionTests
{
    [Fact]
    public void NativeIntegerSearchRetainsAllBitsWithoutBoxing()
    {
        var values = new JSArray<long>(new[] { 9007199254740992L, 9007199254740993L });
        for (var index = 0; index < 100; index++) Assert.Equal(1, values.indexOf(9007199254740993L));
        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = 0;
        for (var index = 0; index < 1000; index++) total += values.indexOf(9007199254740993L);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(1000, total);
        Assert.Equal(0, allocated);
        Assert.Equal(0, values.indexOf(9007199254740992L));
        Assert.Equal(-1, values.indexOf(9007199254740994L));
    }

    [Fact]
    public void BoxedNativeKeysCompareExactValuesWithoutFloatingRounding()
    {
        var values = new Set<object>();
        var inputs = new object[] { 9007199254740992L, 9007199254740993L, ulong.MaxValue, Int128.MinValue,
            UInt128.MaxValue, BigInteger.One << 200, 1, 1L, 1.0, -0.0, double.NaN };
        foreach (var input in inputs) values.add(input);
        Assert.Equal(inputs.Length - 2, values.size);
        foreach (var input in inputs) Assert.True(values.has(input));
        Assert.False(values.has(9007199254740994L));
        Assert.True(values.has(1f));
        Assert.Equal(inputs.Length - 2, values.add(double.NaN).add(0.0).size);
        Assert.Contains(values.values(), value => value is double number && BitConverter.DoubleToInt64Bits(number) == 0);
    }

    [Fact]
    public void NumericConstructionAvoidsNativeOperandBoxing()
    {
        for (var index = 0; index < 100; index++) { _ = Globals.Number(1L); _ = BigIntOps.from(1L); }
        var before = GC.GetAllocatedBytesForCurrentThread();
        double numeric = 0;
        BigInteger integer = 0;
        for (var index = 0; index < 1000; index++) { numeric = Globals.Number(1L); integer = BigIntOps.from(1L); }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(1, numeric);
        Assert.Equal(BigInteger.One, integer);
        Assert.Equal(0, allocated);
    }
}
