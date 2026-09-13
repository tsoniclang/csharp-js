using System;
using System.Numerics;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class BigIntConstructionTests
{
    [Fact]
    public void IntegerInputsRetainAllBits()
    {
        Assert.Equal(BigInteger.Parse("-9007199254740993"), BigIntOps.from(-9007199254740993L));
        Assert.Equal(BigInteger.Parse("18446744073709551615"), BigIntOps.from(ulong.MaxValue));
        Assert.Equal(BigInteger.Parse("170141183460469231731687303715884105727"), BigIntOps.from(Int128.MaxValue));
        Assert.Equal(BigInteger.Parse("340282366920938463463374607431768211455"), BigIntOps.from(UInt128.MaxValue));
        Assert.Equal(BigInteger.One << 200, BigIntOps.from(BigInteger.One << 200));
        Assert.Equal(new BigInteger(42), BigIntOps.from((nuint)42));
        Assert.Equal(new BigInteger(-42), BigIntOps.from((nint)(-42)));
        Assert.Equal(BigInteger.One, BigIntOps.from(true));
        Assert.Equal(BigInteger.Zero, BigIntOps.from(false));
    }

    [Theory]
    [InlineData("", "0")]
    [InlineData("\ufeff\u2000\r\n", "0")]
    [InlineData("\u00a0-00042\u3000", "-42")]
    [InlineData("+42", "42")]
    [InlineData("0Xff", "255")]
    [InlineData("0o77", "63")]
    [InlineData("0b101", "5")]
    [InlineData("9007199254740993", "9007199254740993")]
    public void IntegerStringGrammarIsExact(string source, string expected)
    {
        Assert.Equal(BigInteger.Parse(expected), BigIntOps.from(source));
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("1e3")]
    [InlineData("0x")]
    [InlineData("+0x1")]
    [InlineData("-0b1")]
    [InlineData("1n")]
    [InlineData("1_000")]
    [InlineData("\u00851")]
    [InlineData("Infinity")]
    public void InvalidStringsThrowSyntaxError(string source)
    {
        Assert.Equal("SyntaxError", Assert.Throws<SyntaxError>(() => BigIntOps.from(source)).name);
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonintegerNumbersThrowRangeError(double source)
    {
        Assert.Throws<RangeError>(() => BigIntOps.from(source));
    }

    [Fact]
    public void ClosedUnionDispatchPreservesTheSelectedAlternative()
    {
        var large = BigInteger.Parse("9007199254740993");
        Assert.Equal(large, BigIntOps.from(Union<double, BigInteger>.From2(large)));
        Assert.Equal(new BigInteger(2), BigIntOps.from(Union<double, BigInteger>.From1(2)));
        Assert.Throws<RangeError>(() => BigIntOps.from(Union<double, BigInteger>.From1(2.5)));
        Assert.Equal(9007199254740992d, Globals.Number(large));
        Assert.Equal(9007199254740992d, Globals.Number(Union<double, BigInteger>.From2(large)));
        Assert.Equal(2d, Globals.Number(Union<double, BigInteger>.From1(2)));
        Assert.Equal(BigInteger.Zero, BigIntOps.from(-0d));
        Assert.Throws<TypeError>(() => BigIntOps.from(new object()));
    }
}
