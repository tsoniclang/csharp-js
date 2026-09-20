namespace Tsonic.CSharp.Js
{
    internal static class JSKeyEquality
    {
        public static bool sameValueZero<T>(T left, T right)
        {
            if (NativeValueKey<T>.Supported)
                return System.Collections.Generic.EqualityComparer<T>.Default.Equals(left, right);
            return sameValueZero((object?)left, (object?)right);
        }

        public static int keyHash<T>(T value)
        {
            if (NativeValueKey<T>.Supported)
                return value is null ? 0 : System.Collections.Generic.EqualityComparer<T>.Default.GetHashCode(value);
            return boxedKeyHash(value);
        }

        private static int boxedKeyHash(object? value)
        {
            if (value is TsValue wrapped) return boxedKeyHash(wrapped.unwrap());
            if (value is null) return 0;
            if (tryReadNumber(value, out var number)) return number.GetHashCode();
            if (value is string text) return System.StringComparer.Ordinal.GetHashCode(text);
            if (value is bool boolean) return boolean.GetHashCode();
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
        }

        private static class NativeValueKey<T>
        {
            public static readonly bool Supported =
                typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte) ||
                typeof(T) == typeof(short) || typeof(T) == typeof(ushort) ||
                typeof(T) == typeof(int) || typeof(T) == typeof(uint) ||
                typeof(T) == typeof(long) || typeof(T) == typeof(ulong) ||
                typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) ||
                typeof(T) == typeof(float) || typeof(T) == typeof(double) ||
                typeof(T) == typeof(decimal) || typeof(T) == typeof(bool) ||
                typeof(T) == typeof(char) || typeof(T) == typeof(string) ||
                typeof(T) == typeof(System.Numerics.BigInteger);
        }

        public static bool strictEquals<T>(T left, T right)
        {
            return strictEquals((object?)left, (object?)right);
        }

        public static bool sameValueZeroUndefined<T>(T value)
        {
            return sameValueZero(Undefined.value, (object?)value);
        }

        public static T canonicalizeKeyedCollectionKey<T>(T value)
        {
            if (NativeValueKey<T>.Supported && typeof(T) != typeof(double) && typeof(T) != typeof(float))
                return value;
            return value switch
            {
                double typed when typed == 0.0 => (T)(object)0.0,
                float typed when typed == 0.0f => (T)(object)0.0f,
                TsValue typed => (T)(object)TsValue.from(canonicalizeKeyedCollectionKey(typed.unwrap())),
                object boxed => (T)canonicalizeBoxedKeyedCollectionKey(boxed),
                _ => value
            };
        }

        private static bool strictEquals(object? left, object? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is TsValue leftValue)
            {
                return strictEquals(leftValue.unwrap(), right);
            }

            if (right is TsValue rightValue)
            {
                return strictEquals(left, rightValue.unwrap());
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (tryReadNumber(left, out var leftNumber) && tryReadNumber(right, out var rightNumber))
            {
                return !double.IsNaN(leftNumber) && !double.IsNaN(rightNumber) && leftNumber.Equals(rightNumber);
            }

            if (left is string leftString && right is string rightString)
            {
                return string.Equals(leftString, rightString, System.StringComparison.Ordinal);
            }

            if (left is bool leftBool && right is bool rightBool)
            {
                return leftBool == rightBool;
            }

            return false;
        }

        private static bool sameValueZero(object? left, object? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is TsValue leftValue)
            {
                return sameValueZero(leftValue.unwrap(), right);
            }

            if (right is TsValue rightValue)
            {
                return sameValueZero(left, rightValue.unwrap());
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (tryReadNumber(left, out var leftNumber) && tryReadNumber(right, out var rightNumber))
            {
                return double.IsNaN(leftNumber) && double.IsNaN(rightNumber) || leftNumber.Equals(rightNumber);
            }

            if (left is string leftString && right is string rightString)
            {
                return string.Equals(leftString, rightString, System.StringComparison.Ordinal);
            }

            if (left is bool leftBool && right is bool rightBool)
            {
                return leftBool == rightBool;
            }

            return false;
        }

        private static bool tryReadNumber(object value, out double number)
        {
            switch (value)
            {
                case byte typed:
                    number = typed;
                    return true;
                case sbyte typed:
                    number = typed;
                    return true;
                case short typed:
                    number = typed;
                    return true;
                case ushort typed:
                    number = typed;
                    return true;
                case int typed:
                    number = typed;
                    return true;
                case uint typed:
                    number = typed;
                    return true;
                case long typed:
                    number = typed;
                    return true;
                case ulong typed:
                    number = typed;
                    return true;
                case float typed:
                    number = typed;
                    return true;
                case double typed:
                    number = typed;
                    return true;
                case decimal typed:
                    number = (double)typed;
                    return true;
                default:
                    number = 0;
                    return false;
            }
        }

        private static object canonicalizeBoxedKeyedCollectionKey(object value)
        {
            return value switch
            {
                double typed when typed == 0.0 => 0.0,
                float typed when typed == 0.0f => 0.0f,
                _ => value
            };
        }
    }
}
