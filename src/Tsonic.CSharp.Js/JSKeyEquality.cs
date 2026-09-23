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
        return BoxedStrictEqual(left, right);
    }

    private static bool BoxedStrictEqual(object? left, object? right)
    {
        if (left is TsValue leftValue) return BoxedStrictEqual(leftValue.unwrap(), right);
        if (right is TsValue rightValue) return BoxedStrictEqual(left, rightValue.unwrap());
        if (left is double number && double.IsNaN(number) || left is float single && float.IsNaN(single) ||
            left is System.Half half && System.Half.IsNaN(half)) return false;
        return BoxedEqual(left, right);
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
            : BoxedEqual(left, right);

    public static bool sameValueZeroUndefined<T>(T value) => BoxedEqual(Undefined.value, value);

    public static int keyHash<T>(T value) => NativeValueKey<T>.Supported
        ? value is null ? 0 : System.Collections.Generic.EqualityComparer<T>.Default.GetHashCode(value)
        : BoxedHash(value);

    private static bool BoxedEqual(object? left, object? right)
    {
        if (left is TsValue leftValue) return BoxedEqual(leftValue.unwrap(), right);
        if (right is TsValue rightValue) return BoxedEqual(left, rightValue.unwrap());
        if (ReferenceEquals(left, right)) return true;
        if (IntegerKey(left, out var leftMagnitude, out var leftNegative) &&
            IntegerKey(right, out var rightMagnitude, out var rightNegative))
            return leftMagnitude == rightMagnitude && leftNegative == rightNegative;
        if (FloatingKey(left, out var leftFloat) && FloatingKey(right, out var rightFloat)) return leftFloat.Equals(rightFloat);
        return left is not null && IsNativeValue(left) && left.Equals(right);
    }

    private static int BoxedHash(object? value)
    {
        if (value is TsValue wrapped) return BoxedHash(wrapped.unwrap());
        if (value is null) return 0;
        if (IntegerKey(value, out var magnitude, out var negative)) return System.HashCode.Combine(magnitude, negative);
        if (FloatingKey(value, out var number)) return number.GetHashCode();
        return IsNativeValue(value) ? value.GetHashCode()
            : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
    }

    private static bool FloatingKey(object? value, out double number)
    {
        switch (value)
        {
            case double typed: number = typed; return true;
            case float typed: number = typed; return true;
            case System.Half typed: number = (double)typed; return true;
            default: number = 0; return false;
        }
    }

    private static bool IntegerKey(object? value, out System.UInt128 magnitude, out bool negative)
    {
        System.Int128 signed;
        switch (value)
        {
            case sbyte typed: signed = typed; break;
            case byte typed: signed = typed; break;
            case short typed: signed = typed; break;
            case ushort typed: signed = typed; break;
            case int typed: signed = typed; break;
            case uint typed: signed = typed; break;
            case long typed: signed = typed; break;
            case ulong typed: signed = typed; break;
            case nint typed: signed = typed; break;
            case nuint typed: signed = typed; break;
            case System.Int128 typed: signed = typed; break;
            case System.UInt128 typed: magnitude = typed; negative = false; return true;
            case decimal typed when decimal.IsInteger(typed): signed = (System.Int128)typed; break;
            default:
                if (!FloatingKey(value, out var number) || !double.IsInteger(number) ||
                    number < -170141183460469231731687303715884105728d || number >= 340282366920938463463374607431768211456d)
                {
                    magnitude = 0;
                    negative = false;
                    return false;
                }
                negative = number < 0;
                magnitude = (System.UInt128)System.Math.Abs(number);
                return true;
        }
        negative = signed < 0;
        magnitude = negative ? unchecked((System.UInt128)(~signed) + 1) : (System.UInt128)signed;
        return true;
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
