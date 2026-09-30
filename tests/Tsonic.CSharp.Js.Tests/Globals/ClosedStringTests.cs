using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class ClosedStringTests
{
    [Fact]
    public void ClosedStringsUseNativeValuesWithoutDiagnosticFormatting()
    {
        Assert.Equal("9007199254740993", Globals.String((object)9007199254740993L));
        Assert.Equal("18446744073709551615", Globals.String((object)ulong.MaxValue));
        Assert.Equal("text\n😀", Globals.String((object)"text\n😀"));
        Assert.Equal("Error", Globals.String((object)new Error()));
        Assert.Equal("Error: failure", Globals.String((object)new Error("failure")));
        var repeated = JSArray<object?>.of(["x", null]);
        var values = JSArray<object?>.of([repeated, repeated, 42]);
        Assert.Equal("x,,x,,42", Globals.String((object)values));
    }

    [Fact]
    public void ClosedStringCyclesRejectWithoutDamagingTheArray()
    {
        var values = new JSArray<object?>();
        values.push(values);
        Assert.Throws<TypeError>(() => Globals.String((object)values));
        Assert.Same(values, values[0]);
        values.Clear();
        Assert.Equal("", Globals.String((object)values));
    }

    [Fact]
    public void ClosedNativeStringConversionDoesNotCopyItsPayload()
    {
        var value = new string('x', 64);
        object boxed = value;
        Globals.String(boxed);
        var before = GC.GetAllocatedBytesForCurrentThread();
        string? result = null;
        for (var index = 0; index < 1000; index++) result = Globals.String(boxed);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Same(value, result);
        Assert.Equal(0, allocated);
    }
}
