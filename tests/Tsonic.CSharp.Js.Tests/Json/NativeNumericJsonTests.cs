using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class NativeNumericJsonTests
{
    [Fact]
    public void EveryAdmittedNativeIntegerWidthSerializesWithoutAFloatRoundtrip()
    {
        (object Value, string Expected)[] values = [
            (sbyte.MinValue, "-128"), (byte.MaxValue, "255"),
            (short.MinValue, "-32768"), (ushort.MaxValue, "65535"),
            (int.MinValue, "-2147483648"), (uint.MaxValue, "4294967295"),
            (long.MinValue, "-9223372036854775808"), (ulong.MaxValue, "18446744073709551615"),
            ((long)9_007_199_254_740_993, "9007199254740993"),
            (nint.MinValue, nint.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            (nuint.MaxValue, nuint.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            (Int128.MinValue, "-170141183460469231731687303715884105728"),
            (Int128.MaxValue, "170141183460469231731687303715884105727"),
            (UInt128.MaxValue, "340282366920938463463374607431768211455"),
        ];
        foreach (var (value, expected) in values)
        {
            Assert.Equal(expected, JSON.stringify(value));
            Assert.Equal(expected, JSON.stringify(TsValue.from(value)));
            Assert.Equal("[" + expected + "]", JSON.stringify(JSArray<object>.of([value])));
        }
    }

    [Fact]
    public void NativeHalfSingleDoubleAndDecimalKeepTheirDeclaredPrecision()
    {
        Assert.Equal("0.5", JSON.stringify((Half)0.5));
        Assert.Equal("0.5", JSON.stringify(0.5f));
        Assert.Equal("0.5", JSON.stringify(0.5d));
        Assert.Equal("79228162514264337593543950335", JSON.stringify(decimal.MaxValue));
        Assert.Equal("-79228162514264337593543950335", JSON.stringify(TsValue.from(decimal.MinValue)));
    }
}
