using System;
using System.Collections.Generic;

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
            if (!isSupported(value))
            {
                throw new NotSupportedException("TsValue requires a closed JavaScript runtime carrier.");
            }
            return new TsValue(value);
        }

        public object? unwrap()
        {
            return _value;
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
                IDictionary<string, object?> => true,
                IReadOnlyDictionary<string, object?> => true,
                _ => false
            };
        }
    }
}
