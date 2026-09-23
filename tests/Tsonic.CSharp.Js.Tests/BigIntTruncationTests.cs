using System;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class BigIntTruncationTests
{
    [Theory]
    [InlineData(0, "-17", "0", "0")]
    [InlineData(1, "3", "-1", "1")]
    [InlineData(7, "64", "-64", "64")]
    [InlineData(8, "-129", "127", "127")]
    [InlineData(9, "256", "-256", "256")]
    [InlineData(64, "18446744073709551615", "-1", "18446744073709551615")]
    [InlineData(64, "-9223372036854775809", "9223372036854775807", "9223372036854775807")]
    [InlineData(64, "9007199254740993", "9007199254740993", "9007199254740993")]
    public void ExactWidthsRetainAllBits(double bits, string source, string signed, string unsigned)
    {
        var value = BigInteger.Parse(source);
        Assert.Equal(BigInteger.Parse(signed), BigIntOps.asIntN(bits, value));
        Assert.Equal(BigInteger.Parse(unsigned), BigIntOps.asUintN(bits, value));
        Assert.Equal(BigInteger.Parse(source), value);
    }

    [Fact]
    public void NativeInputsUseTheSameExactContract()
    {
        Assert.Equal(-1, BigIntOps.asIntN(8, byte.MaxValue));
        Assert.Equal(255, BigIntOps.asUintN(8, (sbyte)-1));
        Assert.Equal(-1, BigIntOps.asIntN(16, ushort.MaxValue));
        Assert.Equal(65535, BigIntOps.asUintN(16, (short)-1));
        Assert.Equal(-1, BigIntOps.asIntN(32, uint.MaxValue));
        Assert.Equal(uint.MaxValue, BigIntOps.asUintN(32, -1));
        Assert.Equal(-1, BigIntOps.asIntN(64, ulong.MaxValue));
        Assert.Equal(new BigInteger(ulong.MaxValue), BigIntOps.asUintN(64, -1L));
        Assert.Equal(-1, BigIntOps.asIntN(128, UInt128.MaxValue));
        Assert.Equal(BigInteger.CreateChecked(UInt128.MaxValue), BigIntOps.asUintN(128, (Int128)(-1)));
        Assert.Equal(-1, BigIntOps.asIntN(256, (nint)(-1)));
        Assert.Equal(42, BigIntOps.asUintN(256, (nuint)42));
        Assert.Equal((BigInteger.One << 129) - 1, BigIntOps.asUintN(129, -1L));
    }

    [Fact]
    public void WideResultsRemainArbitraryPrecision()
    {
        var value = BigInteger.One << 200;
        Assert.Equal(-value, BigIntOps.asIntN(201, value));
        Assert.Equal(value, BigIntOps.asUintN(202, value));
        Assert.Equal(-1, BigIntOps.asIntN(200, -1));
        Assert.Equal(BigInteger.One << 64, BigIntOps.asUintN(64, ulong.MaxValue) + 1);
        Assert.Equal(9, BigIntOps.asIntN(9007199254740992d, 9));
        Assert.Equal(9, BigIntOps.asUintN(9007199254740992d, 9));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.0)]
    public void ZeroWidthProducesZero(double bits)
    {
        Assert.Equal(BigInteger.Zero, BigIntOps.asIntN(bits, 9));
        Assert.Equal(BigInteger.Zero, BigIntOps.asUintN(bits, 9));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(-0.5)]
    [InlineData(1.9)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(18446744073709551616d)]
    public void InvalidWidthsRejectBeforeTruncation(double bits)
    {
        Assert.Throws<RangeError>(() => BigIntOps.asIntN(bits, 9));
        Assert.Throws<RangeError>(() => BigIntOps.asUintN(bits, 9));
    }

    [Fact]
    public void NativeResultsRemainFixedWidthAndExact()
    {
        Assert.Equal((Int128)9007199254740993L, BigIntOps.AsIntNative(64, 9007199254740993L));
        Assert.Equal((UInt128)ulong.MaxValue, BigIntOps.AsUintNative(64, -1L));
        Assert.Equal(Int128.MinValue, BigIntOps.AsIntNative(128, (UInt128)1 << 127));
        Assert.Equal(UInt128.MaxValue, BigIntOps.AsUintNative(128, (Int128)(-1)));
        Assert.Throws<RangeError>(() => BigIntOps.AsIntNative(129, 0));
        Assert.Throws<RangeError>(() => BigIntOps.AsUintNative(double.NaN, 0));
        Assert.Throws<RangeError>(() => BigIntOps.AsIntNative(1.5, 0));
    }
}
