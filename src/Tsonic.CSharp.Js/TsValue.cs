using System;
using System.Collections.Generic;
using System.Globalization;

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
            return _value switch
            {
                JSUndefined => throw nullishReadError(key),
                null => throw nullishReadError(key),
                TsObject target => target.ReadCompatSlot(key),
                TsArray target => target.ReadCompatSlot(key),
                TsFunction target => target.ReadCompatSlot(key),
                JSObject target => target.hasOwnProperty(key) ? from(target[key]) : undefined(),
                IDictionary<string, object?> target => target.TryGetValue(key, out var value) ? from(value) : undefined(),
                IReadOnlyDictionary<string, object?> target => target.TryGetValue(key, out var value) ? from(value) : undefined(),
                string target when key == "length" => from(target.Length),
                IJSArray target when key == "length" => from(target.length),
                _ => undefined()
            };
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            var stored = from(value);
            switch (_value)
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
            return _value is TsFunction target
                ? target.InvokeCompat(arguments)
                : throw new TypeError("Value is not callable.");
        }

        public TsValue ConstructCompat(params object?[] arguments)
        {
            return _value is TsFunction target
                ? target.ConstructCompat(arguments)
                : throw new TypeError("Value is not a constructor.");
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
                TsFunction => true,
                JSUndefined => true,
                IDictionary<string, object?> => true,
                IReadOnlyDictionary<string, object?> => true,
                _ => false
            };
        }

        internal static string propertyKey(object? key)
        {
            return key switch
            {
                TsValue value => propertyKey(value.unwrap()),
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
