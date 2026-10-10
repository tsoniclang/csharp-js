using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using NumericFormatting = Tsonic.CSharp.Js.Number;

namespace Tsonic.CSharp.Js
{
    public static partial class Globals
    {
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
        public static string String(Half value) => Tsonic.CSharp.Js.Number.toString(value);
        public static string String(float value) => Tsonic.CSharp.Js.Number.toString(value);
        public static string String(double value) => Tsonic.CSharp.Js.Number.toString(value);
        public static string String(decimal value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(System.Numerics.BigInteger value) => value.ToString(CultureInfo.InvariantCulture);
        public static string String(bool value) => value ? "true" : "false";
        public static string String(bool? value) => value.HasValue ? String(value.Value) : "null";
        public static string String<T>(T? value) where T : struct, System.Numerics.INumberBase<T> =>
            value.HasValue ? Tsonic.CSharp.Js.Number.toString(value.Value) : "null";

        public static string String<TValue>(TValue input)
        {
            object? value = unwrapClosedValue(input);

            if (value == null) return "null";
            if (value is string s) return s;
            if (value is bool b) return b ? "true" : "false";
            if (value is double number) return Tsonic.CSharp.Js.Number.toString(number);
            if (value is float single) return Tsonic.CSharp.Js.Number.toString(single);
            if (value is Tsonic.CSharp.Runtime.Error error)
                return error.message.Length == 0 ? error.name : error.name + ": " + error.message;
            if (value is IDynamicArray array)
            {
                var result = new StringBuilder();
                writeStringArray(array, result, null);
                return result.ToString();
            }
            if (value is IFormattable formatted) return formatted.ToString(null, CultureInfo.InvariantCulture);
            return value.ToString() ?? "";
        }


        private static void writeStringArray(IDynamicArray array, StringBuilder output, HashSet<object>? ancestors)
        {
            if (ancestors is not null && !ancestors.Add(array))
                throw new TypeError("String conversion does not support cyclic arrays");
            var visitor = new ArrayStringWriter(array, output, ancestors);
            try
            {
                array.VisitElements(ref visitor);
            }
            finally { visitor.Ancestors?.Remove(array); }
        }

        private static bool tryAppendNumeric<TValue, TNumber>(ref TValue input, StringBuilder output)
            where TNumber : struct, INumberBase<TNumber>
        {
            if (typeof(TValue) == typeof(TNumber))
            {
                NumericFormatting.AppendDecimal(Unsafe.As<TValue, TNumber>(ref input), output);
                return true;
            }
            if (typeof(TValue) != typeof(TNumber?)) return false;
            var optional = Unsafe.As<TValue, TNumber?>(ref input);
            if (optional.HasValue) NumericFormatting.AppendDecimal(optional.Value, output);
            return true;
        }

        private struct ArrayStringWriter(IDynamicArray array, StringBuilder output, HashSet<object>? ancestors) : IArrayElementVisitor
        {
            public HashSet<object>? Ancestors = ancestors;
            private int _written;

            public void Visit<TValue>(TValue input)
            {
                if (_written++ != 0) output.Append(',');
                appendValue(input);
            }

            private void appendValue<TValue>(TValue input)
            {
                if (tryAppendNumeric<TValue, sbyte>(ref input, output) ||
                    tryAppendNumeric<TValue, byte>(ref input, output) ||
                    tryAppendNumeric<TValue, short>(ref input, output) ||
                    tryAppendNumeric<TValue, ushort>(ref input, output) ||
                    tryAppendNumeric<TValue, int>(ref input, output) ||
                    tryAppendNumeric<TValue, uint>(ref input, output) ||
                    tryAppendNumeric<TValue, long>(ref input, output) ||
                    tryAppendNumeric<TValue, ulong>(ref input, output) ||
                    tryAppendNumeric<TValue, nint>(ref input, output) ||
                    tryAppendNumeric<TValue, nuint>(ref input, output) ||
                    tryAppendNumeric<TValue, Int128>(ref input, output) ||
                    tryAppendNumeric<TValue, UInt128>(ref input, output) ||
                    tryAppendNumeric<TValue, Half>(ref input, output) ||
                    tryAppendNumeric<TValue, float>(ref input, output) ||
                    tryAppendNumeric<TValue, double>(ref input, output) ||
                    tryAppendNumeric<TValue, decimal>(ref input, output) ||
                    tryAppendNumeric<TValue, System.Numerics.BigInteger>(ref input, output)) return;
                if (typeof(TValue) == typeof(bool))
                {
                    output.Append(Unsafe.As<TValue, bool>(ref input) ? "true" : "false");
                    return;
                }
                if (typeof(TValue) == typeof(bool?))
                {
                    var optional = Unsafe.As<TValue, bool?>(ref input);
                    if (optional.HasValue) output.Append(optional.Value ? "true" : "false");
                    return;
                }
                if (input is TsValue closed)
                {
                    appendValue(TsValue.UnwrapClosedValue(closed));
                    return;
                }
                object? wrapped = input;
                var value = unwrapClosedValue(wrapped);
                if (!ReferenceEquals(wrapped, value))
                {
                    appendValue(value);
                    return;
                }
                if (value is IDynamicArray nested)
                {
                    Ancestors ??= new HashSet<object>(ReferenceEqualityComparer.Instance) { array };
                    writeStringArray(nested, output, Ancestors);
                }
                else if (value is Tsonic.CSharp.Runtime.Error error)
                {
                    output.Append(error.name);
                    if (error.message.Length != 0) output.Append(": ").Append(error.message);
                }
                else if (value is double number) NumericFormatting.AppendDecimal(number, output);
                else if (value is float single) NumericFormatting.AppendDecimal(single, output);
                else if (value is Half half) NumericFormatting.AppendDecimal(half, output);
                else if (value is ISpanFormattable formatted) NumericFormatting.AppendNative(formatted, output);
                else if (value is not null) output.Append(String(value));
            }
        }
    }
}
