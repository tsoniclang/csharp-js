/**
 * JavaScript JSON object implementation
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JSON parsing and stringification (AOT-friendly, no reflection)
    /// </summary>
    public static partial class JSON
    {
        /// <summary>
        /// Parse JSON string to a closed JavaScript value carrier.
        /// </summary>
        public static TsValue parse(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(text));
            if (!reader.Read()) throw new JsonException("JSON input is empty.");
            var value = ReadValue(ref reader);
            if (reader.Read()) throw new JsonException("JSON input contains trailing content.");
            return TsValue.from(value);
        }

        private static object? ReadValue(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null: return null;
                case JsonTokenType.True: return true;
                case JsonTokenType.False: return false;
                case JsonTokenType.Number: return reader.GetDouble();
                case JsonTokenType.String: return reader.GetString();
                case JsonTokenType.StartArray:
                    var items = new JSArray<object?>();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray) return items;
                        items.push(ReadValue(ref reader));
                    }
                    break;
                case JsonTokenType.StartObject:
                    var result = new JSObject();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject) return result;
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected a JSON property name.");
                        var name = reader.GetString()!;
                        if (!reader.Read()) break;
                        result[name] = ReadValue(ref reader);
                    }
                    break;
            }
            throw new JsonException("Incomplete or invalid JSON value.");
        }

        public static string stringify(object? value)
        {
            value = NormalizeDirectJsonValue(value);
            var stream = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(stream);
            writeValue(writer, value, new JsonWriteContext(), "");
            writer.Flush();
            return Encoding.UTF8.GetString(stream.WrittenSpan);
        }

        public static string stringify(TsValue value)
        {
            return stringify(value.unwrap());
        }

        public static string stringify(object? value, object? replacer, TsValue space = default)
        {
            replacer = replacer is TsValue wrapped ? wrapped.unwrap() : replacer;
            object? selected;
            switch (replacer)
            {
                case null:
                    selected = NormalizeJsonValue(value, "");
                    break;
                case JsonReplacer callback:
                    selected = ApplyReplacer("", value, callback, new JsonWriteContext()).unwrap();
                    break;
                case IEnumerable propertyNames:
                    selected = FilterProperties(value, PropertyNames(propertyNames), "", new JsonWriteContext());
                    break;
                default:
                    throw new TypeError("JSON.stringify replacer requires a closed callback, property-name sequence, null, or undefined.");
            }
            return FormatWithSpace(stringify(selected), space);
        }

        public static string stringify<TValue>(IDictionary<string, TValue>? value)
        {
            return StringifyRecord<TValue, NativeRecordValue<TValue>>(value);
        }

        public static string stringify<TValue>(IReadOnlyDictionary<string, TValue>? value)
        {
            return StringifyRecord<TValue, NativeRecordValue<TValue>>(value);
        }

        public static string stringify<TValue>(Dictionary<string, TValue>? value)
        {
            return StringifyRecord<TValue, NativeRecordValue<TValue>>(value);
        }

        public static string stringify(Dictionary<string, TsValue>? value)
        {
            return StringifyRecord<TsValue, ClosedRecordValue>(value);
        }

        public static string stringify(IDictionary<string, TsValue>? value)
        {
            return StringifyRecord<TsValue, ClosedRecordValue>(value);
        }

        public static string stringify(IReadOnlyDictionary<string, TsValue>? value)
        {
            return StringifyRecord<TsValue, ClosedRecordValue>(value);
        }

        private static string StringifyRecord<TValue, TRecordValue>(IEnumerable<KeyValuePair<string, TValue>>? value)
            where TRecordValue : struct, IRecordValue<TValue>
        {
            var stream = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(stream);
            WriteObject<TValue, TRecordValue>(writer, value, new JsonWriteContext());
            writer.Flush();
            return Encoding.UTF8.GetString(stream.WrittenSpan);
        }

        /// <summary>
        /// Write value to Utf8JsonWriter
        /// </summary>
        public static JSObject createObject(params object?[] keyValues)
        {
            if (keyValues.Length % 2 != 0)
            {
                throw new ArgumentException("Closed JSON object projection requires exact key/value pairs.", nameof(keyValues));
            }
            var result = new JSObject();
            for (var index = 0; index < keyValues.Length; index += 2)
            {
                if (keyValues[index] is not string key)
                {
                    throw new ArgumentException("Closed JSON object projection requires string keys.", nameof(keyValues));
                }
                result[key] = keyValues[index + 1];
            }
            return result;
        }

        public static void writeProperty(
            Utf8JsonWriter writer,
            string key,
            object? value,
            JsonWriteContext context)
        {
            value = NormalizeDirectJsonValue(value);
            writer.WritePropertyName(key);
            writeValue(writer, value, context, key);
        }

        /// <summary>
        /// Write JSObject as JSON object
        /// </summary>
        private static void WriteJsonValue(
            Utf8JsonWriter writer,
            IJsonValue value,
            JsonWriteContext context,
            string key)
        {
            Enter(value, context);
            try
            {
                value.__tsonicWriteJson(writer, context, key);
            }
            finally
            {
                context.exit(value);
            }
        }

        private static void WriteJsObject(Utf8JsonWriter writer, JSObject obj, JsonWriteContext context)
        {
            Enter(obj, context);
            try
            {
                writer.WriteStartObject();
                foreach (var (key, value) in obj.entries())
                {
                    writeProperty(writer, key, value, context);
                }
                writer.WriteEndObject();
            }
            finally
            {
                context.exit(obj);
            }
        }

        private interface IRecordValue<TValue>
        {
            static abstract object? Unwrap(TValue value);
        }

        private readonly struct NativeRecordValue<TValue> : IRecordValue<TValue>
        {
            public static object? Unwrap(TValue value) => value;
        }

        private readonly struct ClosedRecordValue : IRecordValue<TsValue>
        {
            public static object? Unwrap(TsValue value) => value.unwrap();
        }

        private static void WriteObject<TValue>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, TValue>>? dict, JsonWriteContext context)
        {
            WriteObject<TValue, NativeRecordValue<TValue>>(writer, dict, context);
        }

        private static void WriteObject<TValue, TRecordValue>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, TValue>>? dict, JsonWriteContext context)
            where TRecordValue : struct, IRecordValue<TValue>
        {
            if (dict == null)
            {
                writer.WriteNullValue();
                return;
            }

            Enter(dict, context);
            try
            {
                writer.WriteStartObject();
                List<string>? remainingKeys = null;
                object? pendingValue = null;
                using (var entries = dict.GetEnumerator())
                {
                    while (entries.MoveNext())
                    {
                        var entry = entries.Current;
                        var value = TRecordValue.Unwrap(entry.Value);
                        if (TrackableJsonIdentity(value) != null)
                        {
                            remainingKeys = new List<string> { entry.Key };
                            while (entries.MoveNext()) remainingKeys.Add(entries.Current.Key);
                            pendingValue = value;
                            break;
                        }
                        writeProperty(writer, entry.Key, value, context);
                    }
                }
                if (remainingKeys != null)
                {
                    writeProperty(writer, remainingKeys[0], pendingValue, context);
                    for (var index = 1; index < remainingKeys.Count; index++)
                    {
                        if (TryReadRecordValue<TValue, TRecordValue>(dict, remainingKeys[index], out var value))
                            writeProperty(writer, remainingKeys[index], value, context);
                    }
                }
                writer.WriteEndObject();
            }
            finally
            {
                context.exit(dict);
            }
        }

        private static bool TryReadRecordValue<TValue, TRecordValue>(IEnumerable<KeyValuePair<string, TValue>> record, string key, out object? value)
            where TRecordValue : struct, IRecordValue<TValue>
        {
            TValue selected;
            var present = record switch
            {
                IDictionary<string, TValue> dictionary => dictionary.TryGetValue(key, out selected!),
                IReadOnlyDictionary<string, TValue> dictionary => dictionary.TryGetValue(key, out selected!),
                _ => throw new NotSupportedException("A native JSON record requires its exact typed dictionary interface."),
            };
            value = present ? TRecordValue.Unwrap(selected) : null;
            return present;
        }

        private static void WriteJsArray(Utf8JsonWriter writer, IDynamicArray array, JsonWriteContext context)
        {
            Enter(array, context);
            try
            {
                writer.WriteStartArray();
                for (var index = 0; index < array.Length; index++)
                {
                    if (array.TryGetAt(index, out var item))
                    {
                        var normalized = NormalizeDirectJsonValue(item);
                        writeValue(writer, normalized, context, index.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        writer.WriteNullValue();
                    }
                }
                writer.WriteEndArray();
            }
            finally
            {
                context.exit(array);
            }
        }

        private static void Enter(object value, JsonWriteContext context)
        {
            if (!context.enter(value))
            {
                throw new InvalidOperationException("Converting circular structure to JSON.");
            }
        }

        private static TsValue ApplyReplacer(
            string key,
            object? value,
            JsonReplacer replacer,
            JsonWriteContext context)
        {
            var normalized = NormalizeJsonValue(value, key);
            var replaced = replacer(key, ToTsValue(normalized));
            var unwrapped = TsValue.UnwrapClosedValue(replaced);
            var identity = TrackableJsonIdentity(unwrapped);
            if (identity != null)
            {
                Enter(identity, context);
            }
            try
            {
                var keys = RecordKeys(unwrapped);
                if (keys != null)
                {
                    var result = new JSObject();
                    var record = TsValue.from(unwrapped);
                    foreach (var property in new List<string>(keys))
                    {
                        var child = ApplyReplacer(property, record.ReadDynamicSlot(property), replacer, context);
                        result[property] = child.unwrap();
                    }
                    return TsValue.from(result);
                }
                if (unwrapped is IDynamicArray sourceArray)
                {
                    var result = new JSArray<object?>();
                    var length = sourceArray.Length;
                    for (var index = 0; index < length; index++)
                    {
                        var item = sourceArray.TryGetAt(index, out var current) ? current : null;
                        var child = ApplyReplacer(index.ToString(CultureInfo.InvariantCulture), item, replacer, context);
                        result.push(child.unwrap());
                    }
                    return TsValue.from(result);
                }
                return replaced;
            }
            finally
            {
                if (identity != null)
                {
                    context.exit(identity);
                }
            }
        }

        private static object? FilterProperties(
            object? value,
            HashSet<string> names,
            string key,
            JsonWriteContext context)
        {
            var sourceIdentity = TrackableJsonIdentity(value);
            if (sourceIdentity != null)
            {
                Enter(sourceIdentity, context);
            }
            try
            {
                value = NormalizeJsonValue(value, key);
                var keys = RecordKeys(value);
                if (keys != null)
                {
                    var result = new JSObject();
                    var record = TsValue.from(value);
                    foreach (var property in keys)
                    {
                        if (names.Contains(property))
                        {
                            var selected = FilterProperties(record.ReadDynamicSlot(property), names, property, context);
                            result[property] = selected;
                        }
                    }
                    return result;
                }
                if (value is IDynamicArray sourceArray)
                {
                    var result = new JSArray<object?>();
                    for (var index = 0; index < sourceArray.Length; index++)
                    {
                        result.push(sourceArray.TryGetAt(index, out var item)
                            ? FilterProperties(item, names, index.ToString(CultureInfo.InvariantCulture), context)
                            : null);
                    }
                    return result;
                }
                return value;
            }
            finally
            {
                if (sourceIdentity != null)
                {
                    context.exit(sourceIdentity);
                }
            }
        }

        private static HashSet<string> PropertyNames(IEnumerable values)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in values)
            {
                switch (item)
                {
                    case string text:
                        names.Add(text);
                        break;
                    case byte number:
                        names.Add(Globals.String(number));
                        break;
                    case sbyte number:
                        names.Add(Globals.String(number));
                        break;
                    case short number:
                        names.Add(Globals.String(number));
                        break;
                    case ushort number:
                        names.Add(Globals.String(number));
                        break;
                    case int number:
                        names.Add(Globals.String(number));
                        break;
                    case uint number:
                        names.Add(Globals.String(number));
                        break;
                    case long number:
                        names.Add(Globals.String(number));
                        break;
                    case ulong number:
                        names.Add(Globals.String(number));
                        break;
                    case float number:
                        names.Add(Globals.String(number));
                        break;
                    case double number:
                        names.Add(Globals.String(number));
                        break;
                }
            }
            return names;
        }

        private static object? NormalizeJsonValue(object? value, string key)
        {
            value = TsValue.UnwrapClosedValue(value);
            return value switch
            {
                IJsonValue jsonValue => jsonValue.__tsonicJsonValue(key),
                Date date => date.toJSON(),
                _ => value,
            };
        }

        private static object? NormalizeDirectJsonValue(object? value)
        {
            value = TsValue.UnwrapClosedValue(value);
            return value is Date date ? date.toJSON() : value;
        }

        private static TsValue ToTsValue(object? value)
        {
            return TsValue.from(value);
        }

        private static object? TrackableJsonIdentity(object? value)
        {
            value = TsValue.UnwrapClosedValue(value);
            return value is IJsonValue or JSObject or IDynamicArray or
                IDictionary<string, TsValue> or IReadOnlyDictionary<string, TsValue> or
                IDictionary<string, object?> or IReadOnlyDictionary<string, object?> ? value : null;
        }

        private static IEnumerable<string>? RecordKeys(object? value)
        {
            return value switch
            {
                JSObject record => record.asReadOnlyDictionary().Keys,
                IDictionary<string, TsValue> record => record.Keys,
                IReadOnlyDictionary<string, TsValue> record => record.Keys,
                IDictionary<string, object?> record => record.Keys,
                IReadOnlyDictionary<string, object?> record => record.Keys,
                _ => null,
            };
        }

        private static string FormatWithSpace(string compact, TsValue space)
        {
            var indentation = space.unwrap() switch
            {
                string text => text[..System.Math.Min(10, text.Length)],
                double number when double.IsFinite(number) => new string(' ', System.Math.Clamp((int)System.Math.Truncate(number), 0, 10)),
                int number => new string(' ', System.Math.Clamp(number, 0, 10)),
                _ => string.Empty,
            };
            if (indentation.Length == 0)
            {
                return compact;
            }

            using var document = JsonDocument.Parse(compact);
            var stream = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                document.RootElement.WriteTo(writer);
            }
            var rendered = Encoding.UTF8.GetString(stream.WrittenSpan);
            if (indentation == "  ")
            {
                return rendered;
            }
            var lines = rendered.Split('\n');
            for (var index = 0; index < lines.Length; index++)
            {
                var line = lines[index];
                var spaces = 0;
                while (spaces < line.Length && line[spaces] == ' ')
                {
                    spaces++;
                }
                lines[index] = string.Concat(Enumerable.Repeat(indentation, spaces / 2)) + line[spaces..];
            }
            return string.Join("\n", lines);
        }
    }
}
