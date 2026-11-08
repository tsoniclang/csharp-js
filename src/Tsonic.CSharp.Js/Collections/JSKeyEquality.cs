using System;

namespace Tsonic.CSharp.Js;

internal static class JSKeyEquality
{
    internal static bool HasNativeScalarEquality<TValue>() => NativeValueKey<TValue>.Supported;

    internal static bool NativeStrictEquals<TValue>(TValue left, TValue right) => NativeEqual(left, right, true);

    internal static bool NativeSameValueZero<TValue>(TValue left, TValue right) => NativeEqual(left, right, false);

    private static bool NativeEqual<TValue>(TValue left, TValue right, bool strict)
    {
        if (NativeValueKey<TValue>.Supported)
            return (!strict || !NativeIsNaN(left)) && System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(left, right);
        var leftValue = TsValue.UnwrapClosedValue(left);
        var rightValue = TsValue.UnwrapClosedValue(right);
        return (!strict || !NativeNumericKey.TryCreateBoxed(leftValue, out var number) || !number.IsNaN) &&
            EqualValues(leftValue, rightValue);
    }

    internal static int NativeKeyHash<TValue>(TValue value) => NativeValueKey<TValue>.Supported
        ? value is null ? 0 : System.Collections.Generic.EqualityComparer<TValue>.Default.GetHashCode(value)
        : PayloadHash(TsValue.UnwrapClosedValue(value));

    internal static bool NativeIsNaN<TValue>(TValue value)
    {
        if (typeof(TValue) == typeof(double)) return double.IsNaN(System.Runtime.CompilerServices.Unsafe.As<TValue, double>(ref value));
        if (typeof(TValue) == typeof(float)) return float.IsNaN(System.Runtime.CompilerServices.Unsafe.As<TValue, float>(ref value));
        if (typeof(TValue) == typeof(Half)) return Half.IsNaN(System.Runtime.CompilerServices.Unsafe.As<TValue, Half>(ref value));
        return !NativeValueKey<TValue>.Supported && NativeNumericKey.TryCreateBoxed(TsValue.UnwrapClosedValue(value), out var number) && number.IsNaN;
    }

    public static T canonicalizeKeyedCollectionKey<T>(T value)
    {
        if (typeof(T) == typeof(double))
        {
            ref var number = ref System.Runtime.CompilerServices.Unsafe.As<T, double>(ref value);
            if (number == 0) number = 0;
        }
        else if (typeof(T) == typeof(float))
        {
            ref var number = ref System.Runtime.CompilerServices.Unsafe.As<T, float>(ref value);
            if (number == 0) number = 0;
        }
        else if (typeof(T) == typeof(System.Half))
        {
            ref var number = ref System.Runtime.CompilerServices.Unsafe.As<T, System.Half>(ref value);
            if (number == (System.Half)0) number = (System.Half)0;
        }
        else if (typeof(T) == typeof(TsValue))
        {
            ref var stored = ref System.Runtime.CompilerServices.Unsafe.As<T, TsValue>(ref value);
            stored = CanonicalStored(stored);
        }
        else if (!NativeValueKey<T>.Supported)
        {
            value = value switch
            {
                double number when number == 0 => (T)CanonicalZero<double>.Value,
                float number when number == 0 => (T)CanonicalZero<float>.Value,
                System.Half number when number == (System.Half)0 => (T)CanonicalZero<System.Half>.Value,
                TsValue wrapped => (T)(object)CanonicalStored(wrapped),
                _ => value,
            };
        }
        return value;
    }

    private static int PayloadHash(object? payload)
    {
        if (payload is null) return 0;
        if (NativeNumericKey.TryCreateBoxed(payload, out var number)) return number.GetHashCode();
        return IsNativeValue(payload) ? payload.GetHashCode()
            : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(payload);
    }

    private static bool EqualValues(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (NativeNumericKey.TryCreateBoxed(left, out var leftNumber) &&
            NativeNumericKey.TryCreateBoxed(right, out var rightNumber))
            return leftNumber.Equals(rightNumber);
        return left is not null && IsNativeValue(left) && left.Equals(right);
    }

    private static TsValue CanonicalStored(TsValue value)
    {
        if (value.Category is not (SourceValueCategory.Number or SourceValueCategory.BigInt)) return value;
        var payload = value.unwrap();
        return payload switch
        {
            double number when number == 0 && BitConverter.DoubleToInt64Bits(number) < 0 => TsValue.fromsource(CanonicalZero<double>.Value, value.Category),
            float number when number == 0 && BitConverter.SingleToInt32Bits(number) < 0 => TsValue.fromsource(CanonicalZero<float>.Value, value.Category),
            Half number when number == (Half)0 && BitConverter.HalfToInt16Bits(number) < 0 => TsValue.fromsource(CanonicalZero<Half>.Value, value.Category),
            _ => value,
        };
    }

    private static bool IsNativeValue(object value) => value is
        byte or sbyte or short or ushort or int or uint or long or ulong or nint or nuint or
        System.Int128 or System.UInt128 or System.Half or float or double or decimal or bool or char or string or
        System.Numerics.BigInteger;

    private static class CanonicalZero<TValue> where TValue : struct
    {
        internal static readonly object Value = default(TValue);
        static CanonicalZero() { }
    }

    private static class NativeValueKey<T>
    {
        public static readonly bool Supported =
            typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) ||
            typeof(T) == typeof(short) || typeof(T) == typeof(ushort) ||
            typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
            typeof(T) == typeof(long) || typeof(T) == typeof(ulong) ||
            typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) ||
            typeof(T) == typeof(System.Int128) || typeof(T) == typeof(System.UInt128) ||
            typeof(T) == typeof(System.Half) || typeof(T) == typeof(float) || typeof(T) == typeof(double) ||
            typeof(T) == typeof(decimal) || typeof(T) == typeof(bool) ||
            typeof(T) == typeof(char) || typeof(T) == typeof(string) ||
            typeof(T) == typeof(System.Numerics.BigInteger);
    }
}
