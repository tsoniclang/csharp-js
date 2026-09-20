using System;
using System.Linq;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class JSArrayNumericPropertyTests
{
    [Fact]
    public void NumericPropertiesDoNotTruncateOrChangeLength()
    {
        var values = new JSArray<double>(new[] { 2d });
        values[-1] = 3;
        values[1.5] = 4;
        values[double.NaN] = 5;
        values[double.PositiveInfinity] = 6;
        values[4294967295d] = 7;
        values[-0d] = 8;
        Assert.Equal(1, values.length);
        Assert.Equal(8, values[0]);
        Assert.Equal(3, values[-1d]);
        Assert.Equal(4, values[1.5]);
        Assert.Equal(5, values[double.NaN]);
        Assert.Equal(6, values[double.PositiveInfinity]);
        Assert.Equal(7, values[4294967295d]);
        Assert.False(values.hasIndex(1));
        Assert.True(ArrayLike.HasIndex(-1, values));
        Assert.True(ArrayLike.HasIndex(double.NaN, values));
        Assert.Equal(3, ArrayLike.ReadNumber(values, -1));
        Assert.Equal(4, ArrayLike.ReadNumber(values, 1.5));
        Assert.Equal(5, ArrayLike.ReadNumber(values, double.NaN));
        Assert.Null(ArrayLike.ReadNumber(values, 1));
        Assert.Equal(new[] { "0", "-1", "1.5", "NaN", "Infinity", "4294967295" }, Object.keys(values).ToArray());
        Assert.True(Object.hasOwn(values, "length"));
        Assert.False(Object.hasOwn(values, "-0"));
        Assert.False(Object.hasOwn(values, "1.50"));
    }

    [Fact]
    public void AliasesAndClosedReadsShareOnePropertyStore()
    {
        var values = new JSArray<double?>();
        var alias = values;
        var closed = TsValue.from(values);
        values[1.5] = 9;
        Assert.Equal(9d, closed.ReadDynamicSlot("1.5").unwrap());
        closed.WriteDynamicSlot("-2", 6d);
        Assert.Equal(6, alias[-2]);
        values[double.NaN] = null;
        Assert.Null(closed.ReadDynamicSlot("NaN").unwrap());
        Assert.False(closed.ReadDynamicSlot("NaN").isUndefined());
        Assert.True(values.deleteAt(double.NaN));
        Assert.True(closed.ReadDynamicSlot("NaN").isUndefined());
        Assert.True(values.deleteAt(1.5));
        values[1.5] = 10;
        Assert.Equal(new[] { "-2", "1.5" }, Object.keys(alias).ToArray());
        values[0] = 1;
        values.setLength(0d);
        Assert.Equal(10, alias[1.5]);
        Assert.Equal(0, values.length);
        Assert.False(values.hasIndex(0));
    }

    [Fact]
    public void ClosedWritesUseTheSameLengthAndNumericKeyContract()
    {
        var values = new JSArray<double>(3);
        var closed = TsValue.from(values);
        Assert.Throws<TypeError>(() => closed.WriteDynamicSlot("length", 4d));
        Assert.Equal(3, values.length);
        Assert.Equal(0d, closed.ReadDynamicSlot("1").unwrap());
        closed.WriteDynamicSlot("1", 4d);
        Assert.Equal(4, values[1]);
        closed.WriteDynamicSlot("1.5", 9d);
        closed.WriteDynamicSlot("length", 0d);
        Assert.Equal(0, values.length);
        Assert.Equal(9, values[1.5]);
        Assert.Throws<RangeError>(() => closed.WriteDynamicSlot("length", 1.5));
        Assert.Throws<TypeError>(() => ((IDynamicObject)values).WriteDynamicSlot("length", System.Numerics.BigInteger.One));
        Assert.Throws<NotSupportedException>(() => closed.WriteDynamicSlot("length", System.Numerics.BigInteger.One));
        Assert.Throws<TypeError>(() => closed.WriteDynamicSlot("0", "wrong carrier"));
        Assert.Equal(0, values.length);
    }

    [Fact]
    public void NativeStorageAndLengthBoundariesRejectWithoutTruncation()
    {
        var values = new JSArray<int>();
        Assert.Throws<RangeError>(() => values[4294967294d] = 1);
        Assert.Throws<RangeError>(() => values.setLength(1.5));
        Assert.Throws<RangeError>(() => values.setLength(double.NaN));
        Assert.Equal(0, values.length);
        Assert.False(values.hasIndex(4294967294d));
        Assert.True(values.deleteAt(4294967294d));
    }
}
