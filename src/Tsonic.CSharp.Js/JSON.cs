/**
 * JavaScript JSON object implementation
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JSON parsing and stringification (AOT-friendly, no reflection)
    /// </summary>
    public static class JSON
    {
        /// <summary>
        /// Parse JSON string to a closed JavaScript value carrier.
        /// </summary>
        public static TsValue parse(string text)
        {
            using var doc = JsonDocument.Parse(text);
            return TsValue.from(ConvertJsonElement(doc.RootElement));
        }

        /// <summary>
        /// Convert JsonElement to runtime objects
        /// </summary>
        private static object? ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Array => ConvertJsonArray(element),
                JsonValueKind.Object => ConvertJsonObject(element),
                _ => null
            };
        }

        /// <summary>
        /// Convert JSON array to a closed JavaScript array carrier.
        /// </summary>
        private static object ConvertJsonArray(JsonElement element)
        {
            var items = new JSArray<object?>();
            foreach (var item in element.EnumerateArray())
            {
                items.push(ConvertJsonElement(item));
            }
            return items;
        }

        /// <summary>
        /// Convert JSON object to JSObject
        /// </summary>
        private static object ConvertJsonObject(JsonElement element)
        {
            var obj = new JSObject();
            foreach (var prop in element.EnumerateObject())
            {
                obj[prop.Name] = ConvertJsonElement(prop.Value);
            }
            return obj;
        }

        /// <summary>
        /// Convert a closed JavaScript value carrier to JSON string.
        /// </summary>
        public static string stringify(object? value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            WriteValue(writer, value);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string stringify(TsValue value)
        {
            return stringify(value.unwrap());
        }

        public static string stringify<TValue>(IDictionary<string, TValue>? value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            WriteObject(writer, value);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string stringify<TValue>(IReadOnlyDictionary<string, TValue>? value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            WriteObject(writer, value);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Write value to Utf8JsonWriter
        /// </summary>
        private static void WriteValue(Utf8JsonWriter writer, object? value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case bool b:
                    writer.WriteBooleanValue(b);
                    break;
                case string s:
                    writer.WriteStringValue(s);
                    break;
                case double d:
                    writer.WriteNumberValue(d);
                    break;
                case float f:
                    writer.WriteNumberValue(f);
                    break;
                case int i:
                    writer.WriteNumberValue(i);
                    break;
                case long l:
                    writer.WriteNumberValue(l);
                    break;
                case uint ui:
                    writer.WriteNumberValue(ui);
                    break;
                case byte bt:
                    writer.WriteNumberValue(bt);
                    break;
                case short sh:
                    writer.WriteNumberValue(sh);
                    break;
                case JSObject obj:
                    WriteJsObject(writer, obj);
                    break;
                case IJSArray array:
                    WriteJsArray(writer, array);
                    break;
                case IDictionary<string, object?> dict:
                    WriteObject(writer, dict);
                    break;
                case IReadOnlyDictionary<string, object?> dict:
                    WriteObject(writer, dict);
                    break;
                default:
                    throw new NotSupportedException("JSON.stringify requires a closed JS value carrier.");
            }
        }

        /// <summary>
        /// Write JSObject as JSON object
        /// </summary>
        private static void WriteJsObject(Utf8JsonWriter writer, JSObject obj)
        {
            writer.WriteStartObject();
            foreach (var (key, value) in obj.entries())
            {
                writer.WritePropertyName(key);
                WriteValue(writer, value);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Write dictionary as JSON object
        /// </summary>
        private static void WriteObject(Utf8JsonWriter writer, IDictionary<string, object?> dict)
        {
            writer.WriteStartObject();
            foreach (var kvp in dict)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }

        private static void WriteObject(Utf8JsonWriter writer, IReadOnlyDictionary<string, object?> dict)
        {
            writer.WriteStartObject();
            foreach (var kvp in dict)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }

        private static void WriteObject<TValue>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, TValue>>? dict)
        {
            if (dict == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            foreach (var kvp in dict)
            {
                writer.WritePropertyName(kvp.Key);
                WriteValue(writer, kvp.Value);
            }
            writer.WriteEndObject();
        }

        private static void WriteJsArray(Utf8JsonWriter writer, IJSArray array)
        {
            writer.WriteStartArray();
            for (var index = 0; index < array.length; index++)
            {
                if (array.tryGetAtObject(index, out var item))
                {
                    WriteValue(writer, item);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
            writer.WriteEndArray();
        }
    }
}
