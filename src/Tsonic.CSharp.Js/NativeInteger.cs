using System;

namespace Tsonic.CSharp.Js;

internal static class NativeInteger
{
    internal static uint Bits32<T>(T value) where T : System.Numerics.INumberBase<T> =>
        typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(Half)
            ? Bits32(double.CreateChecked(value)) : uint.CreateTruncating(value);

    internal static long Index(double value) => double.IsNaN(value) ? 0
        : value <= long.MinValue ? long.MinValue
        : value >= -(double)long.MinValue ? long.MaxValue : (long)value;

    internal static int IndexLength(double value) =>
        Length(double.IsNaN(value) ? 0 : System.Math.Truncate(value));

    internal static uint Bits32(double value)
    {
        var bits = BitConverter.DoubleToUInt64Bits(value);
        var exponent = (int)((bits >> 52) & 0x7ff) - 1023;
        if (exponent < 0 || exponent >= 84) return 0;
        var significand = (bits & ((1UL << 52) - 1)) | (1UL << 52);
        var magnitude = exponent < 52 ? (uint)(significand >> (52 - exponent))
            : (uint)significand << (exponent - 52);
        return bits >> 63 != 0 ? unchecked(0U - magnitude) : magnitude;
    }

    internal static uint SplitLimit(double? value) =>
        value.HasValue ? Bits32(value.Value) : uint.MaxValue;

    internal static int Length(double value)
    {
        if (!double.IsInteger(value) || value < 0 || value > int.MaxValue)
            throw new RangeError("Length must be a non-negative native integer.");
        return (int)value;
    }
}
