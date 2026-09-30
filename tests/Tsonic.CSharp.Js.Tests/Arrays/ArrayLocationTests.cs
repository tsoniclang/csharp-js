using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class ArrayLocationTests
{
    [Fact]
    public void IndexedLocationsRetainAliasesAbsenceNativeWidthAndResizing()
    {
        var values = new JSArray<long?>(new long?[] { 9007199254740993L });
        var location = values.elementLocation(0);
        var alias = values.elementLocation(-0.0);
        Assert.True(Location<long?>.Same(location, alias));
        Assert.Equal(Location<long?>.Hash(location), Location<long?>.Hash(alias));
        Assert.Equal(9007199254740993L, location.Load());
        location.Store(null);
        Assert.Null(values[0]);
        Assert.Null(alias.Load());
        values[0] = 9007199254740995L;
        Assert.Equal(9007199254740995L, alias.Load());
        values.Clear();
        Assert.Throws<IndexOutOfRangeException>(() => location.Load());
        location.Store(7);
        Assert.Equal(7L, values[0]);
        Assert.False(Location<long?>.Same(location, values.elementLocation(1)));
        Assert.False(Location<long?>.Same(location, new JSArray<long?>(values).elementLocation(0)));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void NumericPropertyLocationsUseTheirSelectedNativeKey(double index)
    {
        var values = new JSArray<long>();
        var location = values.elementLocation(index);
        var alias = values.elementLocation(index);
        Assert.Throws<IndexOutOfRangeException>(() => location.Load());
        location.Store(9007199254740993L);
        Assert.Equal(9007199254740993L, alias.Load());
        Assert.Equal(9007199254740993L, values[index]);
        Assert.True(Location<long>.Same(location, alias));
        Assert.Equal(Location<long>.Hash(location), Location<long>.Hash(alias));
    }
}
