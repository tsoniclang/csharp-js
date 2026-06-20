using System;
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
            var result = Assert.IsType<JSObject>(JSON.parse(json));

            Assert.Equal("Alice", result["Name"]);
            Assert.Equal(25d, result["Age"]);
        }

        [Fact]
        public void parse_DeserializesComplexObjectCarrier()
        {
            var json = "{\"Id\":42,\"Value\":\"data\",\"IsActive\":false}";
            var result = Assert.IsType<JSObject>(JSON.parse(json));

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
            var result = Assert.IsType<JSObject>(JSON.parse(json));

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
            var restored = Assert.IsType<JSObject>(JSON.parse(json));

            Assert.Equal(99d, restored["Id"]);
            Assert.Equal("test data", restored["Value"]);
            Assert.True((bool)restored["IsActive"]!);
        }

        [Fact]
        public void Object_Keys_Values_Entries_WorkWithParsedJson()
        {
            object value = JSON.parse("{\"name\":\"tsumo\",\"count\":2}")!;

            Assert.Equal(new[] { "name", "count" }, Object.keys(value));
            Assert.Equal(new object?[] { "tsumo", 2d }, Object.values(value));

            var entries = Object.entries(value);
            Assert.Equal(("name", (object?)"tsumo"), entries[0]);
            Assert.Equal(("count", (object?)2d), entries[1]);
        }

        [Fact]
        public void Parse_Object_PreservesJsonArraysAsArrays()
        {
            object value = JSON.parse("{\"mounts\":[{\"name\":\"docs\"}]}")!;
            var entries = Object.entries(value);
            var mounts = Assert.IsType<object?[]>(entries[0].value);

            Assert.True(JSArrayStatics.isArray(mounts));
            var firstMount = Assert.IsType<JSObject>(mounts[0]);
            Assert.Equal("docs", firstMount["name"]);
        }
    }
}
