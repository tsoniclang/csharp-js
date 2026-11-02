using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeSameValueTests
{
    [Fact]
    public void SameValuePreservesEveryNativeNumericDomainWithoutDoubleRounding()
    {
        Check(9007199254740993L, 9007199254740992L, false, "int64 above binary precision");
        Check(9007199254740993UL, 9007199254740992UL, false, "uint64 above binary precision");
        Check(long.MaxValue, long.MaxValue - 1, false, "int64 maximum neighbors");
        Check(long.MinValue, long.MinValue + 1, false, "int64 minimum neighbors");
        Check(ulong.MaxValue, ulong.MaxValue - 1, false, "uint64 maximum neighbors");
        Check(9007199254740993m, 9007199254740992m, false, "decimal integer neighbors");
        Check(1.0000000000000000000000000001m, 1m, false, "decimal fraction precision");
        Check(1m, 1.00m, true, "decimal native scale equality");
        Check(1, 1d, true, "exact integer and double");
        Check(9007199254740993L, 9007199254740992d, false, "int64 is never rounded through double");
        Check(long.MaxValue, 9223372036854775808d, false, "signed upper boundary is not widened");
        Check(ulong.MaxValue, 18446744073709551616d, false, "unsigned upper boundary is not widened");
        Check(0.1m, 0.1d, false, "decimal and binary fractions retain their actual values");
        Check(0.5m, 0.5d, true, "exact decimal and binary fraction");
        Check(0.1f, 0.1d, false, "float32 retains its own precision");
        Check((Half)0.5, 0.5f, true, "float16 widens exactly");
        Check(Int128.MinValue, Int128.MinValue + 1, false, "int128 minimum neighbors");
        Check(UInt128.MaxValue, UInt128.MaxValue - 1, false, "uint128 maximum neighbors");
        Check((nint)(-7), -7L, true, "native signed integer");
        Check((nuint)7, 7UL, true, "native unsigned integer");
        Check(new BigInteger(9007199254740993L), 9007199254740993L, true, "BigInteger exact int64");
        Check(new BigInteger(9007199254740993L), 9007199254740992d, false, "BigInteger is not rounded");
        Check(BigInteger.One << 1000, double.ScaleB(1, 1000), true, "large exact binary integer");
        Check(-(BigInteger.One << 1000), -double.ScaleB(1, 1000), true, "large negative exact binary integer");
        Check((BigInteger.One << 1000) + 1, double.ScaleB(1, 1000), false, "large adjacent binary integer");
        Check(new BigInteger(double.MaxValue), double.MaxValue, true, "largest finite binary value");
        Check(new BigInteger(double.MaxValue) + 1, double.MaxValue, false, "largest finite binary neighbor");
        Check(decimal.MaxValue, decimal.MaxValue, true, "largest native decimal");
        Check(decimal.MinValue, decimal.MinValue, true, "smallest native decimal");
        Check(double.Epsilon, (Half)0, false, "smallest binary subnormal");
    }

    [Fact]
    public void SameValueKeepsFloatingSignsReferenceIdentityAndOneAbsence()
    {
        Check(float.NaN, double.NaN, true, "NaN across native binary widths");
        Check(Half.NaN, double.NaN, true, "float16 NaN");
        Check(float.PositiveInfinity, double.PositiveInfinity, true, "positive infinity");
        Check(float.NegativeInfinity, double.NegativeInfinity, true, "negative infinity");
        Check(float.PositiveInfinity, double.NegativeInfinity, false, "opposite infinities");
        Check(0d, -0d, false, "binary signed zero");
        Check(0L, -0d, false, "integer zero is not negative binary zero");
        Check(0m, -0d, false, "decimal zero is not negative binary zero");
        Check(new decimal(0, 0, 0, true, 0), 0d, true, "decimal native zero equality");
        Check(false, 0, false, "Boolean is not a numeric coercion");
        Check('1', 49, false, "char is not an integer coercion");
        Check("1", 1, false, "string is not numeric coercion");
        Check(new string(new[] { 'a', 'b' }), "ab", true, "native ordinal string equality");
        var original = new EqualByValue();
        Assert.True(Tsonic.CSharp.Js.Object.@is(original, original), "the same original reference");
        Assert.False(Tsonic.CSharp.Js.Object.@is(original, new EqualByValue()), "Equals never replaces reference identity");
        var array = new int[] { 7 };
        Assert.True(Tsonic.CSharp.Js.Object.@is(array, array), "the same native array");
        Assert.False(Tsonic.CSharp.Js.Object.@is(array, new int[] { 7 }), "equal array contents are not identity");
        Assert.True(Tsonic.CSharp.Js.Object.@is(null, null), "one native absence");
        Assert.True(Tsonic.CSharp.Js.Object.@is(TsValue.undefined(), null), "the existing closed value unwraps to one absence");
        Assert.False(Tsonic.CSharp.Js.Object.@is(TsValue.undefined(), 0), "absence is not numeric zero");
    }

    [Fact]
    public void NativeSameValueAllocatesExactlyTheHandwrittenScalarComparison()
    {
        MeasureSame((sbyte)(-7), "sbyte");
        MeasureSame((byte)7, "byte");
        MeasureSame((short)(-7), "short");
        MeasureSame((ushort)7, "ushort");
        MeasureSame(-7, "int");
        MeasureSame(7U, "uint");
        MeasureSame(-7L, "long");
        MeasureSame(7UL, "ulong");
        MeasureSame((nint)(-7), "nint");
        MeasureSame((nuint)7, "nuint");
        MeasureSame(Int128.MinValue, "Int128");
        MeasureSame(UInt128.MaxValue, "UInt128");
        MeasureSame((Half)0.5, "Half");
        MeasureSame(0.5f, "float");
        MeasureSame(0.5d, "double");
        MeasureSame(0.5m, "decimal");
        MeasureSame(BigInteger.One << 1000, "BigInteger");
        MeasureSame(true, "bool");
        MeasureSame('x', "char");
        MeasureSame("value", "string");
        var integer = 7L;
        var fraction = 0.5m;
        var big = new BigInteger(7);
        Measure(() => Selected(integer, 7d), () => integer == 7, "mixed int64/double");
        Measure(() => Selected(fraction, 0.5d), () => fraction == 0.5m, "mixed decimal/double");
        Measure(() => Selected(big, integer), () => big == integer, "mixed BigInteger/int64");
    }

    private static void Check<TValue, TOther>(TValue value, TOther other, bool expected, string label)
    {
        Assert.True(Tsonic.CSharp.Js.Object.@is(value, other) == expected, label);
        Assert.True(Tsonic.CSharp.Js.Object.@is(other, value) == expected, label + " reverse");
        Assert.True(Tsonic.CSharp.Js.Object.@is((object?)value, (object?)other) == expected, label + " already boxed");
        Assert.True(Tsonic.CSharp.Js.Object.@is((object?)other, (object?)value) == expected, label + " already boxed reverse");
    }

    private static void MeasureSame<TValue>(TValue value, string label) =>
        Measure(() => Selected(value, value), () => EqualityComparer<TValue>.Default.Equals(value, value), label);

    private static void Measure(Func<bool> selected, Func<bool> native, string label)
    {
        for (var index = 0; index < 40000; index++)
        {
            _ = selected();
            _ = native();
        }
        var selectedCount = 0;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 40000; index++) if (selected()) selectedCount++;
        var selectedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        var nativeCount = 0;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 40000; index++) if (native()) nativeCount++;
        var nativeBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(selectedCount == 40000 && selectedCount == nativeCount, label + " exact values");
        Assert.True(nativeBytes == 0, label + " handwritten allocation");
        Assert.True(selectedBytes == nativeBytes, label + " selected allocation equals handwritten");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Selected<TValue, TOther>(TValue value, TOther other) => Tsonic.CSharp.Js.Object.@is(value, other);

    private sealed class EqualByValue
    {
        public override bool Equals(object? other) => other is EqualByValue;
        public override int GetHashCode() => 0;
    }
}
