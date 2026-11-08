using System;
using System.Buffers.Binary;
using System.Numerics;
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
        if (magnitude == 0)
        {
            binaryExponent = 0;
            quinaryExponent = 0;
            negative = false;
        }
        else
        {
            var low = (ulong)magnitude;
            var binaryZeros = low == 0
                ? 64 + BitOperations.TrailingZeroCount((ulong)(magnitude >> 64))
                : BitOperations.TrailingZeroCount(low);
            magnitude >>= binaryZeros;
            binaryExponent += binaryZeros;
            while (magnitude % 5 == 0)
            {
                magnitude /= 5;
                quinaryExponent++;
            }
        }
        _magnitude = magnitude;
        _binaryExponent = binaryExponent;
        _quinaryExponent = quinaryExponent;
        _negative = negative;
        _kind = kind;
    }

    internal bool IsNaN => _kind == 1;

    internal static bool TryCreate<TValue>(TValue value, out NativeNumericKey key)
    {
        if (typeof(TValue) == typeof(sbyte)) { key = Signed(Unsafe.As<TValue, sbyte>(ref value)); return true; }
        if (typeof(TValue) == typeof(byte)) { key = Unsigned(Unsafe.As<TValue, byte>(ref value)); return true; }
        if (typeof(TValue) == typeof(short)) { key = Signed(Unsafe.As<TValue, short>(ref value)); return true; }
        if (typeof(TValue) == typeof(ushort)) { key = Unsigned(Unsafe.As<TValue, ushort>(ref value)); return true; }
        if (typeof(TValue) == typeof(int)) { key = Signed(Unsafe.As<TValue, int>(ref value)); return true; }
        if (typeof(TValue) == typeof(uint)) { key = Unsigned(Unsafe.As<TValue, uint>(ref value)); return true; }
        if (typeof(TValue) == typeof(long)) { key = Signed(Unsafe.As<TValue, long>(ref value)); return true; }
        if (typeof(TValue) == typeof(ulong)) { key = Unsigned(Unsafe.As<TValue, ulong>(ref value)); return true; }
        if (typeof(TValue) == typeof(nint)) { key = Signed(Unsafe.As<TValue, nint>(ref value)); return true; }
        if (typeof(TValue) == typeof(nuint)) { key = Unsigned(Unsafe.As<TValue, nuint>(ref value)); return true; }
        if (typeof(TValue) == typeof(Int128)) { key = Signed(Unsafe.As<TValue, Int128>(ref value)); return true; }
        if (typeof(TValue) == typeof(UInt128)) { key = Unsigned(Unsafe.As<TValue, UInt128>(ref value)); return true; }
        if (typeof(TValue) == typeof(Half)) { key = Floating((double)Unsafe.As<TValue, Half>(ref value)); return true; }
        if (typeof(TValue) == typeof(float)) { key = Floating(Unsafe.As<TValue, float>(ref value)); return true; }
        if (typeof(TValue) == typeof(double)) { key = Floating(Unsafe.As<TValue, double>(ref value)); return true; }
        if (typeof(TValue) == typeof(decimal)) { key = Decimal(Unsafe.As<TValue, decimal>(ref value)); return true; }
        if (typeof(TValue) == typeof(BigInteger)) return Integer(Unsafe.As<TValue, BigInteger>(ref value), out key);
        key = default;
        return false;
    }

    internal static bool TryCreateBoxed(object? value, out NativeNumericKey key)
    {
        switch (value)
        {
            case sbyte number: return TryCreate(number, out key);
            case byte number: return TryCreate(number, out key);
            case short number: return TryCreate(number, out key);
            case ushort number: return TryCreate(number, out key);
            case int number: return TryCreate(number, out key);
            case uint number: return TryCreate(number, out key);
            case long number: return TryCreate(number, out key);
            case ulong number: return TryCreate(number, out key);
            case nint number: return TryCreate(number, out key);
            case nuint number: return TryCreate(number, out key);
            case Int128 number: return TryCreate(number, out key);
            case UInt128 number: return TryCreate(number, out key);
            case Half number: return TryCreate(number, out key);
            case float number: return TryCreate(number, out key);
            case double number: return TryCreate(number, out key);
            case decimal number: return TryCreate(number, out key);
            case BigInteger number: return TryCreate(number, out key);
            default: key = default; return false;
        }
    }

    private static NativeNumericKey Signed(Int128 value) =>
        new(value < 0 ? unchecked((UInt128)(~value) + 1) : (UInt128)value, 0, 0, value < 0);

    private static NativeNumericKey Unsigned(UInt128 value) => new(value, 0, 0, false);

    private static NativeNumericKey Floating(double value)
    {
        var bits = (ulong)BitConverter.DoubleToInt64Bits(value);
        var exponent = (int)((bits >> 52) & 0x7ff);
        var fraction = bits & 0x000fffffffffffff;
        var negative = (bits >> 63) != 0;
        if (exponent == 0x7ff)
            return new(0, 0, 0, false, fraction != 0 ? (byte)1 : negative ? (byte)3 : (byte)2);
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

    private static bool Integer(BigInteger value, out NativeNumericKey key)
    {
        var magnitude = BigInteger.Abs(value);
        var bitLength = magnitude.GetBitLength();
        if (bitLength <= 128)
        {
            Span<byte> bytes = stackalloc byte[16];
            bytes.Clear();
            if (!magnitude.TryWriteBytes(bytes, out _, isUnsigned: true, isBigEndian: false))
                throw new InvalidOperationException("A bounded native integer key exceeded its proven storage.");
            var exact = (UInt128)BinaryPrimitives.ReadUInt64LittleEndian(bytes) |
                (UInt128)BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..]) << 64;
            key = new(exact, 0, 0, value.Sign < 0);
            return true;
        }
        if (bitLength <= 1024 && bitLength - (int)BigInteger.TrailingZeroCount(magnitude) <= 53)
        {
            var number = (double)value;
            if (double.IsFinite(number))
            {
                key = Floating(number);
                return true;
            }
        }
        key = default;
        return false;
    }

    public bool Equals(NativeNumericKey other) => _kind == other._kind && _negative == other._negative &&
        _magnitude == other._magnitude && _binaryExponent == other._binaryExponent && _quinaryExponent == other._quinaryExponent;

    public override bool Equals(object? other) => other is NativeNumericKey key && Equals(key);

    public override int GetHashCode() => HashCode.Combine(_kind, _negative, _magnitude, _binaryExponent, _quinaryExponent);
}
