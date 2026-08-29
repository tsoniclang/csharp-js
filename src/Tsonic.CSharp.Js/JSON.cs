/**
 * JavaScript JSON object implementation
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public static string? stringify(object? value)
        {
            value = NormalizeDirectJsonValue(value);
            if (IsUndefined(value))
            {
                return null;
            }
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            writeValue(writer, value, new JsonWriteContext(), "");
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string? stringify(TsValue value)
        {
            return stringify(value.unwrap());
        }

        public static string? stringify(object? value, object? replacer, TsValue space = default)
        {
            replacer = replacer is TsValue wrapped ? wrapped.unwrap() : replacer;
            object? selected;
            switch (replacer)
            {
                case null:
                case Undefined:
                    selected = NormalizeJsonValue(value, "");
                    break;
                case JsonReplacer callback:
                    selected = ApplyReplacer("", value, callback, null, new JsonWriteContext()).unwrap();
                    break;
                case IEnumerable propertyNames:
                    selected = FilterProperties(value, PropertyNames(propertyNames), "", new JsonWriteContext());
                    break;
                default:
                    throw new TypeError("JSON.stringify replacer requires a closed callback, property-name sequence, null, or undefined.");
            }
            if (IsUndefined(selected))
            {
                return null;
            }
            return FormatWithSpace(stringify(selected)!, space);
        }

        public static string stringify<TValue>(IDictionary<string, TValue>? value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            WriteObject(writer, value, new JsonWriteContext());
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string stringify<TValue>(IReadOnlyDictionary<string, TValue>? value)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            WriteObject(writer, value, new JsonWriteContext());
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
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
                    WriteJsObject(writer, obj, context);
                    break;
                case IDynamicArray array:
                    WriteJsArray(writer, array, context);
                    break;
                case IJsonValue jsonValue:
                    WriteJsonValue(writer, jsonValue, context, key);
                    break;
                case TsValue wrapped:
                    writeValue(writer, wrapped.unwrap(), context, key);
                    break;
                case TsUnion union:
                    writeValue(writer, union.unwrap(), context, key);
                    break;
                case IDictionary<string, object?> dict:
                    WriteObject(writer, dict, context);
                    break;
                case IReadOnlyDictionary<string, object?> dict:
                    WriteObject(writer, dict, context);
                    break;
                default:
                    throw new NotSupportedException("JSON.stringify requires a closed JS value carrier.");
            }
        }

        public static void writeProperty(
            Utf8JsonWriter writer,
            string key,
            object? value,
            JsonWriteContext context)
        {
            value = NormalizeDirectJsonValue(value);
            if (IsUndefined(value))
            {
                return;
            }
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

        /// <summary>
        /// Write dictionary as JSON object
        /// </summary>
        private static void WriteObject(Utf8JsonWriter writer, IDictionary<string, object?> dict, JsonWriteContext context)
        {
            Enter(dict, context);
            try
            {
                writer.WriteStartObject();
                foreach (var kvp in dict)
                {
                    writeProperty(writer, kvp.Key, kvp.Value, context);
                }
                writer.WriteEndObject();
            }
            finally
            {
                context.exit(dict);
            }
        }

        private static void WriteObject(Utf8JsonWriter writer, IReadOnlyDictionary<string, object?> dict, JsonWriteContext context)
        {
            Enter(dict, context);
            try
            {
                writer.WriteStartObject();
                foreach (var kvp in dict)
                {
                    writeProperty(writer, kvp.Key, kvp.Value, context);
                }
                writer.WriteEndObject();
            }
            finally
            {
                context.exit(dict);
            }
        }

        private static void WriteObject<TValue>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, TValue>>? dict, JsonWriteContext context)
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
                foreach (var kvp in dict)
                {
                    writeProperty(writer, kvp.Key, kvp.Value, context);
                }
                writer.WriteEndObject();
            }
            finally
            {
                context.exit(dict);
            }
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
                        if (IsUndefined(normalized))
                        {
                            writer.WriteNullValue();
                        }
                        else
                        {
                            writeValue(writer, normalized, context, index.ToString(CultureInfo.InvariantCulture));
                        }
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
            object? holder,
            JsonWriteContext context)
        {
            _ = holder;
            var sourceIdentity = TrackableJsonIdentity(value);
            if (sourceIdentity != null)
            {
                Enter(sourceIdentity, context);
            }
            try
            {
                var normalized = NormalizeJsonValue(value, key);
                var replaced = replacer(key, ToTsValue(normalized));
                if (replaced.isUndefined())
                {
                    return replaced;
                }
                var unwrapped = replaced.unwrap();
                var replacementIdentity = TrackableJsonIdentity(unwrapped);
                var trackReplacement = replacementIdentity != null &&
                    !ReferenceEquals(replacementIdentity, sourceIdentity);
                if (trackReplacement)
                {
                    Enter(replacementIdentity!, context);
                }
                try
                {
                    if (unwrapped is JSObject sourceObject)
                    {
                        var result = new JSObject();
                        foreach (var (property, propertyValue) in sourceObject.entries())
                        {
                            var child = ApplyReplacer(property, propertyValue, replacer, sourceObject, context);
                            if (!child.isUndefined())
                            {
                                result[property] = child.unwrap();
                            }
                        }
                        return TsValue.from(result);
                    }
                    if (unwrapped is IDynamicArray sourceArray)
                    {
                        var result = new JSArray<object?>();
                        for (var index = 0; index < sourceArray.Length; index++)
                        {
                            var item = sourceArray.TryGetAt(index, out var current) ? current : null;
                            var child = ApplyReplacer(index.ToString(CultureInfo.InvariantCulture), item, replacer, sourceArray, context);
                            result.push(child.isUndefined() ? null : child.unwrap());
                        }
                        return TsValue.from(result);
                    }
                    return replaced;
                }
                finally
                {
                    if (trackReplacement)
                    {
                        context.exit(replacementIdentity!);
                    }
                }
            }
            finally
            {
                if (sourceIdentity != null)
                {
                    context.exit(sourceIdentity);
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
                if (value is JSObject sourceObject)
                {
                    var result = new JSObject();
                    foreach (var (property, propertyValue) in sourceObject.entries())
                    {
                        if (names.Contains(property))
                        {
                            var selected = FilterProperties(propertyValue, names, property, context);
                            if (!IsUndefined(selected))
                            {
                                result[property] = selected;
                            }
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
            value = value switch
            {
                TsValue wrapped => wrapped.unwrap(),
                TsUnion union => union.unwrap(),
                _ => value,
            };
            return value switch
            {
                IJsonValue jsonValue => jsonValue.__tsonicJsonValue(key),
                Date date => date.toJSON(),
                _ => value,
            };
        }

        private static object? NormalizeDirectJsonValue(object? value)
        {
            value = value switch
            {
                TsValue wrapped => wrapped.unwrap(),
                TsUnion union => union.unwrap(),
                _ => value,
            };
            return value is Date date ? date.toJSON() : value;
        }

        private static TsValue ToTsValue(object? value)
        {
            return IsUndefined(value) ? TsValue.undefined() : TsValue.from(value);
        }

        private static bool IsUndefined(object? value)
        {
            return value is Undefined || value is TsValue wrapped && wrapped.isUndefined();
        }

        private static object? TrackableJsonIdentity(object? value)
        {
            value = value switch
            {
                TsValue wrapped => wrapped.unwrap(),
                TsUnion union => union.unwrap(),
                _ => value,
            };
            return value is IJsonValue or JSObject or IDynamicArray ? value : null;
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
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                document.RootElement.WriteTo(writer);
            }
            var rendered = Encoding.UTF8.GetString(stream.ToArray());
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
