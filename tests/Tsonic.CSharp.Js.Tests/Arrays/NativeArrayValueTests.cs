using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class NativeArrayValueTests
{
    [Fact]
    public void TypedErasedArraysRetainNativeBackingIdentityAndMutation()
    {
        var backing = JSArray<string>.of(["first"]);
        var erased = TsValue.from(backing);
        var copied = erased;
        Assert.True(erased.IsArray());
        Assert.Same(backing, TsValue.CastDynamic<JSArray<string>>(copied));
        TsValue.CastDynamic<JSArray<string>>(copied).push("second");
        Assert.Equal(2, backing.length);
        Assert.Equal("second", backing[1]);
        Assert.Equal(2, erased.ArrayLength);
        Assert.Equal("first,second", Globals.String(erased));
        Assert.Equal("[\"first\",\"second\"]", JSON.stringify(erased));
        var different = TsValue.from(JSArray<TsValue>.of([TsValue.from("first")]));
        Assert.Throws<TypeError>(() => TsValue.CastDynamic<JSArray<string>>(different));
    }

    [Fact]
    public void ErasureRecoveryAndLengthHaveNoAdditionalAllocation()
    {
        var backing = JSArray<string>.of(["first"]);
        for (var index = 0; index < 1000; index++) TsValue.CastDynamic<JSArray<string>>(TsValue.from(backing));
        var before = GC.GetAllocatedBytesForCurrentThread();
        var matches = 0;
        for (var index = 0; index < 1000; index++)
        {
            var erased = TsValue.from(backing);
            if (erased.IsArray() && erased.ArrayLength == 1 && ReferenceEquals(backing, TsValue.CastDynamic<JSArray<string>>(erased))) matches++;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(1000, matches);
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void NativeUnionElementsRetainNestedArraysAbsenceAndExactWidths()
    {
        var nested = JSArray<string>.of(["first"]);
        var values = JSArray<Union<JSArray<string>, ulong, string?>>.of([
            Union<JSArray<string>, ulong, string?>.From1(nested),
            Union<JSArray<string>, ulong, string?>.From2(ulong.MaxValue),
            Union<JSArray<string>, ulong, string?>.From3(null),
        ]);
        var erased = TsValue.from(values);
        var selected = erased.ReadDynamicElement(0);
        Assert.True(selected.IsArray());
        Assert.Same(nested, TsValue.CastDynamic<JSArray<string>>(selected));
        nested.push("second");
        Assert.Equal(2, selected.ArrayLength);
        Assert.Equal(ulong.MaxValue, TsValue.CastDynamic<ulong>(erased.ReadDynamicElement(1)));
        Assert.True(erased.ReadDynamicElement(2).isUndefined());
        Assert.Equal("first,second,18446744073709551615,", Globals.String(erased));
        Assert.Equal("[[\"first\",\"second\"],18446744073709551615,null]", JSON.stringify(erased));
    }
}
