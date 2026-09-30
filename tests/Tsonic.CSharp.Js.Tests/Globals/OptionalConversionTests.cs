using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class OptionalConversionTests
{
    [Fact]
    public void OptionalConversionsRetainNativeValuesAndAbsence()
    {
        Assert.Equal("9007199254740993", Globals.String((long?)9007199254740993));
        Assert.Equal("null", Globals.String((long?)null));
        Assert.Equal("false", Globals.String((bool?)false));
        Assert.Equal("null", Globals.String((bool?)null));
        Assert.Equal(0, Globals.Number((ulong?)null));
        Assert.Equal((double)ulong.MaxValue, Globals.Number((ulong?)ulong.MaxValue));
        Assert.Equal(0, Globals.Number((bool?)false));
        Assert.Equal(1, Globals.Number((bool?)true));
        Assert.Equal(0, Globals.Number((bool?)null));
        Assert.True(double.IsNaN(Globals.Number((double?)double.NaN)));
        Assert.Equal(double.NegativeInfinity, 1 / Globals.Number((double?)(-0.0)));
    }

    [Fact]
    public void OptionalConversionsAllocateOnlyTheRequestedString()
    {
        for (var index = 0; index < 1000; index++)
        {
            Format(index);
            FormatNative(index);
            Convert(index);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var sum = 0.0;
        for (var index = 0; index < 1000; index++) sum += Convert(index);
        var converted = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(499500, sum);
        Assert.Equal(0, converted);
        before = GC.GetAllocatedBytesForCurrentThread();
        var length = 0;
        for (var index = 0; index < 1000; index++) length += Format(9007199254740993).Length;
        var formatted = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        var nativeLength = 0;
        for (var index = 0; index < 1000; index++) nativeLength += FormatNative(9007199254740993).Length;
        var native = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(nativeLength, length);
        Assert.Equal(native, formatted);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double Convert(long? value) => Globals.Number(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string Format(long? value) => Globals.String(value);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string FormatNative(long? value) => value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "null";
}
