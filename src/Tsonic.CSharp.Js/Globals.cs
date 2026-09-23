/**
 * Global functions available at Tsonic.CSharp.Js root level
 */

using System;
using System.Globalization;
using System.Text;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Global functions (parseInt, parseFloat, encoding, etc.)
    /// </summary>
    public static class Globals
    {
        private static readonly UTF8Encoding StrictUtf8 = new(false, true);

        // Global constants
        public const double Infinity = double.PositiveInfinity;
        public const double NaN = double.NaN;
        public static readonly object undefined = Undefined.value;

        public static double parseInt(string str, int? radix = null) => Tsonic.CSharp.Js.Number.parseInt(str, radix);

        public static double parseFloat(string str) => Tsonic.CSharp.Js.Number.parseFloat(str);

        /// <summary>
        /// Check if value is NaN
        /// </summary>
        public static bool isNaN<T>(T value) where T : System.Numerics.INumberBase<T> => Tsonic.CSharp.Js.Number.isNaN(value);

        public static bool isNaN<T>(T? value) where T : struct, System.Numerics.INumberBase<T> => Tsonic.CSharp.Js.Number.isNaN(value);

        public static bool isFinite<T>(T value) where T : System.Numerics.INumberBase<T> => Tsonic.CSharp.Js.Number.isFinite(value);

        public static bool isFinite<T>(T? value) where T : struct, System.Numerics.INumberBase<T> => Tsonic.CSharp.Js.Number.isFinite(value);

        /// <summary>
        /// Helper method to percent-encode a rune (Unicode scalar value)
        /// </summary>
        private static void AppendPercentEncoded(StringBuilder sb, Rune rune)
        {
            Span<byte> utf8Bytes = stackalloc byte[4];
            var bytesWritten = rune.EncodeToUtf8(utf8Bytes);
            for (var i = 0; i < bytesWritten; i++)
            {
                sb.Append('%');
                sb.Append(utf8Bytes[i].ToString("X2"));
            }
        }

        /// <summary>
        /// Check if character should NOT be encoded by encodeURI
        /// Per spec: A-Z a-z 0-9 ; , / ? : @ & = + $ - _ . ! ~ * ' ( ) #
        /// </summary>
        private static bool IsUriUnescaped(Rune rune)
        {
            var value = rune.Value;
            return (value >= 'A' && value <= 'Z') ||
                   (value >= 'a' && value <= 'z') ||
                   (value >= '0' && value <= '9') ||
                   value == ';' || value == ',' || value == '/' || value == '?' ||
                   value == ':' || value == '@' || value == '&' || value == '=' ||
                   value == '+' || value == '$' || value == '-' || value == '_' ||
                   value == '.' || value == '!' || value == '~' || value == '*' ||
                   value == '\'' || value == '(' || value == ')' || value == '#';
        }

        /// <summary>
        /// Check if character should NOT be encoded by encodeURIComponent
        /// Per spec: A-Z a-z 0-9 - _ . ! ~ * ' ( )
        /// </summary>
        private static bool IsComponentUnescaped(Rune rune)
        {
            var value = rune.Value;
            return (value >= 'A' && value <= 'Z') ||
                   (value >= 'a' && value <= 'z') ||
                   (value >= '0' && value <= '9') ||
                   value == '-' || value == '_' || value == '.' ||
                   value == '!' || value == '~' || value == '*' ||
                   value == '\'' || value == '(' || value == ')';
        }

        /// <summary>
        /// Encode URI component (encodes all except: A-Z a-z 0-9 - _ . ! ~ * ' ( ))
        /// </summary>
        public static string encodeURIComponent(string component)
        {
            var sb = new StringBuilder(component.Length);
            foreach (var rune in component.EnumerateRunes())
            {
                if (IsComponentUnescaped(rune))
                {
                    sb.Append(rune);
                }
                else
                {
                    AppendPercentEncoded(sb, rune);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decode URI component
        /// </summary>
        public static string decodeURIComponent(string component)
        {
            return DecodeUriText(component, preserveReserved: false);
        }

        /// <summary>
        /// Encode URI (preserves URI reserved characters: ; , / ? : @ & = + $ #)
        /// </summary>
        public static string encodeURI(string uri)
        {
            var sb = new StringBuilder(uri.Length);
            foreach (var rune in uri.EnumerateRunes())
            {
                if (IsUriUnescaped(rune))
                {
                    sb.Append(rune);
                }
                else
                {
                    AppendPercentEncoded(sb, rune);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decode URI
        /// </summary>
        public static string decodeURI(string uri)
        {
            return DecodeUriText(uri, preserveReserved: true);
        }

        private static string DecodeUriText(string value, bool preserveReserved)
        {
            var output = new StringBuilder(value.Length);
            Span<byte> encoded = stackalloc byte[4];
            for (var index = 0; index < value.Length;)
            {
                if (value[index] != '%')
                {
                    output.Append(value[index]);
                    index++;
                    continue;
                }

                encoded[0] = ParsePercentEncodedByte(value, index);
                var byteCount = encoded[0] switch
                {
                    <= 0x7f => 1,
                    >= 0xc2 and <= 0xdf => 2,
                    >= 0xe0 and <= 0xef => 3,
                    >= 0xf0 and <= 0xf4 => 4,
                    _ => throw InvalidUriEncoding()
                };

                for (var byteIndex = 1; byteIndex < byteCount; byteIndex++)
                {
                    encoded[byteIndex] = ParsePercentEncodedByte(
                        value,
                        index + byteIndex * 3);
                }

                if (preserveReserved && byteCount == 1 && IsUriReserved((char)encoded[0]))
                {
                    output.Append(value, index, 3);
                }
                else
                {
                    try
                    {
                        output.Append(StrictUtf8.GetString(encoded[..byteCount]));
                    }
                    catch (DecoderFallbackException exception)
                    {
                        throw new URIError("URI contains malformed UTF-8 data.", exception);
                    }
                }

                index += byteCount * 3;
            }
            return output.ToString();
        }

        private static byte ParsePercentEncodedByte(string value, int index)
        {
            if (index + 2 >= value.Length || value[index] != '%')
            {
                throw InvalidUriEncoding();
            }
            var high = HexDigit(value[index + 1]);
            var low = HexDigit(value[index + 2]);
            if (high < 0 || low < 0)
            {
                throw InvalidUriEncoding();
            }
            return (byte)(high * 16 + low);
        }

        private static int HexDigit(char value)
        {
            return value switch
            {
                >= '0' and <= '9' => value - '0',
                >= 'A' and <= 'F' => value - 'A' + 10,
                >= 'a' and <= 'f' => value - 'a' + 10,
                _ => -1
            };
        }

        private static bool IsUriReserved(char value)
        {
            return value is ';' or '/' or '?' or ':' or '@' or '&' or '=' or '+' or '$' or ',' or '#';
        }

        private static URIError InvalidUriEncoding()
        {
            return new URIError("URI contains an invalid percent-encoded sequence.");
        }

        /// <summary>
        /// Convert value to number
        /// </summary>
        public static double Number<TFirst, TSecond>(Union<TFirst, TSecond> value) =>
            value.Match(part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird>(Union<TFirst, TSecond, TThird> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird, TFourth>(Union<TFirst, TSecond, TThird, TFourth> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird, TFourth, TFifth>(Union<TFirst, TSecond, TThird, TFourth, TFifth> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part));

        public static double Number<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth> value) =>
            value.Match(part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part), part => Number(part));

        public static double Number(sbyte value) => (double)value;
        public static double Number(byte value) => (double)value;
        public static double Number(short value) => (double)value;
        public static double Number(ushort value) => (double)value;
        public static double Number(int value) => (double)value;
        public static double Number(uint value) => (double)value;
        public static double Number(long value) => (double)value;
        public static double Number(ulong value) => (double)value;
        public static double Number(nint value) => (double)value;
        public static double Number(nuint value) => (double)value;
        public static double Number(Int128 value) => (double)value;
        public static double Number(UInt128 value) => (double)value;
        public static double Number(Half value) => (double)value;
        public static double Number(float value) => (double)value;
        public static double Number(double value) => (double)value;
        public static double Number(decimal value) => (double)value;
        public static double Number(System.Numerics.BigInteger value) => (double)value;
        public static double Number(bool value) => value ? 1 : 0;

        public static double Number(object? value = null)
        {
            value = unwrapClosedValue(value);

            if (value == null) return 0;
            if (value is Undefined) return double.NaN;

            if (value is double d) return d;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is float f) return f;
            if (value is decimal dec) return (double)dec;
            if (value is System.Numerics.BigInteger bigInteger) return (double)bigInteger;
            if (value is ulong unsignedInteger) return unsignedInteger;
            if (value is Int128 wideInteger) return (double)wideInteger;
            if (value is UInt128 wideUnsignedInteger) return (double)wideUnsignedInteger;
            if (value is sbyte signedByte) return signedByte;
            if (value is byte unsignedByte) return unsignedByte;
            if (value is short signedShort) return signedShort;
            if (value is ushort unsignedShort) return unsignedShort;
            if (value is uint unsignedWord) return unsignedWord;
            if (value is nint signedNative) return signedNative;
            if (value is nuint unsignedNative) return unsignedNative;
            if (value is Half half) return (double)half;
            if (value is bool b) return b ? 1 : 0;

            if (value is string str)
            {
                return Tsonic.CSharp.Js.Number.parseFloat(str);
            }

            return double.NaN;
        }

        /// <summary>
        /// Convert value to string
        /// </summary>
        public static string String()
        {
            return "";
        }

        public static string String<T1, T2>(Tsonic.CSharp.Runtime.Union<T1, T2>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String);

        public static string String<T1, T2, T3>(Tsonic.CSharp.Runtime.Union<T1, T2, T3>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String);

        public static string String<T1, T2, T3, T4>(Tsonic.CSharp.Runtime.Union<T1, T2, T3, T4>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String, String);

        public static string String<T1, T2, T3, T4, T5>(Tsonic.CSharp.Runtime.Union<T1, T2, T3, T4, T5>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String, String, String);

        public static string String<T1, T2, T3, T4, T5, T6>(Tsonic.CSharp.Runtime.Union<T1, T2, T3, T4, T5, T6>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String, String, String, String);

        public static string String<T1, T2, T3, T4, T5, T6, T7>(Tsonic.CSharp.Runtime.Union<T1, T2, T3, T4, T5, T6, T7>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String, String, String, String, String);

        public static string String<T1, T2, T3, T4, T5, T6, T7, T8>(Tsonic.CSharp.Runtime.Union<T1, T2, T3, T4, T5, T6, T7, T8>? value)
            => value is null ? "null" : value.Value.Match<string>(String, String, String, String, String, String, String, String);

        public static string String(sbyte value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(byte value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(short value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(ushort value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(int value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(uint value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(long value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(ulong value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(nint value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(nuint value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(Int128 value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(UInt128 value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(Half value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(float value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(double value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(decimal value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(System.Numerics.BigInteger value) => value.ToString(CultureInfo.InvariantCulture);

        public static string String<TValue>(TValue input)
        {
            object? value = unwrapClosedValue(input);
            while (value is Tsonic.CSharp.Runtime.TsUnion union)
            {
                value = union.unwrap();
            }

            if (value == null || value is Tsonic.CSharp.Runtime.Null) return "null";
            if (value is Undefined) return "undefined";
            if (value is string s) return s;
            if (value is bool b) return b ? "true" : "false";
            if (value is double number) return Tsonic.CSharp.Js.Number.toString(number);
            if (value is float single) return Tsonic.CSharp.Js.Number.toString(single);
            if (value is IFormattable formatted) return formatted.ToString(null, CultureInfo.InvariantCulture);
            return value.ToString() ?? "";
        }

        /// <summary>
        /// Convert value to boolean
        /// </summary>
        public static bool Boolean(object? value = null)
        {
            value = unwrapClosedValue(value);

            if (value == null) return false;
            if (value is Undefined) return false;

            if (value is bool b) return b;
            if (value is string s) return s.Length > 0;
            if (value is double d)
            {
                if (double.IsNaN(d)) return false;
                return d != 0;
            }
            if (value is int i) return i != 0;
            if (value is long l) return l != 0;
            if (value is float f) return f != 0;
            if (value is decimal dec) return dec != 0;

            return true; // Objects are truthy
        }

        private static object? unwrapClosedValue(object? value)
        {
            return value is TsValue tsValue ? tsValue.unwrap() : value;
        }
    }
}
