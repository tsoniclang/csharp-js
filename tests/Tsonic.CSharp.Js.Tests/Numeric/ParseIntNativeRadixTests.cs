using System;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class ParseIntNativeRadixTests
{
    [Theory]
    [InlineData(10.9, 10d)]
    [InlineData(2.9, 2d)]
    [InlineData(4294967298d, 2d)]
    [InlineData(double.NaN, 10d)]
    [InlineData(double.PositiveInfinity, 10d)]
    [InlineData(double.NegativeInfinity, 10d)]
    public void FloatingRadixUsesOnlyTheParsingApisExplicitIntegerNormalization(double radix, double expected)
    {
        Assert.Equal(expected, Number.parseInt("10", radix));
        Assert.Equal(expected, Globals.parseInt("10", radix));
        Assert.Equal(expected, Number.parseInt("10", (double?)radix));
        Assert.Equal(expected, Globals.parseInt("10", (double?)radix));
    }

    [Fact]
    public void NativeRadixCarriersRemainExactWithoutFloatingPointRoundTrips()
    {
        Check((byte)2, 2);
        Check((sbyte)2, 2);
        Check((short)2, 2);
        Check((ushort)2, 2);
        Check(2, 2);
        Check(2U, 2);
        Check((nint)2, 2);
        Check((nuint)2, 2);
        Check(9007199254740995L, 3);
        Check(9007199254740995UL, 3);
        Check(((Int128)1 << 100) + 3, 3);
        Check(((UInt128)1 << 100) + 3, 3);
        Check((BigInteger.One << 200) + 3, 3);
        Check(2f, 2);
        Check((Half)2, 2);
        Check(2m, 2);
    }

    [Fact]
    public void NativeAbsenceAndInvalidRadixKeepOneCurrentParsingContract()
    {
        Assert.Equal(16, Number.parseInt("0x10"));
        Assert.Equal(16, Globals.parseInt("0x10", null));
        Assert.Equal(16, Number.parseInt("0x10", (int?)null));
        Assert.Equal(16, Globals.parseInt("0x10", (double?)null));
        Assert.True(double.IsNaN(Number.parseInt("10", 1.9)));
        Assert.True(double.IsNaN(Globals.parseInt("10", -2L)));
        Assert.True(double.IsNaN(Number.parseInt("10", 37)));
        Assert.Equal(long.MinValue, BitConverter.DoubleToInt64Bits(Number.parseInt("-0", 10d)));
    }

    [Fact]
    public void NumericRadixNormalizationAllocatesNoStorage()
    {
        for (var iteration = 0; iteration < 1000; iteration++)
        {
            Globals.parseInt("10", 9007199254740995L);
            Number.parseInt("10", 2.9);
            Number.parseInt("10", (int?)2);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var total = 0d;
        for (var iteration = 0; iteration < 10000; iteration++)
            total += Globals.parseInt("10", 9007199254740995L) + Number.parseInt("10", 2.9) + Number.parseInt("10", (int?)2);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(70000, total);
        Assert.Equal(0, allocated);
    }

    private static void Check<T>(T radix, double expected) where T : struct, INumberBase<T>
    {
        Assert.Equal(expected, Number.parseInt("10", radix));
        Assert.Equal(expected, Globals.parseInt("10", radix));
        Assert.Equal(expected, Number.parseInt("10", (T?)radix));
        Assert.Equal(expected, Globals.parseInt("10", (T?)radix));
    }
}
