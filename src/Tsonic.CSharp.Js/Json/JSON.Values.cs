using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Tsonic.CSharp.Js
{
    public static partial class JSON
    {
        public static void writeValue(
            Utf8JsonWriter writer,
            object? value,
            JsonWriteContext context,
            string key = "")
        {
            value = NormalizeDirectJsonValue(value);
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case bool boolean:
                    writer.WriteBooleanValue(boolean);
                    break;
                case string text:
                    writer.WriteStringValue(text);
                    break;
                case double number:
                    writer.WriteNumberValue(number);
                    break;
                case float single:
                    writer.WriteNumberValue(single);
                    break;
                case int integer:
                    writer.WriteNumberValue(integer);
                    break;
                case long signedLong:
                    writer.WriteNumberValue(signedLong);
                    break;
                case uint unsignedInteger:
                    writer.WriteNumberValue(unsignedInteger);
                    break;
                case ulong unsignedLong:
                    writer.WriteNumberValue(unsignedLong);
                    break;
                case byte unsignedByte:
                    writer.WriteNumberValue(unsignedByte);
                    break;
                case sbyte signedByte:
                    writer.WriteNumberValue(signedByte);
                    break;
                case short signedShort:
                    writer.WriteNumberValue(signedShort);
                    break;
                case ushort unsignedShort:
                    writer.WriteNumberValue(unsignedShort);
                    break;
                case nint nativeInteger:
                    writer.WriteNumberValue((long)nativeInteger);
                    break;
                case nuint nativeUnsignedInteger:
                    writer.WriteNumberValue((ulong)nativeUnsignedInteger);
                    break;
                case Int128 wideInteger:
                    WriteWideInteger(writer, wideInteger);
                    break;
                case UInt128 wideUnsignedInteger:
                    WriteWideInteger(writer, wideUnsignedInteger);
                    break;
                case Half half:
                    writer.WriteNumberValue((float)half);
                    break;
                case decimal precise:
                    writer.WriteNumberValue(precise);
                    break;
                case JSObject record:
                    WriteJsObject(writer, record, context);
                    break;
                case IDynamicArray array:
                    WriteJsArray(writer, array, context);
                    break;
                case IJsonValue jsonValue:
                    WriteJsonValue(writer, jsonValue, context, key);
                    break;
                case IDictionary<string, object?> dictionary:
                    WriteObject(writer, dictionary, context);
                    break;
                case IReadOnlyDictionary<string, object?> dictionary:
                    WriteObject(writer, dictionary, context);
                    break;
                default:
                    throw new NotSupportedException("JSON.stringify requires a closed JS value carrier.");
            }
        }

        private static void WriteWideInteger<T>(Utf8JsonWriter writer, T value)
            where T : IBinaryInteger<T>, IUtf8SpanFormattable
        {
            Span<byte> buffer = stackalloc byte[40];
            if (!value.TryFormat(buffer, out var written, default, CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Native 128-bit decimal formatting exceeded its exact bound.");
            writer.WriteRawValue(buffer[..written], skipInputValidation: true);
        }
    }
}
