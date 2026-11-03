using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

public static partial class Object
{
    public static bool @is(sbyte value, sbyte other) => value == other;
    public static bool @is(byte value, byte other) => value == other;
    public static bool @is(short value, short other) => value == other;
    public static bool @is(ushort value, ushort other) => value == other;
    public static bool @is(int value, int other) => value == other;
    public static bool @is(uint value, uint other) => value == other;
    public static bool @is(long value, long other) => value == other;
    public static bool @is(ulong value, ulong other) => value == other;
    public static bool @is(nint value, nint other) => value == other;
    public static bool @is(nuint value, nuint other) => value == other;
    public static bool @is(Int128 value, Int128 other) => value == other;
    public static bool @is(UInt128 value, UInt128 other) => value == other;
    public static bool @is(Half value, Half other) => SameFloatingValue(value, other);
    public static bool @is(float value, float other) => SameFloatingValue(value, other);
    public static bool @is(double value, double other) => SameFloatingValue(value, other);
    public static bool @is(decimal value, decimal other) => value == other;
    public static bool @is(BigInteger value, BigInteger other) => value == other;
    public static bool @is(bool value, bool other) => value == other;
    public static bool @is(char value, char other) => value == other;
    public static bool @is(string? value, string? other) => string.Equals(value, other, StringComparison.Ordinal);

    public static bool @is(object? value, object? other) => SameValue(UnwrapClosedValue(value), UnwrapClosedValue(other));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool @is<TValue, TOther>(TValue value, TOther other) =>
        IsNativeScalar<TValue>() && IsNativeScalar<TOther>()
            ? SameValue(value, other)
            : SameValue(UnwrapClosedValue(value), UnwrapClosedValue(other));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameValue<TValue, TOther>(TValue value, TOther other) => value switch
    {
        null => other is null,
        sbyte number => SameNumericOther(number, other),
        byte number => SameNumericOther(number, other),
        short number => SameNumericOther(number, other),
        ushort number => SameNumericOther(number, other),
        int number => SameNumericOther(number, other),
        uint number => SameNumericOther(number, other),
        long number => SameNumericOther(number, other),
        ulong number => SameNumericOther(number, other),
        nint number => SameNumericOther(number, other),
        nuint number => SameNumericOther(number, other),
        Int128 number => SameNumericOther(number, other),
        UInt128 number => SameNumericOther(number, other),
        Half number => SameNumericOther(number, other),
        float number => SameNumericOther(number, other),
        double number => SameNumericOther(number, other),
        decimal number => SameNumericOther(number, other),
        BigInteger number => SameNumericOther(number, other),
        string text => other is string otherText && string.Equals(text, otherText, StringComparison.Ordinal),
        bool boolean => other is bool otherBoolean && boolean == otherBoolean,
        char character => other is char otherCharacter && character == otherCharacter,
        _ => ReferenceEquals(value, other),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameNumericOther<TValue, TOther>(TValue value, TOther other)
        where TValue : INumberBase<TValue> => other switch
    {
        sbyte number => SameNumericValue(value, number),
        byte number => SameNumericValue(value, number),
        short number => SameNumericValue(value, number),
        ushort number => SameNumericValue(value, number),
        int number => SameNumericValue(value, number),
        uint number => SameNumericValue(value, number),
        long number => SameNumericValue(value, number),
        ulong number => SameNumericValue(value, number),
        nint number => SameNumericValue(value, number),
        nuint number => SameNumericValue(value, number),
        Int128 number => SameNumericValue(value, number),
        UInt128 number => SameNumericValue(value, number),
        Half number => SameNumericValue(value, number),
        float number => SameNumericValue(value, number),
        double number => SameNumericValue(value, number),
        decimal number => SameNumericValue(value, number),
        BigInteger number => SameNumericValue(value, number),
        _ => false,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameNumericValue<TValue, TOther>(TValue value, TOther other)
        where TValue : INumberBase<TValue>
        where TOther : INumberBase<TOther>
    {
        if (typeof(TValue) == typeof(TOther))
        {
            var converted = TValue.CreateChecked(other);
            return IsBinaryFloating<TValue>() ? SameFloatingValue(value, converted) : value == converted;
        }
        if (IsBinaryFloating<TValue>() && IsBinaryFloating<TOther>())
            return SameFloatingValue(double.CreateChecked(value), double.CreateChecked(other));
        if (IsFixedInteger<TValue>() && IsFixedInteger<TOther>())
            return TValue.IsNegative(value) == TOther.IsNegative(other) && Magnitude(value) == Magnitude(other);
        if (TValue.IsNaN(value) || TOther.IsNaN(other))
            return TValue.IsNaN(value) && TOther.IsNaN(other);
        if (TValue.IsZero(value) || TOther.IsZero(other))
            return TValue.IsZero(value) && TOther.IsZero(other) &&
                (IsBinaryFloating<TValue>() && TValue.IsNegative(value)) ==
                (IsBinaryFloating<TOther>() && TOther.IsNegative(other));
        if (TValue.IsNegative(value) != TOther.IsNegative(other)) return false;
        if (TValue.IsInfinity(value) || TOther.IsInfinity(other))
            return TValue.IsInfinity(value) && TOther.IsInfinity(other);
        if (typeof(TValue) == typeof(BigInteger))
            return SameBigInteger(BigInteger.CreateChecked(value), NumericParts(other));
        if (typeof(TOther) == typeof(BigInteger))
            return SameBigInteger(BigInteger.CreateChecked(other), NumericParts(value));
        return NumericParts(value) == NumericParts(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameFloatingValue<TValue>(TValue value, TValue other)
        where TValue : INumberBase<TValue> =>
        value == other
            ? !TValue.IsZero(value) || TValue.IsNegative(value) == TValue.IsNegative(other)
            : TValue.IsNaN(value) && TValue.IsNaN(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFixedInteger<TValue>() =>
        typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(byte) ||
        typeof(TValue) == typeof(short) || typeof(TValue) == typeof(ushort) ||
        typeof(TValue) == typeof(int) || typeof(TValue) == typeof(uint) ||
        typeof(TValue) == typeof(long) || typeof(TValue) == typeof(ulong) ||
        typeof(TValue) == typeof(nint) || typeof(TValue) == typeof(nuint) ||
        typeof(TValue) == typeof(Int128) || typeof(TValue) == typeof(UInt128);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBinaryFloating<TValue>() =>
        typeof(TValue) == typeof(Half) || typeof(TValue) == typeof(float) || typeof(TValue) == typeof(double);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNativeScalar<TValue>() => IsFixedInteger<TValue>() || IsBinaryFloating<TValue>() ||
        typeof(TValue) == typeof(decimal) || typeof(TValue) == typeof(BigInteger) ||
        typeof(TValue) == typeof(bool) || typeof(TValue) == typeof(char) || typeof(TValue) == typeof(string);

    private static UInt128 Magnitude<TValue>(TValue value) where TValue : INumberBase<TValue>
    {
        if (!TValue.IsNegative(value)) return UInt128.CreateChecked(value);
        var signed = Int128.CreateChecked(value);
        return UInt128.CreateChecked(-(signed + 1)) + 1;
    }

    private static (UInt128 Coefficient, int BinaryExponent, int FiveExponent) NumericParts<TValue>(TValue value)
        where TValue : INumberBase<TValue>
    {
        UInt128 coefficient;
        var binaryExponent = 0;
        var fiveExponent = 0;
        if (typeof(TValue) == typeof(decimal))
        {
            Span<int> bits = stackalloc int[4];
            decimal.GetBits(decimal.CreateChecked(value), bits);
            coefficient = (UInt128)(uint)bits[0] | ((UInt128)(uint)bits[1] << 32) | ((UInt128)(uint)bits[2] << 64);
            binaryExponent = fiveExponent = -((bits[3] >> 16) & 0xff);
        }
        else if (IsBinaryFloating<TValue>())
        {
            var bits = BitConverter.DoubleToUInt64Bits(double.CreateChecked(value));
            var exponent = (int)((bits >> 52) & 0x7ff);
            coefficient = bits & 0xfffffffffffff;
            if (exponent != 0) coefficient |= (UInt128)1 << 52;
            binaryExponent = exponent == 0 ? -1074 : exponent - 1075;
        }
        else
        {
            coefficient = Magnitude(value);
        }
        while ((coefficient & 1) == 0)
        {
            coefficient >>= 1;
            binaryExponent++;
        }
        while (coefficient % 5 == 0)
        {
            coefficient /= 5;
            fiveExponent++;
        }
        return (coefficient, binaryExponent, fiveExponent);
    }

    private static bool SameBigInteger(BigInteger value, (UInt128 Coefficient, int BinaryExponent, int FiveExponent) other)
    {
        if (other.BinaryExponent < 0 || other.FiveExponent < 0 || value.GetByteCount() > 129) return false;
        Span<byte> actual = stackalloc byte[129];
        Span<byte> expected = stackalloc byte[129];
        actual.Clear();
        expected.Clear();
        if (!value.TryWriteBytes(actual, out var written)) return false;
        if (value.Sign < 0)
        {
            var carry = 1;
            for (var index = 0; index < written; index++)
            {
                var next = (byte)~actual[index] + carry;
                actual[index] = (byte)next;
                carry = next >> 8;
            }
        }
        var coefficient = other.Coefficient;
        for (var index = 0; index < other.FiveExponent; index++) coefficient *= 5;
        var byteOffset = other.BinaryExponent / 8;
        var bitOffset = other.BinaryExponent % 8;
        for (var index = 0; coefficient != 0; index++, coefficient >>= 8)
        {
            var shifted = (int)(byte)coefficient << bitOffset;
            expected[byteOffset + index] |= (byte)shifted;
            expected[byteOffset + index + 1] |= (byte)(shifted >> 8);
        }
        return actual.SequenceEqual(expected);
    }
}
