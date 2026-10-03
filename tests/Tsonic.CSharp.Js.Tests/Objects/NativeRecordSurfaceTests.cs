using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class NativeRecordSurfaceTests
    {
        [Fact]
        public void Json_UsesExactRecordChildrenAndLiveBacking()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.from(ulong.MaxValue),
                ["absent"] = TsValue.undefined(),
                ["enabled"] = TsValue.from(false),
            };
            var value = TsValue.from(record);
            Assert.Equal("{\"wide\":18446744073709551615,\"absent\":null,\"enabled\":false}", JSON.stringify(value));
            record["wide"] = TsValue.from(1UL);
            Assert.Equal("{\"wide\":1,\"absent\":null,\"enabled\":false}", JSON.stringify(value));
            Assert.Same(record, value.unwrap());
            var view = new ReadOnlyDictionary<string, TsValue>(record);
            Assert.Equal(JSON.stringify(value), JSON.stringify(TsValue.from(view)));
            record["cycle"] = value;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value)).Message);
        }

        [Fact]
        public void ObjectEnumeration_RetainsKeysAndNativeValuesWithoutRecordCopy()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.from(ulong.MaxValue),
                ["absent"] = TsValue.undefined(),
            };
            var value = TsValue.from(record);
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
            record["new"] = TsValue.from("live");
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(value));
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(TsValue.from(new ReadOnlyDictionary<string, TsValue>(record))));
            var selected = TsValue.from(Union<int, Dictionary<string, TsValue>>.From2(record));
            Assert.Equal(new[] { "wide", "absent", "new" }, Object.keys(selected));
        }

        [Fact]
        public void JsonReplacer_SnapshotsOnlyKeysAndReadsLaterValuesLive()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.from(1),
                ["second"] = TsValue.from(2),
            };
            var calls = new List<string>();
            JsonReplacer replacer = (key, value) =>
            {
                calls.Add(key);
                if (key == "") record["root"] = TsValue.from("included");
                if (key == "first")
                {
                    record["second"] = TsValue.from(ulong.MaxValue);
                    record["late"] = TsValue.from("excluded");
                    return TsValue.from(10);
                }
                return value;
            };
            Assert.Equal("{\"first\":10,\"second\":18446744073709551615,\"root\":\"included\"}", JSON.stringify(TsValue.from(record), replacer));
            Assert.Equal(new[] { "", "first", "second", "root" }, calls);
            Assert.Equal(ulong.MaxValue, record["second"].unwrap());
            Assert.Equal("excluded", record["late"].unwrap());
        }

        [Fact]
        public void JsonReplacer_ReadsRemovedMembersAsTheOneAbsence()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.from(1),
                ["removed"] = TsValue.from(2),
            };
            JsonReplacer replacer = (key, value) =>
            {
                if (key == "first") record.Remove("removed");
                return value;
            };
            Assert.Equal("{\"first\":1,\"removed\":null}", JSON.stringify(TsValue.from(record), replacer));
        }

        [Fact]
        public void JsonReplacer_CanRestoreADeletedSnapshottedMemberFromNativeAbsence()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["first"] = TsValue.from(1),
                ["removed"] = TsValue.from(2),
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
                    return TsValue.from(ulong.MaxValue);
                }
                return value;
            };
            Assert.Equal("{\"first\":1,\"removed\":18446744073709551615,\"present\":null}", JSON.stringify(TsValue.from(record), replacer));
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
            JsonReplacer replacer = (key, value) => key == "keep" ? TsValue.from(10) : value;
            Assert.Equal("{\"keep\":10,\"drop\":2,\"nested\":{\"keep\":10,\"drop\":4}}", JSON.stringify(record, replacer));
            Assert.Equal("{\"keep\":1,\"nested\":{\"keep\":3}}", JSON.stringify(record, new[] { "keep", "nested" }));
            var closed = new Dictionary<string, TsValue>
            {
                ["keep"] = TsValue.from(ulong.MaxValue),
                ["drop"] = TsValue.from(false),
                ["nested"] = TsValue.from(record),
            };
            Assert.Equal("{\"keep\":18446744073709551615,\"nested\":{\"keep\":1,\"nested\":{\"keep\":3}}}", JSON.stringify(TsValue.from(closed), new[] { "keep", "nested" }));
            Assert.Equal("{\"keep\":18446744073709551615,\"nested\":{\"keep\":1,\"nested\":{\"keep\":3}}}", JSON.stringify(TsValue.from(new ReadOnlyDictionary<string, TsValue>(closed)), new[] { "keep", "nested" }));
        }

        [Fact]
        public void JsonReplacer_TracksTheSelectedIdentityAndCanRemoveCycles()
        {
            var record = new Dictionary<string, TsValue>();
            var value = TsValue.from(record);
            record["self"] = value;
            JsonReplacer identity = (_, child) => child;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value, identity)).Message);
            JsonReplacer removeCycle = (key, child) => key == "self" ? TsValue.undefined() : child;
            Assert.Equal("{\"self\":null}", JSON.stringify(value, removeCycle));
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(value, new[] { "self" })).Message);
            var replacement = new Dictionary<string, TsValue>();
            var replacementValue = TsValue.from(replacement);
            replacement["self"] = replacementValue;
            JsonReplacer replaceRoot = (key, child) => key == "" ? replacementValue : child;
            Assert.Equal("Converting circular structure to JSON.", Assert.Throws<InvalidOperationException>(() => JSON.stringify(TsValue.from("root"), replaceRoot)).Message);
        }

        [Fact]
        public void JsonDefaultBorrowedPath_HasTheSameAllocationAsDirectNativeRecordWriting()
        {
            var record = new Dictionary<string, TsValue>
            {
                ["wide"] = TsValue.from(ulong.MaxValue),
                ["text"] = TsValue.from("native"),
                ["absent"] = TsValue.undefined(),
            };
            var value = TsValue.from(record);
            for (var index = 0; index < 1000; index++)
            {
                JSON.stringify(record);
                JSON.stringify(value);
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            var directLength = 0;
            for (var index = 0; index < 1000; index++) directLength += JSON.stringify(record).Length;
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
            var record = new Dictionary<string, TsValue> { ["prefix"] = TsValue.from(1) };
            var invocations = 0;
            var projection = new MutationProjection(() =>
            {
                invocations++;
                record["prefix"] = TsValue.from(99);
                record["later"] = TsValue.from(ulong.MaxValue);
                record.Remove("removed");
                record["late"] = TsValue.from("not in original keys");
            });
            record["nested"] = TsValue.from(new JSArray<object?>(new object?[] { projection }));
            record["later"] = TsValue.from(2);
            record["removed"] = TsValue.from(3);
            record["present"] = TsValue.undefined();
            Assert.Equal("{\"prefix\":1,\"nested\":[\"projected\"],\"later\":18446744073709551615,\"present\":null}", JSON.stringify(TsValue.from(record)));
            Assert.Equal(1, invocations);
            Assert.Equal(99, record["prefix"].unwrap());
            Assert.Equal("not in original keys", record["late"].unwrap());
        }

        [Fact]
        public void IntlOptions_ReadTheExactNativeRecordAndOneAbsence()
        {
            var options = new Dictionary<string, TsValue>
            {
                ["useGrouping"] = TsValue.from(false),
                ["maximumFractionDigits"] = TsValue.from(0),
                ["currency"] = TsValue.undefined(),
            };
            var value = TsValue.from(options);
            var formatter = new IntlNumberFormat(TsValue.from("en"), value);
            Assert.Equal("18446744073709551615", formatter.formatInteger(ulong.MaxValue));
            Assert.Null(formatter.resolvedOptions().currency);
            Assert.Same(options, value.unwrap());
            options["useGrouping"] = TsValue.from(true);
            var grouped = new IntlNumberFormat(TsValue.from("en"), value);
            Assert.Equal("18,446,744,073,709,551,615", grouped.formatInteger(ulong.MaxValue));
            options["useGrouping"] = TsValue.from("invalid");
            Assert.Throws<RangeError>(() => new IntlNumberFormat(TsValue.from("en"), value));
        }

        [Fact]
        public void UnknownOpenDictionaries_AreNotRecoveredByRuntimeIntrospection()
        {
            Assert.Throws<NotSupportedException>(() => TsValue.from(new Dictionary<string, int>()));
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
