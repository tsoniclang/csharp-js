using System;
using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    using JsObjectStatics = Tsonic.CSharp.Js.Object;

    public class ObjectTests
    {
        [Fact]
        public void keys_values_entries_ReturnClosedJsArraysForJsObject()
        {
            var value = new JSObject
            {
                ["name"] = "tsonic",
                ["count"] = 3
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<List<string>>(keys);
            Assert.IsType<List<object?>>(values);
            Assert.IsType<List<(string key, object? value)>>(entries);
            Assert.Equal(new[] { "name", "count" }, keys);
            Assert.Equal(new object?[] { "tsonic", 3 }, values);
            Assert.Equal(("name", (object?)"tsonic"), entries[0]);
            Assert.Equal(("count", (object?)3), entries[1]);
        }

        [Fact]
        public void is_UsesJavaScriptSameValueSemantics()
        {
            var objectValue = new JSObject();

            Assert.True(JsObjectStatics.@is(double.NaN, double.NaN));
            Assert.True(JsObjectStatics.@is(1, 1d));
            Assert.True(JsObjectStatics.@is("x", "x"));
            Assert.True(JsObjectStatics.@is(null, null));
            Assert.True(JsObjectStatics.@is(TsValue.undefined(), JSUndefined.value));
            Assert.True(JsObjectStatics.@is(objectValue, objectValue));
            Assert.False(JsObjectStatics.@is(0d, -0d));
            Assert.False(JsObjectStatics.@is(null, 0));
            Assert.False(JsObjectStatics.@is(TsValue.undefined(), null));
            Assert.False(JsObjectStatics.@is(new JSObject(), new JSObject()));
        }

        [Fact]
        public void keys_values_entries_UseEnumerableOwnArrayIndexesOnly()
        {
            var value = new JSArray<int>();
            value.setLength(5);
            value[1] = 20;
            value[3] = 40;

            Assert.Equal(new[] { "1", "3" }, JsObjectStatics.keys(value));
            Assert.Equal(new object?[] { 20, 40 }, JsObjectStatics.values(value));

            var entries = JsObjectStatics.entries(value);

            Assert.Equal(2, entries.Count);
            Assert.Equal(("1", (object?)20), entries[0]);
            Assert.Equal(("3", (object?)40), entries[1]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForReadOnlyDictionary()
        {
            IReadOnlyDictionary<string, int> value = new Dictionary<string, int>
            {
                ["first"] = 10,
                ["second"] = 20
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<List<string>>(keys);
            Assert.IsType<List<int>>(values);
            Assert.IsType<List<(string key, int value)>>(entries);
            Assert.Equal(new[] { "first", "second" }, keys);
            Assert.Equal(new[] { 10, 20 }, values);
            Assert.Equal(("first", 10), entries[0]);
            Assert.Equal(("second", 20), entries[1]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForMutableDictionaryInterface()
        {
            IDictionary<string, string> value = new Dictionary<string, string>
            {
                ["name"] = "tsonic",
                ["target"] = "csharp"
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<List<string>>(keys);
            Assert.IsType<List<string>>(values);
            Assert.IsType<List<(string key, string value)>>(entries);
            Assert.Equal(new[] { "name", "target" }, keys);
            Assert.Equal(new[] { "tsonic", "csharp" }, values);
            Assert.Equal(("name", "tsonic"), entries[0]);
            Assert.Equal(("target", "csharp"), entries[1]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForConcreteDictionaryWithoutAmbiguousOverload()
        {
            var value = new Dictionary<string, bool>
            {
                ["ok"] = true,
                ["done"] = false
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<List<string>>(keys);
            Assert.IsType<List<bool>>(values);
            Assert.IsType<List<(string key, bool value)>>(entries);
            Assert.Equal(new[] { "ok", "done" }, keys);
            Assert.Equal(new[] { true, false }, values);
            Assert.Equal(("ok", true), entries[0]);
            Assert.Equal(("done", false), entries[1]);
        }

        [Fact]
        public void hasOwn_SupportsClosedTypedDictionaryWithoutReflection()
        {
            var value = new Dictionary<string, int>
            {
                ["answer"] = 42
            };

            Assert.True(JsObjectStatics.hasOwn(value, "answer"));
            Assert.False(JsObjectStatics.hasOwn(value, "missing"));
        }

        [Fact]
        public void keys_values_entries_SupportStringBoxingWithoutReflection()
        {
            Assert.Equal(new[] { "0", "1", "2" }, JsObjectStatics.keys("abc"));
            Assert.Equal(new object?[] { "a", "b", "c" }, JsObjectStatics.values("abc"));

            var entries = JsObjectStatics.entries("abc");

            Assert.Equal(("0", (object?)"a"), entries[0]);
            Assert.Equal(("1", (object?)"b"), entries[1]);
            Assert.Equal(("2", (object?)"c"), entries[2]);
        }

        [Fact]
        public void keys_values_entries_ReturnEmptyForScalarBoxing()
        {
            Assert.Empty(JsObjectStatics.keys(42));
            Assert.Empty(JsObjectStatics.values(true));
            Assert.Empty(JsObjectStatics.entries(3.14));
        }

        [Fact]
        public void keys_RejectsNullReceiver()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(null));
        }

        [Fact]
        public void keys_RejectsUndefinedReceiver()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(JSUndefined.value));
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(TsValue.undefined()));
        }

        [Fact]
        public void keys_RejectsUnsupportedClrObjectWithoutReflection()
        {
            Assert.Throws<NotSupportedException>(() => JsObjectStatics.keys(new { name = "not-a-js-carrier" }));
        }

        [Fact]
        public void keys_values_entries_UseClosedTsObjectCarrier()
        {
            var value = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["name"] = "tsonic",
                ["missing"] = JSUndefined.value,
                ["nil"] = null
            }));

            Assert.Equal(new[] { "name", "missing", "nil" }, JsObjectStatics.keys(value));
            Assert.Equal(new object?[] { "tsonic", JSUndefined.value, null }, JsObjectStatics.values(value));

            var entries = JsObjectStatics.entries(value);

            Assert.Equal(("name", (object?)"tsonic"), entries[0]);
            Assert.Equal(("missing", (object?)JSUndefined.value), entries[1]);
            Assert.Equal(("nil", (object?)null), entries[2]);
        }

        [Fact]
        public void keys_values_entries_UseClosedTsArrayPresentSlotsOnly()
        {
            var array = new TsArray();
            array.WriteCompatElement(2, "third");
            array.WriteCompatElement(1, JSUndefined.value);

            Assert.Equal(new[] { "1", "2" }, JsObjectStatics.keys(TsValue.from(array)));
            Assert.Equal(new object?[] { JSUndefined.value, "third" }, JsObjectStatics.values(TsValue.from(array)));

            var entries = JsObjectStatics.entries(TsValue.from(array));

            Assert.Equal(("1", (object?)JSUndefined.value), entries[0]);
            Assert.Equal(("2", (object?)"third"), entries[1]);
        }

        [Fact]
        public void assign_MutatesTargetFromClosedSourcesAndReturnsTarget()
        {
            var target = new JSObject
            {
                ["name"] = "old"
            };
            var source = new JSObject
            {
                ["name"] = "new",
                ["active"] = true
            };
            var sparse = new JSArray<int>();
            sparse.setLength(4);
            sparse[2] = 9;

            var result = JsObjectStatics.assign(target, null, JSUndefined.value, TsValue.undefined(), source, sparse, "xy");

            Assert.Same(target, result);
            Assert.Equal("new", target["name"]);
            Assert.Equal(true, target["active"]);
            Assert.Equal("x", target["0"]);
            Assert.Equal("y", target["1"]);
            Assert.Equal(9, target["2"]);
            Assert.False(target.hasOwnProperty("3"));
        }

        [Fact]
        public void assign_CopiesIntoDictionaryTarget()
        {
            var target = new Dictionary<string, object?>();
            var source = new JSObject
            {
                ["answer"] = 42
            };

            var result = JsObjectStatics.assign(target, source);

            Assert.Same(target, result);
            Assert.Equal(42, target["answer"]);
        }

        [Fact]
        public void assign_CopiesFromClosedTsObjectAndSkipsUndefinedSources()
        {
            var target = new JSObject();
            var source = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["name"] = "closed",
                ["nil"] = null,
                ["missing"] = JSUndefined.value
            }));

            var result = JsObjectStatics.assign(target, TsValue.undefined(), source);

            Assert.Same(target, result);
            Assert.Equal("closed", target["name"]);
            Assert.Null(target["nil"]);
            Assert.Same(JSUndefined.value, target["missing"]);
        }

        [Fact]
        public void assign_CopiesFromClosedTsArrayPresentSlotsOnly()
        {
            var target = new JSObject();
            var source = new TsArray();
            source.WriteCompatElement(2, "third");
            source.WriteCompatElement(1, JSUndefined.value);

            var result = JsObjectStatics.assign(target, source);

            Assert.Same(target, result);
            Assert.False(target.hasOwnProperty("0"));
            Assert.Same(JSUndefined.value, target["1"]);
            Assert.Equal("third", target["2"]);
        }

        [Fact]
        public void assign_CopiesTypedDictionarySourcesWithoutReflection()
        {
            var target = new Dictionary<string, int>
            {
                ["answer"] = 1
            };
            var first = new Dictionary<string, int>
            {
                ["answer"] = 42
            };
            var second = new Dictionary<string, int>
            {
                ["extra"] = 7
            };

            var result = JsObjectStatics.assign(target, first, second);

            Assert.Same(target, result);
            Assert.Equal(42, target["answer"]);
            Assert.Equal(7, target["extra"]);
        }

        [Fact]
        public void assign_RejectsNullTarget()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.assign((JSObject)null!));
        }

        [Fact]
        public void assign_RejectsUnsupportedSourceWithoutReflection()
        {
            Assert.Throws<NotSupportedException>(() =>
                JsObjectStatics.assign(new JSObject(), new { name = "not-a-js-carrier" })
            );
        }
    }
}
