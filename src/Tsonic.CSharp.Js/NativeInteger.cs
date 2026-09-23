using System;

namespace Tsonic.CSharp.Js;

internal static class NativeInteger
{
    internal static long Index(double value) => checked((long)value);

    internal static int Length(double value)
    {
        if (!double.IsInteger(value) || value < 0 || value > int.MaxValue)
            throw new RangeError("Length must be a non-negative native integer.");
        return (int)value;
    }
}
