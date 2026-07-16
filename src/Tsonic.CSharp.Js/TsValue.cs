using System;
using System.Collections.Generic;
using System.Globalization;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Closed compatibility carrier for JavaScript values whose static TypeScript source type is broad.
    /// </summary>
    public readonly struct TsValue
    {
        private readonly object? _value;

        private TsValue(object? value)
        {
            _value = value;
        }

        public static TsValue from(object? value)
        {
            if (value is TsValue typed)
            {
                return typed;
            }

            if (!isSupported(value))
            {
                throw new NotSupportedException("TsValue requires a closed JavaScript runtime carrier.");
            }
            return new TsValue(value);
        }

        public static TsValue from<T1, T2>(Union<T1, T2>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3>(Union<T1, T2, T3>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3, T4>(Union<T1, T2, T3, T4>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3, T4, T5>(Union<T1, T2, T3, T4, T5>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3, T4, T5, T6>(Union<T1, T2, T3, T4, T5, T6>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3, T4, T5, T6, T7>(Union<T1, T2, T3, T4, T5, T6, T7>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue from<T1, T2, T3, T4, T5, T6, T7, T8>(Union<T1, T2, T3, T4, T5, T6, T7, T8>? value)
        {
            return value is null ? from(null) : from(TsUnion.From(value));
        }

        public static TsValue undefined()
        {
            return new TsValue(JSUndefined.value);
        }

        public object? unwrap()
        {
            return _value;
        }

        public TsValue ReadCompatSlot(string key)
        {
            return unwrapForOperation(_value) switch
            {
                JSUndefined => throw nullishReadError(key),
                null => throw nullishReadError(key),
                TsObject target => target.ReadCompatSlot(key),
                TsArray target => target.ReadCompatSlot(key),
                TsFunction target => target.ReadCompatSlot(key),
                Error target => readErrorSlot(target, key),
                JSObject target => target.hasOwnProperty(key) ? from(target[key]) : undefined(),
                IDictionary<string, object?> target => target.TryGetValue(key, out var value) ? from(value) : undefined(),
                IReadOnlyDictionary<string, object?> target => target.TryGetValue(key, out var value) ? from(value) : undefined(),
                string target when key == "length" => from(target.Length),
                IJSArray target when key == "length" => from(target.length),
                IJSArray target when tryReadArrayIndexKey(key, out var index) => target.tryGetAtObject(index, out var value) ? from(value) : undefined(),
                _ => undefined()
            };
        }

        public T ReadCompatSlotAs<T>(string key)
        {
            return CastCompat<T>(ReadCompatSlot(key));
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            var stored = from(value);
            switch (unwrapForOperation(_value))
            {
                case JSUndefined:
                    throw nullishWriteError(key);
                case null:
                    throw nullishWriteError(key);
                case TsObject target:
                    return target.WriteCompatSlot(key, stored);
                case TsArray target:
                    return target.WriteCompatSlot(key, stored);
                case TsFunction target:
                    return target.WriteCompatSlot(key, stored);
                case IJSArray target when key == "length":
                    target.setLength(toArrayIndex(stored.unwrap()));
                    return from(target.length);
                case IJSArray target when tryReadArrayIndexKey(key, out var index):
                    if (!target.trySetAtObject(index, stored.unwrap()))
                    {
                        throw new TypeError($"Cannot store value in closed JavaScript array index '{key}' because the element carrier is incompatible.");
                    }
                    return stored;
                case JSObject target:
                    target[key] = stored.unwrap();
                    return stored;
                case IDictionary<string, object?> target:
                    target[key] = stored.unwrap();
                    return stored;
                default:
                    throw new TypeError($"Cannot set property '{key}' on a closed non-object JavaScript carrier.");
            }
        }

        public TsValue ReadCompatElement(object? key)
        {
            return ReadCompatSlot(propertyKey(key));
        }

        public TsValue WriteCompatElement(object? key, object? value)
        {
            return WriteCompatSlot(propertyKey(key), value);
        }

        public TsValue InvokeCompat(params object?[] arguments)
        {
            return unwrapForOperation(_value) is TsFunction target
                ? target.InvokeCompat(arguments)
                : throw new TypeError("Value is not callable.");
        }

        public TsValue ConstructCompat(params object?[] arguments)
        {
            return unwrapForOperation(_value) is TsFunction target
                ? target.ConstructCompat(arguments)
                : throw new TypeError("Value is not a constructor.");
        }

        public static TsValue ApplyCompatBinary(object? left, string op, object? right)
        {
            return op switch
            {
                "+" => plus(left, right),
                "-" => from(toNumber(left) - toNumber(right)),
                "*" => from(toNumber(left) * toNumber(right)),
                "/" => from(toNumber(left) / toNumber(right)),
                "%" => from(toNumber(left) % toNumber(right)),
                "??" => isNullish(left) ? from(right) : from(left),
                "&&" => truthy(left) ? from(right) : from(left),
                "||" => truthy(left) ? from(left) : from(right),
                _ => throw unsupportedOperator(op)
            };
        }

        public static bool ApplyCompatBinaryBoolean(object? left, string op, object? right)
        {
            return op switch
            {
                "==" => looseEquals(left, right),
                "!=" => !looseEquals(left, right),
                "===" => strictEquals(left, right),
                "!==" => !strictEquals(left, right),
                "<" => tryCompare(left, right, out var lessThanComparison) && lessThanComparison < 0,
                "<=" => tryCompare(left, right, out var lessThanOrEqualComparison) && lessThanOrEqualComparison <= 0,
                ">" => tryCompare(left, right, out var greaterThanComparison) && greaterThanComparison > 0,
                ">=" => tryCompare(left, right, out var greaterThanOrEqualComparison) && greaterThanOrEqualComparison >= 0,
                _ => throw unsupportedOperator(op)
            };
        }

        public static TsValue ApplyCompatUnary(object? operand, string op)
        {
            return op switch
            {
                "+" => from(toNumber(operand)),
                "-" => from(-toNumber(operand)),
                "~" => from(~(int)toNumber(operand)),
                _ => throw unsupportedOperator(op)
            };
        }

        public static bool ApplyCompatUnaryBoolean(object? operand, string op)
        {
            return op switch
            {
                "!" => !truthy(operand),
                _ => throw unsupportedOperator(op)
            };
        }

        public static TsValue ApplyCompatVoid(object? operand)
        {
            _ = operand;
            return undefined();
        }

        public static string ApplyCompatTypeof(object? operand)
        {
            var unwrapped = unwrapForOperation(operand);
            return unwrapped switch
            {
                JSUndefined => "undefined",
                null => "object",
                bool => "boolean",
                string => "string",
                double or float or decimal or int or long or uint or ulong or byte or sbyte or short or ushort => "number",
                TsFunction => "function",
                _ => "object"
            };
        }

        public static T CastCompat<T>(object? value)
        {
            if (TryCastCompat<T>(value, out var typed))
            {
                return typed;
            }
            var unwrapped = unwrapForOperation(value);
            if (unwrapped is null or JSUndefined)
            {
                throw new TypeError("Cannot cast null or undefined to the requested closed value carrier.");
            }
            throw new TypeError("TsValue cannot cross the requested typed boundary because the closed carrier value is not assignable.");
        }

        internal static bool TryCastCompat<T>(object? value, out T result)
        {
            var carrier = UnwrapCompatCarrier(value);
            if (carrier is T direct)
            {
                result = direct;
                return true;
            }
            var unwrapped = unwrapForOperation(carrier);
            if (unwrapped is T typed)
            {
                result = typed;
                return true;
            }
            if ((unwrapped is null or JSUndefined) && default(T) is null)
            {
                result = default!;
                return true;
            }
            result = default!;
            return false;
        }

        private static bool isSupported(object? value)
        {
            return value switch
            {
                null => true,
                bool => true,
                string => true,
                double => true,
                float => true,
                decimal => true,
                int => true,
                long => true,
                uint => true,
                ulong => true,
                byte => true,
                sbyte => true,
                short => true,
                ushort => true,
                JSObject => true,
                IJSArray => true,
                TsObject => true,
                TsArray => true,
                TsUnion => true,
                TsFunction => true,
                Error => true,
                JSUndefined => true,
                IDictionary<string, object?> => true,
                IReadOnlyDictionary<string, object?> => true,
                _ => false
            };
        }

        private static TsValue plus(object? left, object? right)
        {
            var leftValue = unwrapForOperation(left);
            var rightValue = unwrapForOperation(right);
            return leftValue is string || rightValue is string
                ? from(toJsString(leftValue) + toJsString(rightValue))
                : from(toNumber(leftValue) + toNumber(rightValue));
        }

        private static bool tryCompare(object? left, object? right, out int comparison)
        {
            var leftValue = unwrapForOperation(left);
            var rightValue = unwrapForOperation(right);
            if (leftValue is string leftText && rightValue is string rightText)
            {
                comparison = string.CompareOrdinal(leftText, rightText);
                return true;
            }
            var leftNumber = toNumber(leftValue);
            var rightNumber = toNumber(rightValue);
            if (double.IsNaN(leftNumber) || double.IsNaN(rightNumber))
            {
                comparison = 0;
                return false;
            }
            comparison = leftNumber.CompareTo(rightNumber);
            return true;
        }

        private static bool looseEquals(object? left, object? right)
        {
            var leftValue = unwrapForOperation(left);
            var rightValue = unwrapForOperation(right);
            if (isNullish(leftValue) && isNullish(rightValue))
            {
                return true;
            }
            if (isNullish(leftValue) || isNullish(rightValue))
            {
                return false;
            }
            if (leftValue is bool || rightValue is bool || isNumeric(leftValue) || isNumeric(rightValue))
            {
                var leftNumber = toNumber(leftValue);
                var rightNumber = toNumber(rightValue);
                return !double.IsNaN(leftNumber) && !double.IsNaN(rightNumber) && leftNumber == rightNumber;
            }
            return strictEquals(leftValue, rightValue);
        }

        private static bool strictEquals(object? left, object? right)
        {
            var leftValue = unwrapForOperation(left);
            var rightValue = unwrapForOperation(right);
            if (ReferenceEquals(leftValue, rightValue))
            {
                return true;
            }
            if (isNullish(leftValue) || isNullish(rightValue))
            {
                return false;
            }
            if (isNumeric(leftValue) && isNumeric(rightValue))
            {
                var leftNumber = toNumber(leftValue);
                var rightNumber = toNumber(rightValue);
                return !double.IsNaN(leftNumber) && !double.IsNaN(rightNumber) && leftNumber == rightNumber;
            }
            return leftValue!.GetType() == rightValue!.GetType() && Equals(leftValue, rightValue);
        }

        private static bool truthy(object? value)
        {
            var unwrapped = unwrapForOperation(value);
            return unwrapped switch
            {
                null => false,
                JSUndefined => false,
                bool boolean => boolean,
                string text => text.Length > 0,
                double number => number != 0 && !double.IsNaN(number),
                float number => number != 0 && !float.IsNaN(number),
                decimal number => number != 0,
                int number => number != 0,
                long number => number != 0,
                uint number => number != 0,
                ulong number => number != 0,
                byte number => number != 0,
                sbyte number => number != 0,
                short number => number != 0,
                ushort number => number != 0,
                _ => true
            };
        }

        private static double toNumber(object? value)
        {
            var unwrapped = unwrapForOperation(value);
            return unwrapped switch
            {
                null => 0,
                JSUndefined => double.NaN,
                bool boolean => boolean ? 1 : 0,
                string text => parseNumber(text),
                double number => number,
                float number => number,
                decimal number => (double)number,
                int number => number,
                long number => number,
                uint number => number,
                ulong number => number,
                byte number => number,
                sbyte number => number,
                short number => number,
                ushort number => number,
                _ => double.NaN
            };
        }

        private static double parseNumber(string text)
        {
            var trimmed = text.Trim();
            if (trimmed.Length == 0)
            {
                return 0;
            }
            return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                ? number
                : double.NaN;
        }

        private static string toJsString(object? value)
        {
            var unwrapped = unwrapForOperation(value);
            return unwrapped switch
            {
                null => "null",
                JSUndefined => "undefined",
                bool boolean => boolean ? "true" : "false",
                string text => text,
                double number => number.ToString(CultureInfo.InvariantCulture),
                float number => number.ToString(CultureInfo.InvariantCulture),
                decimal number => number.ToString(CultureInfo.InvariantCulture),
                int number => number.ToString(CultureInfo.InvariantCulture),
                long number => number.ToString(CultureInfo.InvariantCulture),
                uint number => number.ToString(CultureInfo.InvariantCulture),
                ulong number => number.ToString(CultureInfo.InvariantCulture),
                byte number => number.ToString(CultureInfo.InvariantCulture),
                sbyte number => number.ToString(CultureInfo.InvariantCulture),
                short number => number.ToString(CultureInfo.InvariantCulture),
                ushort number => number.ToString(CultureInfo.InvariantCulture),
                _ => "[object Object]"
            };
        }

        private static TsValue readErrorSlot(Error error, string key)
        {
            return key switch
            {
                "name" => from(error.name),
                "message" => from(error.message),
                "stack" => error.stack is null ? undefined() : from(error.stack),
                _ => undefined()
            };
        }

        private static bool isNullish(object? value)
        {
            var unwrapped = unwrapForOperation(value);
            return unwrapped is null or JSUndefined;
        }

        private static bool isNumeric(object? value)
        {
            var unwrapped = unwrapForOperation(value);
            return unwrapped is double or float or decimal or int or long or uint or ulong or byte or sbyte or short or ushort;
        }

        internal static object? UnwrapCompatCarrier(object? value)
        {
            var unwrapped = value is TsValue typed ? typed._value : value;
            if (!isSupported(unwrapped))
            {
                throw closedCarrierError();
            }
            return unwrapped;
        }

        private static object? unwrapForOperation(object? value)
        {
            var carrier = UnwrapCompatCarrier(value);
            return carrier is TsUnion union ? unwrapForOperation(union.value()) : carrier;
        }

        private static NotSupportedException unsupportedOperator(string op)
        {
            return new NotSupportedException($"TsValue does not support JavaScript operator '{op}' in the closed compatibility runtime.");
        }

        internal static string propertyKey(object? key)
        {
            key = unwrapForOperation(key);
            return key switch
            {
                JSUndefined => "undefined",
                null => "null",
                string value => value,
                bool value => value ? "true" : "false",
                byte value => value.ToString(CultureInfo.InvariantCulture),
                sbyte value => value.ToString(CultureInfo.InvariantCulture),
                short value => value.ToString(CultureInfo.InvariantCulture),
                ushort value => value.ToString(CultureInfo.InvariantCulture),
                int value => value.ToString(CultureInfo.InvariantCulture),
                uint value => value.ToString(CultureInfo.InvariantCulture),
                long value => value.ToString(CultureInfo.InvariantCulture),
                ulong value => value.ToString(CultureInfo.InvariantCulture),
                float value => value.ToString(CultureInfo.InvariantCulture),
                double value => value.ToString(CultureInfo.InvariantCulture),
                decimal value => value.ToString(CultureInfo.InvariantCulture),
                _ => throw new TypeError("Only closed primitive property keys are supported by TsValue.")
            };
        }

        private static NotSupportedException closedCarrierError()
        {
            return new NotSupportedException("TsValue requires a closed JavaScript runtime carrier.");
        }

        private static bool tryReadArrayIndexKey(string key, out int index)
        {
            if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out index))
            {
                return false;
            }

            return index >= 0 && key == index.ToString(CultureInfo.InvariantCulture);
        }

        private static int toArrayIndex(object? value)
        {
            var key = propertyKey(value);
            return tryReadArrayIndexKey(key, out var index)
                ? index
                : throw new RangeError("Invalid array length.");
        }

        private static TypeError nullishReadError(string key)
        {
            return new TypeError($"Cannot read property '{key}' of null or undefined.");
        }

        private static TypeError nullishWriteError(string key)
        {
            return new TypeError($"Cannot set property '{key}' of null or undefined.");
        }
    }
}
