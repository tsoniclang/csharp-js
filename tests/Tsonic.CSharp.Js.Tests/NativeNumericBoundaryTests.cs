using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeNumericBoundaryTests
{
    [Fact]
    public void SplitAndStorageSizesRejectNonNativeLengths()
    {
        var expression = new RegExp(",");
        foreach (var invalid in new[] { -1d, 1.5, double.NaN, double.PositiveInfinity, (double)int.MaxValue + 1 })
        {
            Assert.Throws<RangeError>(() => "a,b,c".split(",", invalid));
            Assert.Throws<RangeError>(() => "a,b,c".split(expression, invalid));
            Assert.Throws<RangeError>(() => new ArrayBuffer(invalid));
            Assert.Throws<RangeError>(() => new Uint8Array(invalid));
        }
        Assert.Equal(3, "a,b,c".split(expression, int.MaxValue).Count);
    }

    [Fact]
    public void IntegerElementsUseCheckedNativeClrConversions()
    {
        foreach (var value in new[] { 0d, 1.9, 127d })
        {
            Assert.Equal(checked((sbyte)value), new Int8Array(new[] { value })[0]);
            Assert.Equal(checked((byte)value), new Uint8Array(new[] { value })[0]);
            Assert.Equal(checked((short)value), new Int16Array(new[] { value })[0]);
            Assert.Equal(checked((ushort)value), new Uint16Array(new[] { value })[0]);
            Assert.Equal(checked((int)value), new Int32Array(new[] { value })[0]);
            Assert.Equal(checked((uint)value), new Uint32Array(new[] { value })[0]);
        }
        foreach (var value in new[] { -1d, 256d, double.NaN, double.PositiveInfinity })
            Assert.Throws<OverflowException>(() => new Uint8Array(new[] { value }));
    }

    [Fact]
    public void GlobalPredicatesRetainExactNativeIntegerCarriers()
    {
        Assert.True(Globals.isFinite(long.MaxValue));
        Assert.True(Globals.isFinite(UInt128.MaxValue));
        Assert.False(Globals.isNaN(long.MinValue));
        Assert.False(Globals.isNaN(UInt128.MaxValue));
        Assert.True(Globals.isNaN(double.NaN));
        Assert.False(Globals.isFinite(double.PositiveInfinity));
    }
}
