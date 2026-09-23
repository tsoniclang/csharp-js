using System;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeBinaryIndexTests
{
    [Fact]
    public void TypedElementsRetainEveryNativeIndexWidth()
    {
        CheckTypedIndex((sbyte)1);
        CheckTypedIndex((byte)1);
        CheckTypedIndex((short)1);
        CheckTypedIndex((ushort)1);
        CheckTypedIndex(1);
        CheckTypedIndex(1U);
        CheckTypedIndex(1L);
        CheckTypedIndex(1UL);
        CheckTypedIndex((nint)1);
        CheckTypedIndex((nuint)1);
        CheckTypedIndex((Int128)1);
        CheckTypedIndex((UInt128)1);
        CheckTypedIndex(BigInteger.One);
        CheckTypedIndex(1.0);
        CheckTypedIndex(1.0f);
    }

    [Fact]
    public void InvalidTypedIndexesNeverRoundIntoExistingStorage()
    {
        var values = new Uint32Array(2);
        values.Set(1, 71U);
        foreach (var index in new[] { -1.0, 0.5, double.NaN, double.PositiveInfinity })
        {
            Assert.Equal(0U, values.Get(index));
            Assert.Equal(5U, values.Set(index, 5U));
        }
        Assert.Equal(0U, values.Get(9007199254740993UL));
        Assert.Equal(0U, values.Get(UInt128.MaxValue));
        Assert.Equal(0U, values.Get(Int128.MinValue));
        Assert.Equal(0U, values.Get(BigInteger.One << 256));
        Assert.Equal(71U, values.Get(1));
        Assert.Equal(0U, values.Get(0));
        values.Set(-0.0, 19U);
        Assert.Equal(19U, values.Get(0));
        Assert.Equal(19U, values.Get(-0.0f));
    }

    [Fact]
    public void DataViewNativeIndexesPreserveRangeAndFractionRules()
    {
        var view = new DataView(new ArrayBuffer(16));
        view.setUint32((nuint)0, uint.MaxValue, true);
        Assert.Equal(uint.MaxValue, view.getUint32(0UL, true));
        view.setInt16((UInt128)4, -17, true);
        Assert.Equal((short)-17, view.getInt16(BigInteger.One * 4, true));
        view.setFloat32(8U, 1.25, true);
        Assert.Equal(1.25f, view.getFloat32((nint)8, true));
        view.setUint8(12.75, 31);
        Assert.Equal((byte)31, view.getUint8(12));
        Assert.Throws<RangeError>(() => view.getUint8(9007199254740993UL));
        Assert.Throws<RangeError>(() => view.setUint8(UInt128.MaxValue, 3));
        Assert.Throws<RangeError>(() => view.getUint8(Int128.MinValue));
        Assert.Throws<RangeError>(() => view.getUint8(double.NaN));
        Assert.Throws<RangeError>(() => view.getUint8(double.PositiveInfinity));
        Assert.Throws<RangeError>(() => view.getUint32(14L));
    }

    private static void CheckTypedIndex<TIndex>(TIndex index) where TIndex : INumberBase<TIndex>
    {
        var values = new Uint32Array(2);
        Assert.Equal(uint.MaxValue, values.Set(index, uint.MaxValue));
        Assert.Equal(uint.MaxValue, values.Get(index));
        Assert.Equal(uint.MaxValue, values.Update<uint, TIndex>(index, true, false));
        Assert.Equal(0U, values.Get(index));
    }
}
