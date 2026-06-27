namespace Tsonic.CSharp.Js
{
    internal static class JSKeyEquality
    {
        public static bool sameValueZero<T>(T left, T right)
        {
            return sameValueZero((object?)left, (object?)right);
        }

        public static bool strictEquals<T>(T left, T right)
        {
            return strictEquals((object?)left, (object?)right);
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
    }
}
