namespace Tsonic.CSharp.Js;

internal static class JSKeyEquality
{
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
        return left is not null && IsNativeValue(left) && left.Equals(right);
    }

    private static int BoxedHash(object? value)
    {
        if (value is TsValue wrapped) return BoxedHash(wrapped.unwrap());
        if (value is null) return 0;
        return IsNativeValue(value) ? value.GetHashCode()
            : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
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
