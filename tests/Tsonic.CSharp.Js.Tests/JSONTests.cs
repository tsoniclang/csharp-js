using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class JSONTests
    {
        [Fact]
        public void stringify_SerializesJsObject()
        {
            var obj = new JSObject
            {
                ["name"] = "John",
                ["age"] = 30
            };
            var json = JSON.stringify(obj);

            Assert.Contains("John", json);
            Assert.Contains("30", json);
        }

        [Fact]
        public void stringify_RejectsUnknownClrObject()
        {
            Assert.Throws<NotSupportedException>(() => JSON.stringify(new { name = "John" }));
        }

        [Fact]
        public void parse_DeserializesObjectCarrier()
        {
            var json = "{\"Name\":\"Alice\",\"Age\":25}";
            var result = Assert.IsType<JSObject>(JSON.parse(json).unwrap());

            Assert.Equal("Alice", result["Name"]);
            Assert.Equal(25d, result["Age"]);
        }

        [Fact]
        public void parse_DeserializesComplexObjectCarrier()
        {
            var json = "{\"Id\":42,\"Value\":\"data\",\"IsActive\":false}";
            var result = Assert.IsType<JSObject>(JSON.parse(json).unwrap());

            Assert.Equal(42d, result["Id"]);
            Assert.Equal("data", result["Value"]);
            Assert.False((bool)result["IsActive"]!);
        }

        [Fact]
        public void stringify_HandlesNull()
        {
            var obj = new JSObject
            {
                ["Name"] = null,
                ["Age"] = 0
            };
            var json = JSON.stringify(obj);

            Assert.Contains("null", json);
        }

        [Fact]
        public void stringify_HandlesNumbers()
        {
            var obj = new JSObject
            {
                ["integer"] = 42,
                ["floating"] = 3.14,
                ["negative"] = -10
            };
            var json = JSON.stringify(obj);

            Assert.Contains("42", json);
            Assert.Contains("3.14", json);
            Assert.Contains("-10", json);
        }

        [Fact]
        public void stringify_HandlesBooleans()
        {
            var obj = new JSObject
            {
                ["trueValue"] = true,
                ["falseValue"] = false
            };
            var json = JSON.stringify(obj);

            Assert.Contains("true", json);
            Assert.Contains("false", json);
        }

        [Fact]
        public void parse_HandlesEmptyObject()
        {
            var json = "{}";
            var result = Assert.IsType<JSObject>(JSON.parse(json).unwrap());

            Assert.Empty(result.keys());
        }

        [Fact]
        public void RoundTrip_PreservesClosedJsObjectData()
        {
            var original = new JSObject
            {
                ["Id"] = 99,
                ["Value"] = "test data",
                ["IsActive"] = true
            };

            var json = JSON.stringify(original);
            var restored = Assert.IsType<JSObject>(JSON.parse(json).unwrap());

            Assert.Equal(99d, restored["Id"]);
            Assert.Equal("test data", restored["Value"]);
            Assert.True((bool)restored["IsActive"]!);
        }

        [Fact]
        public void Object_Keys_Values_Entries_WorkWithParsedJson()
        {
            object value = JSON.parse("{\"name\":\"tsumo\",\"count\":2}").unwrap()!;

            Assert.Equal(new[] { "name", "count" }, Object.keys(value));
            Assert.Equal(new object?[] { "tsumo", 2d }, Object.values(value));

            var entries = Object.entries(value);
            Assert.Equal(("name", (object?)"tsumo"), entries[0]);
            Assert.Equal(("count", (object?)2d), entries[1]);
        }

        [Fact]
        public void Parse_Object_PreservesJsonArraysAsArrays()
        {
            object value = JSON.parse("{\"mounts\":[{\"name\":\"docs\"}]}").unwrap()!;
            var entries = Object.entries(value);
            var mounts = Assert.IsType<JSArray<object?>>(entries[0].value);

            Assert.True(JSArrayStatics.isArray(mounts));
            var firstMount = Assert.IsType<JSObject>(mounts[0]);
            Assert.Equal("docs", firstMount["name"]);
        }

        [Fact]
        public void stringify_SerializesJsArrayCarrier()
        {
            var value = new JSArray<int>(new[] { 1, 2, 3 });

            Assert.Equal("[1,2,3]", JSON.stringify(value));
        }

        [Fact]
        public void stringify_SerializesJsArrayHolesAsNull()
        {
            var value = new JSArray<object?>();
            value.setLength(4);
            value[1] = 2;
            value[3] = null;

            Assert.Equal("[null,2,null,null]", JSON.stringify(value));
        }

        [Fact]
        public void stringify_RejectsNonCarrierEnumerables()
        {
            Assert.Throws<NotSupportedException>(() => JSON.stringify(new List<object?> { 1, 2, 3 }));
        }

        [Fact]
        public void TsValue_RejectsOpenClrObjects()
        {
            Assert.Throws<NotSupportedException>(() => TsValue.from(new { name = "not-a-carrier" }));
        }

        [Fact]
        public void stringify_AcceptsClosedTsValueCarrier()
        {
            var value = TsValue.from(new JSObject
            {
                ["name"] = "carrier"
            });

            Assert.Equal("{\"name\":\"carrier\"}", JSON.stringify(value));
        }

        [Fact]
        public void stringify_SerializesClosedRuntimeUnionActiveArm()
        {
            var text = TsValue.from(Union<int, string>.From2("ready"));
            var number = TsValue.from(Union<int, string>.From1(42));

            Assert.Equal("\"ready\"", JSON.stringify(text));
            Assert.Equal("42", JSON.stringify(number));
        }
    }
}
