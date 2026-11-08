using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    [Collection("Native allocation cost")]
    public class NativeRecordSurfaceTests
    {
        [Fact]
        public void Json_UsesExactRecordChildrenAndLiveBacking()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.fromnative(ulong.MaxValue),
                ["absent"] = TsValue.undefined(),
                ["enabled"] = TsValue.fromnative(false),
            };
            var value = TsValue.fromnative(record);
            Assert.Equal("{\"wide\":18446744073709551615,\"absent\":null,\"enabled\":false}", JSON.stringify(value));
            record["wide"] = TsValue.fromnative(1UL);
            Assert.Equal("{\"wide\":1,\"absent\":null,\"enabled\":false}", JSON.stringify(value));
            Assert.Same(record, value.unwrap());
            var view = new ReadOnlyDictionary<string, TsValue>(record);
            Assert.Equal(JSON.stringify(value), JSON.stringify(TsValue.fromnative(view)));
            record["cycle"] = value;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value)).Message);
        }

        [Fact]
        public void ObjectEnumeration_RetainsKeysAndNativeValuesWithoutRecordCopy()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.fromnative(ulong.MaxValue),
                ["absent"] = TsValue.undefined(),
            };
            var value = TsValue.fromnative(record);
            Assert.Equal(new[] { "wide", "absent" }, Object.keys(value));
            var values = Object.values(value);
            Assert.Equal(ulong.MaxValue, Assert.IsType<ulong>(values[0]));
            Assert.Null(values[1]);
            var entries = Object.entries(value);
            Assert.Equal(("wide", (object?)ulong.MaxValue), entries[0]);
            Assert.Equal(("absent", (object?)null), entries[1]);
            var target = new JSObject();
            Assert.Same(target, Object.assign(target, value));
            Assert.Equal(ulong.MaxValue, Assert.IsType<ulong>(target["wide"]));
            Assert.Null(target["absent"]);
            Assert.Same(record, value.unwrap());
            record["new"] = TsValue.fromnative("live");
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(value));
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(TsValue.fromnative(new ReadOnlyDictionary<string, TsValue>(record))));
            var selected = TsValue.fromnative(Union<int, Dictionary<string, TsValue>>.From2(record));
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(selected));
        }

        [Fact]
        public void JsonReplacer_SnapshotsOnlyKeysAndReadsLaterValuesLive()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.fromnative(1),
                ["second"] = TsValue.fromnative(2),
            };
            var calls = new List<string>();
            JsonReplacer replacer = (key, value) =>
            {
                calls.Add(key);
                if (key == "") record["root"] = TsValue.fromnative("included");
                if (key == "first")
                {
                    record["second"] = TsValue.fromnative(ulong.MaxValue);
                    record["late"] = TsValue.fromnative("excluded");
                    return TsValue.fromnative(10);
                }
                return value;
            };
            Assert.Equal("{\"first\":10,\"second\":18446744073709551615,\"root\":\"included\"}", JSON.stringify(TsValue.fromnative(record), replacer));
            Assert.Equal(new[] { "", "first", "second", "root" }, calls);
            Assert.Equal(ulong.MaxValue, record["second"].unwrap());
            Assert.Equal("excluded", record["late"].unwrap());
        }

        [Fact]
        public void JsonReplacer_ReadsRemovedMembersAsTheOneAbsence()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.fromnative(1),
                ["removed"] = TsValue.fromnative(2),
            };
            JsonReplacer replacer = (key, value) =>
            {
                if (key == "first") record.Remove("removed");
                return value;
            };
            Assert.Equal("{\"first\":1,\"removed\":null}", JSON.stringify(TsValue.fromnative(record), replacer));
        }

        [Fact]
        public void JsonReplacer_CanRestoreADeletedSnapshottedMemberFromNativeAbsence()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.fromnative(1),
                ["removed"] = TsValue.fromnative(2),
                ["present"] = TsValue.undefined(),
            };
            var visited = new List<string>();
            JsonReplacer replacer = (key, value) =>
            {
                visited.Add(key);
                if (key == "first") record.Remove("removed");
                if (key == "removed")
                {
                    Assert.Null(value.unwrap());
                    return TsValue.fromnative(ulong.MaxValue);
                }
                return value;
            };
            Assert.Equal("{\"first\":1,\"removed\":18446744073709551615,\"present\":null}", JSON.stringify(TsValue.fromnative(record), replacer));
            Assert.Equal(new[] { "", "first", "removed", "present" }, visited);
            Assert.False(record.ContainsKey("removed"));
        }

        [Fact]
        public void JsonCallbacksAndPropertyFilters_AlsoTraverseExistingObjectDictionaries()
        {
            var record = new Dictionary<string, object?>
            {
                ["keep"] = 1,
                ["drop"] = 2,
                ["nested"] = new Dictionary<string, object?> { ["keep"] = 3, ["drop"] = 4 },
            };
            JsonReplacer replacer = (key, value) => key == "keep" ? TsValue.fromnative(10) : value;
            Assert.Equal("{\"keep\":10,\"drop\":2,\"nested\":{\"keep\":10,\"drop\":4}}", JSON.stringify(record, replacer));
            Assert.Equal("{\"keep\":1,\"nested\":{\"keep\":3}}", JSON.stringify(record, new[] { "keep", "nested" }));
            var closed = new Dictionary<string, TsValue>
            {
                ["keep"] = TsValue.fromnative(ulong.MaxValue),
                ["drop"] = TsValue.fromnative(false),
                ["nested"] = TsValue.fromnative(record),
            };
            Assert.Equal("{\"keep\":18446744073709551615,\"nested\":{\"keep\":1,\"nested\":{\"keep\":3}}}", JSON.stringify(TsValue.fromnative(closed), new[] { "keep", "nested" }));
            Assert.Equal("{\"keep\":18446744073709551615,\"nested\":{\"keep\":1,\"nested\":{\"keep\":3}}}", JSON.stringify(TsValue.fromnative(new ReadOnlyDictionary<string, TsValue>(closed)), new[] { "keep", "nested" }));
        }

        [Fact]
        public void JsonReplacer_TracksTheSelectedIdentityAndCanRemoveCycles()
        {
            var record = new Dictionary<string, TsValue>();
            var value = TsValue.fromnative(record);
            record["self"] = value;
            JsonReplacer identity = (_, child) => child;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value, identity)).Message);
            JsonReplacer removeCycle = (key, child) => key == "self" ? TsValue.undefined() : child;
            Assert.Equal("{\"self\":null}", JSON.stringify(value, removeCycle));
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value, new[] { "self" })).Message);
            var replacement = new Dictionary<string, TsValue>();
            var replacementValue = TsValue.fromnative(replacement);
            replacement["self"] = replacementValue;
            JsonReplacer replaceRoot = (key, child) => key == "" ? replacementValue : child;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.fromnative("root"), replaceRoot)).Message);
        }

        [Fact]
        public void JsonDefaultBorrowedPath_HasTheSameAllocationAsDirectNativeRecordWriting()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.fromnative(ulong.MaxValue),
                ["text"] = TsValue.fromnative("native"),
                ["absent"] = TsValue.undefined(),
            };
            var value = TsValue.fromnative(record);
            for (var index = 0; index < 1000; index++)
            {
                JSON.stringify(record);
                JSON.stringify(value);
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            var directLength = 0;
            for (var index = 0; index < 1000; index++) directLength += JSON.stringify(record)!.Length;
            var directBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            before = GC.GetAllocatedBytesForCurrentThread();
            var admittedLength = 0;
            for (var index = 0; index < 1000; index++) admittedLength += JSON.stringify(value)!.Length;
            var admittedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(directLength, admittedLength);
            Assert.Equal(directBytes, admittedBytes);
            Assert.Same(record, value.unwrap());
        }

        [Fact]
        public void JsonDefaultWriter_SnapshotsOnlyRemainingKeysBeforeNestedNativeProjections()
        {
            var record = new Dictionary<string, TsValue> { ["prefix"] = TsValue.fromnative(1) };
            var invocations = 0;
            var projection = new MutationProjection(() =>
            {
                invocations++;
                record["prefix"] = TsValue.fromnative(99);
                record["later"] = TsValue.fromnative(ulong.MaxValue);
                record.Remove("removed");
                record["late"] = TsValue.fromnative("not in original keys");
            });
            record["nested"] = TsValue.fromnative(new JSArray<object?>(new object?[] { projection }));
            record["later"] = TsValue.fromnative(2);
            record["removed"] = TsValue.fromnative(3);
            record["present"] = TsValue.undefined();
            Assert.Equal("{\"prefix\":1,\"nested\":[\"projected\"],\"later\":18446744073709551615,\"present\":null}", JSON.stringify(TsValue.fromnative(record)));
            Assert.Equal(1, invocations);
            Assert.Equal(99, record["prefix"].unwrap());
            Assert.Equal("not in original keys", record["late"].unwrap());
        }

        [Fact]
        public void IntlOptions_ReadTheExactNativeRecordAndOneAbsence()
        {
            var options = new Dictionary<string, TsValue>
            {
                ["useGrouping"] = TsValue.fromnative(false),
                ["maximumFractionDigits"] = TsValue.fromnative(0),
                ["currency"] = TsValue.undefined(),
            };
            var value = TsValue.fromnative(options);
            var formatter = new IntlNumberFormat(TsValue.fromnative("en"), value);
            Assert.Equal("18446744073709551615", formatter.formatInteger(ulong.MaxValue));
            Assert.Null(formatter.resolvedOptions().currency);
            Assert.Same(options, value.unwrap());
            options["useGrouping"] = TsValue.fromnative(true);
            var grouped = new IntlNumberFormat(TsValue.fromnative("en"), value);
            Assert.Equal("18,446,744,073,709,551,615", grouped.formatInteger(ulong.MaxValue));
            options["useGrouping"] = TsValue.fromnative("invalid");
            Assert.Throws<RangeError>(() => new IntlNumberFormat(TsValue.fromnative("en"), value));
        }

        [Fact]
        public void UnknownOpenDictionaries_AreNotRecoveredByRuntimeIntrospection()
        {
            Assert.Throws<NotSupportedException>(() => TsValue.fromnative(new Dictionary<string, int>()));
            Assert.Throws<NotSupportedException>(() => Object.keys(new Dictionary<int, TsValue>()));
            Assert.Throws<NotSupportedException>(() => JSON.stringify(new Dictionary<int, TsValue>()));
        }

        private sealed class MutationProjection : IJsonValue, IDynamicObject
        {
            private readonly Action _effect;

            public MutationProjection(Action effect)
            {
                _effect = effect;
            }

            public object? __tsonicJsonValue(string key)
            {
                _effect();
                return "projected";
            }

            public void __tsonicWriteJson(Utf8JsonWriter writer, JsonWriteContext context, string key)
            {
                _effect();
                writer.WriteStringValue("projected");
            }

            public bool TryReadDynamicSlot(string key, out object? value)
            {
                value = null;
                return false;
            }

            public void WriteDynamicSlot(string key, object? value)
            {
                throw new NotSupportedException("Projection fixture is read-only.");
            }
        }
    }
}
