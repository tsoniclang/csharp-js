using System;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

internal readonly struct NativeNumericKey : IEquatable<NativeNumericKey>
{
    private readonly UInt128 _magnitude;
    private readonly int _binaryExponent;
    private readonly int _quinaryExponent;
    private readonly bool _negative;
    private readonly byte _kind;

    private NativeNumericKey(UInt128 magnitude, int binaryExponent, int quinaryExponent, bool negative, byte kind = 0)
    {
        if (magnitude == 0) negative = false;
        else
        {
            while (magnitude % 2 == 0) { magnitude /= 2; binaryExponent++; }
            while (magnitude % 5 == 0) { magnitude /= 5; quinaryExponent++; }
        }
        _magnitude = magnitude;
        _binaryExponent = magnitude == 0 ? 0 : binaryExponent;
        _quinaryExponent = magnitude == 0 ? 0 : quinaryExponent;
        _negative = negative;
        _kind = kind;
    }

    public static NativeNumericKey Create<T>(T value) => TryCreate(value, out var key) ? key
        : throw new InvalidOperationException("A native numeric key requires its exact closed numeric carrier.");

    public static bool TryCreate<T>(T value, out NativeNumericKey key)
    {
        if (typeof(T) == typeof(sbyte)) { key = Signed(Unsafe.As<T, sbyte>(ref value)); return true; }
        if (typeof(T) == typeof(byte)) { key = Unsigned(Unsafe.As<T, byte>(ref value)); return true; }
        if (typeof(T) == typeof(short)) { key = Signed(Unsafe.As<T, short>(ref value)); return true; }
        if (typeof(T) == typeof(ushort)) { key = Unsigned(Unsafe.As<T, ushort>(ref value)); return true; }
        if (typeof(T) == typeof(int)) { key = Signed(Unsafe.As<T, int>(ref value)); return true; }
        if (typeof(T) == typeof(uint)) { key = Unsigned(Unsafe.As<T, uint>(ref value)); return true; }
        if (typeof(T) == typeof(long)) { key = Signed(Unsafe.As<T, long>(ref value)); return true; }
        if (typeof(T) == typeof(ulong)) { key = Unsigned(Unsafe.As<T, ulong>(ref value)); return true; }
        if (typeof(T) == typeof(nint)) { key = Signed(Unsafe.As<T, nint>(ref value)); return true; }
        if (typeof(T) == typeof(nuint)) { key = Unsigned(Unsafe.As<T, nuint>(ref value)); return true; }
        if (typeof(T) == typeof(Int128)) { key = Signed(Unsafe.As<T, Int128>(ref value)); return true; }
        if (typeof(T) == typeof(UInt128)) { key = Unsigned(Unsafe.As<T, UInt128>(ref value)); return true; }
        if (typeof(T) == typeof(double)) { key = Floating(Unsafe.As<T, double>(ref value)); return true; }
        if (typeof(T) == typeof(float)) { key = Floating(Unsafe.As<T, float>(ref value)); return true; }
        if (typeof(T) == typeof(Half)) { key = Floating((double)Unsafe.As<T, Half>(ref value)); return true; }
        if (typeof(T) == typeof(decimal)) { key = Decimal(Unsafe.As<T, decimal>(ref value)); return true; }
        key = default;
        return false;
    }

    private static NativeNumericKey Signed(Int128 value)
    {
        var negative = value < 0;
        var magnitude = negative ? unchecked((UInt128)(~value) + 1) : (UInt128)value;
        return new(magnitude, 0, 0, negative);
    }

    private static NativeNumericKey Unsigned(UInt128 value) => new(value, 0, 0, false);

    private static NativeNumericKey Floating(double value)
    {
        var bits = (ulong)BitConverter.DoubleToInt64Bits(value);
        var exponent = (int)((bits >> 52) & 0x7ff);
        var fraction = bits & 0x000fffffffffffff;
        var negative = (bits >> 63) != 0;
        if (exponent == 0x7ff) return new(0, 0, 0, false,
            fraction != 0 ? (byte)1 : negative ? (byte)3 : (byte)2);
        return new(exponent == 0 ? fraction : fraction | 0x0010000000000000,
            exponent == 0 ? -1074 : exponent - 1075, 0, negative);
    }

    private static NativeNumericKey Decimal(decimal value)
    {
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(value, bits);
        var magnitude = (UInt128)(uint)bits[0] | (UInt128)(uint)bits[1] << 32 | (UInt128)(uint)bits[2] << 64;
        var scale = (bits[3] >> 16) & 0xff;
        return new(magnitude, -scale, -scale, bits[3] < 0);
    }

    public bool Equals(NativeNumericKey other) => _kind == other._kind && _negative == other._negative &&
        _magnitude == other._magnitude && _binaryExponent == other._binaryExponent && _quinaryExponent == other._quinaryExponent;

    public override bool Equals(object? other) => other is NativeNumericKey key && Equals(key);
    public override int GetHashCode() => HashCode.Combine(_kind, _negative, _magnitude, _binaryExponent, _quinaryExponent);
}
