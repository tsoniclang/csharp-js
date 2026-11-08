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
        var value = TsValue.fromnative(source);
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
            return key == "0" ? TsValue.fromnative(11) : value;
        };
        Assert.Equal("{\"items\":[11,8],\"omit\":9}", JSON.stringify(TsValue.fromnative(source), replacer));
        Assert.Equal(5, calls);
        Assert.Equal("{\"items\":[7,8]}", JSON.stringify(TsValue.fromnative(source), new[] { "items" }));
        Assert.Equal(7, Assert.IsType<int>(array.ReadDynamicElement(0).unwrap()));
    }

    [Fact]
    public void ClosedCollectionCyclesRetainTheActualBackingIdentity()
    {
        var source = new TsObject();
        source.WriteDynamicSlot("self", source);
        Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.fromnative(source)));
        var array = new TsArray();
        array.WriteDynamicElement(0, array);
        Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.fromnative(array)));
        Assert.Throws<NotSupportedException>(() => JSON.stringify(new { value = 7 }));
        Assert.Throws<NotSupportedException>(() => TsValue.fromnative(new { value = 7 }));
    }

    [Fact]
    public void RootReplacerDeterminesPresenceBeforeAnyChildTraversal()
    {
        var record = new TsObject();
        record.WriteDynamicSlot("value", 7L);
        var calls = 0;
        JsonReplacer removeRoot = (key, value) =>
        {
            calls++;
            Assert.Equal(string.Empty, key);
            Assert.Same(record, value.unwrap());
            return TsValue.undefined();
        };
        Assert.Null(JSON.stringify(TsValue.fromnative(record), removeRoot, TsValue.fromnative(" ")));
        Assert.Equal(1, calls);
        Assert.Equal(7L, record.ReadDynamicSlotAs<long>("value"));
        calls = 0;
        JsonReplacer replaceAbsence = (key, value) =>
        {
            calls++;
            Assert.Equal(string.Empty, key);
            Assert.True(value.isUndefined());
            return TsValue.fromsource<long, SourceNumberCategory<long>>(7);
        };
        Assert.Equal("7", JSON.stringify(TsValue.undefined(), replaceAbsence));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void ReplacerObservesOriginalCategoriesOnRootRecordAndArrayComponents()
    {
        var number = TsValue.fromsource<long, SourceNumberCategory<long>>(7);
        var integer = TsValue.fromsource<long, SourceBigIntCategory<long>>(7);
        var record = new TsObject();
        record.WriteDynamicSlot("number", number);
        record.WriteDynamicSlot("integer", integer);
        var array = new TsArray();
        array.WriteDynamicElement(0, number);
        array.WriteDynamicElement(1, integer);
        var seen = new System.Collections.Generic.List<SourceValueCategory>();
        JsonReplacer replacer = (key, value) =>
        {
            seen.Add(value.Category);
            return value;
        };
        Assert.Equal("7", JSON.stringify(number, replacer));
        Assert.Equal(new[] { SourceValueCategory.Number }, seen);
        seen.Clear();
        Assert.Equal("{\"number\":7,\"integer\":7}", JSON.stringify(TsValue.fromnative(record), replacer));
        Assert.Equal(new[] { SourceValueCategory.Object, SourceValueCategory.Number, SourceValueCategory.BigInt }, seen);
        seen.Clear();
        Assert.Equal("[7,7]", JSON.stringify(TsValue.fromnative(array), replacer));
        Assert.Equal(new[] { SourceValueCategory.Object, SourceValueCategory.Number, SourceValueCategory.BigInt }, seen);
    }
}
