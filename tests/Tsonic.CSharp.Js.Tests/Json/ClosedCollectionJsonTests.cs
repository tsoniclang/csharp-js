using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class ClosedCollectionJsonTests
{
    [Fact]
    public void ClosedCollectionsSerializeNestedNativeValuesAndOneAbsence()
    {
        var array = new TsArray();
        array.WriteDynamicElement(1, long.MinValue);
        array.WriteDynamicElement(2, ulong.MaxValue);
        var source = new TsObject();
        source.WriteDynamicSlot("items", array);
        source.WriteDynamicSlot("absent", TsValue.undefined());
        var value = TsValue.from(source);
        Assert.Equal("{\"items\":[null,-9223372036854775808,18446744073709551615],\"absent\":null}", JSON.stringify(value));
        source.WriteDynamicSlot("items", true);
        Assert.Equal("{\"items\":true,\"absent\":null}", JSON.stringify(value));
        Assert.Same(source, value.unwrap());
    }

    [Fact]
    public void ClosedCollectionsUseTheExistingReplacerAndFilter()
    {
        var array = new TsArray(new object?[] { 7, 8 });
        var source = new TsObject();
        source.WriteDynamicSlot("items", array);
        source.WriteDynamicSlot("omit", 9);
        var calls = 0;
        JsonReplacer replacer = (key, value) =>
        {
            calls++;
            return key == "0" ? TsValue.from(11) : value;
        };
        Assert.Equal("{\"items\":[11,8],\"omit\":9}", JSON.stringify(TsValue.from(source), replacer));
        Assert.Equal(5, calls);
        Assert.Equal("{\"items\":[7,8]}", JSON.stringify(TsValue.from(source), new[] { "items" }));
        Assert.Equal(7, Assert.IsType<int>(array.ReadDynamicElement(0).unwrap()));
    }

    [Fact]
    public void ClosedCollectionCyclesRetainTheActualBackingIdentity()
    {
        var source = new TsObject();
        source.WriteDynamicSlot("self", source);
        Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.from(source)));
        var array = new TsArray();
        array.WriteDynamicElement(0, array);
        Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.from(array)));
        Assert.Throws<NotSupportedException>(() => JSON.stringify(new { value = 7 }));
        Assert.Throws<NotSupportedException>(() => TsValue.from(new { value = 7 }));
    }
}
