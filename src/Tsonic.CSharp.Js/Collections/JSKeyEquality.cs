namespace Tsonic.CSharp.Js;

internal static class JSKeyEquality
{
    public static bool strictEquals<T>(T left, T right)
    {
        if (typeof(T) == typeof(double))
            return System.Runtime.CompilerServices.Unsafe.As<T, double>(ref left) == System.Runtime.CompilerServices.Unsafe.As<T, double>(ref right);
        if (typeof(T) == typeof(float))
            return System.Runtime.CompilerServices.Unsafe.As<T, float>(ref left) == System.Runtime.CompilerServices.Unsafe.As<T, float>(ref right);
        if (typeof(T) == typeof(System.Half))
            return System.Runtime.CompilerServices.Unsafe.As<T, System.Half>(ref left) == System.Runtime.CompilerServices.Unsafe.As<T, System.Half>(ref right);
        if (NativeValueKey<T>.Supported) return System.Collections.Generic.EqualityComparer<T>.Default.Equals(left, right);
        var leftValue = TsValue.UnwrapClosedValue(left);
        var rightValue = TsValue.UnwrapClosedValue(right);
        return !(NativeNumericKey.TryCreateBoxed(leftValue, out var number) && number.IsNaN) &&
            EqualValues(leftValue, rightValue);
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
        else if (!NativeValueKey<T>.Supported)
        {
            value = value switch
            {
                double number when number == 0 => (T)(object)0.0,
                float number when number == 0 => (T)(object)0.0f,
                System.Half number when number == (System.Half)0 => (T)(object)(System.Half)0,
                TsValue wrapped => (T)(object)TsValue.from(canonicalizeKeyedCollectionKey(wrapped.unwrap())),
                _ => value,
            };
        }
        return value;
    }

    public static bool sameValueZero<T>(T left, T right) =>
        NativeValueKey<T>.Supported
            ? System.Collections.Generic.EqualityComparer<T>.Default.Equals(left, right)
            : EqualValues(TsValue.UnwrapClosedValue(left), TsValue.UnwrapClosedValue(right));

    public static bool sameValueZeroUndefined<T>(T value) => TsValue.UnwrapClosedValue(value) is null;

    public static int keyHash<T>(T value)
    {
        if (NativeValueKey<T>.Supported)
            return value is null ? 0 : System.Collections.Generic.EqualityComparer<T>.Default.GetHashCode(value);
        var payload = TsValue.UnwrapClosedValue(value);
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

    private static bool IsNativeValue(object value) => value is
        byte or sbyte or short or ushort or int or uint or long or ulong or nint or nuint or
        System.Int128 or System.UInt128 or System.Half or float or double or decimal or bool or char or string or
        System.Numerics.BigInteger;

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
