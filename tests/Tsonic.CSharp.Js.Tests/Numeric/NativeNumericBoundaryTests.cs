using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeNumericBoundaryTests
{
    [Theory]
    [InlineData("1\0")]
    [InlineData("1\0\0")]
    [InlineData("1\0" + "2")]
    [InlineData("+1\0")]
    [InlineData("1 2")]
    [InlineData("+-1")]
    [InlineData("1e3")]
    public void BigIntParsingRejectsMalformedWholeTokens(string text)
    {
        Assert.Throws<SyntaxError>(() => BigIntOps.from(text));
    }

    [Fact]
    public void NumericFormattingAllocatesOnlyTheResultString()
    {
        Func<string>[] formats = [
            () => 1.25.toFixed(1),
            () => 1.25.toExponential(1),
            () => 1.25.toPrecision(2),
            () => 9007199254740993L.toFixed(100),
            () => UInt128.MaxValue.toPrecision(100)
        ];
        foreach (var format in formats)
        {
            var length = format().Length;
            Assert.Equal(Measure(() => new string('x', length)), Measure(format));
        }
    }

    private static long Measure(Func<string> format)
    {
        for (var iteration = 0; iteration < 1000; iteration++) GC.KeepAlive(format());
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 1000; iteration++) GC.KeepAlive(format());
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void SurfaceLimitsNormalizeWithoutChangingNativeStorageBounds()
    {
        var expression = new RegExp(",");
        foreach (var (limit, length) in new[] { (-1d, 3), (1.5, 1), (double.NaN, 0), (double.PositiveInfinity, 0), (4294967296d, 0), (4294967297d, 1) })
        {
            Assert.Equal(length, "a,b,c".split(",", limit).Count);
            Assert.Equal(length, "a,b,c".split(expression, limit).Count);
        }
        foreach (var invalid in new[] { -1d, double.PositiveInfinity, (double)int.MaxValue + 1 })
        {
            Assert.Throws<RangeError>(() => new ArrayBuffer(invalid));
            Assert.Throws<RangeError>(() => new Uint8Array(invalid));
        }
        Assert.Equal(0, new ArrayBuffer(double.NaN).byteLength);
        Assert.Equal(1, new Uint8Array(1.9).length);
    }

    [Fact]
    public void ExplicitIntegerApiConversionMatchesModuloBits()
    {
        var view = new DataView(new ArrayBuffer(4));
        for (var exponent = -1074; exponent <= 1024; exponent++)
        {
            foreach (var sign in new[] { -1d, 1d })
            {
                var value = sign * double.ScaleB(1.23456789, exponent);
                var remainder = double.IsFinite(value) ? System.Math.Truncate(value) % 4294967296d : 0;
                var expected = (uint)(remainder < 0 ? remainder + 4294967296d : remainder);
                view.setUint32(0, value);
                Assert.Equal(expected, view.getUint32(0));
                Assert.Equal(unchecked((int)expected), Math.imul(value, 1));
                Assert.Equal(System.Numerics.BitOperations.LeadingZeroCount(expected), Math.clz32(value));
            }
        }
        Assert.Equal(1, Math.imul(9007199254740993L, 1));
        Assert.Equal(0, Math.clz32(ulong.MaxValue));
    }

    [Fact]
    public void NativePredicatesAndTextRetainExactCarriers()
    {
        Assert.True(Globals.isFinite(long.MaxValue));
        Assert.True(Globals.isFinite(UInt128.MaxValue));
        Assert.False(Globals.isNaN(long.MinValue));
        Assert.False(Number.isSafeInteger(9007199254740993L));
        Assert.True(Number.isInteger(9007199254740993L));
        Assert.True(Number.isSafeInteger(16777216f));
        Assert.False(Number.isSafeInteger(9007199254740992f));
        Assert.Equal("9007199254740993", 9007199254740993L.toString());
        Assert.Equal("9007199254740993.00", 9007199254740993L.toFixed(2));
        Assert.Equal("-80000000000000000000000000000000", Int128.MinValue.toString(16));
        Assert.Equal("ffffffffffffffffffffffffffffffff", UInt128.MaxValue.toString(16));
        Assert.Equal("1.3e+1", 12.5.toExponential(1));
        Assert.Equal("1.3", 1.25.toPrecision(2));
        Assert.Equal("0", (-0d).toFixed());
        Assert.Equal(0, Globals.Number(""));
        Assert.Equal(16, Globals.Number("0x10"));
        Assert.True(double.IsNaN(Globals.Number("12suffix")));
        Assert.Equal(12, Number.parseFloat("12suffix"));
    }
}
