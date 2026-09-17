using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class ArrayStaticsTests
    {
        [Theory]
        [InlineData("", new string[0])]
        [InlineData("abc", new[] { "a", "b", "c" })]
        [InlineData("a😀é", new[] { "a", "😀", "é" })]
        public void from_StringOverloadsPreserveExactIteratorValues(string input, string[] expected)
        {
            Assert.Equal(expected, JSArrayStatics.from(input));
            Assert.Equal(expected, Tsonic.CSharp.Js.Array.from(input));
            var visits = 0;
            Assert.Equal(expected, JSArrayStatics.from(input, value => { visits++; return value; }));
            Assert.Equal(expected.Length, visits);
            visits = 0;
            Assert.Equal(expected, JSArrayStatics.from(input, (value, index) =>
            {
                Assert.Equal(visits, index);
                visits++;
                return value;
            }));
            Assert.Equal(expected.Length, visits);
        }

        [Fact]
        public void from_StringOverloadsPreserveLoneSurrogates()
        {
            var high = new string((char)0xD800, 1);
            var low = new string((char)0xDC00, 1);
            from_StringOverloadsPreserveExactIteratorValues(high + "x" + low, new[] { high, "x", low });
            from_StringOverloadsPreserveExactIteratorValues(low + "😀" + high, new[] { low, "😀", high });
        }

        [Fact]
        public void from_StringMappingStopsOnTheExactCallbackException()
        {
            var error = new System.InvalidOperationException("stop");
            var visited = "";
            var thrown = Assert.Throws<System.InvalidOperationException>(() => JSArrayStatics.from("a😀z", (value, index) =>
            {
                visited += value;
                if (index == 1) throw error;
                return value;
            }));
            Assert.Same(error, thrown);
            Assert.Equal("a😀", visited);
            visited = "";
            thrown = Assert.Throws<System.InvalidOperationException>(() => JSArrayStatics.from("a😀z", value =>
            {
                visited += value;
                if (value == "😀") throw error;
                return value;
            }));
            Assert.Same(error, thrown);
            Assert.Equal("a😀", visited);
        }

        [Fact]
        public void from_WithEnumerable_ReturnsArray()
        {
            var result = JSArrayStatics.from(new List<int> { 1, 2, 3 });

            Assert.IsType<JSArray<int>>(result);
            Assert.Equal(3, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
        }

        [Fact]
        public void from_WithMapFunction_ProjectsItems()
        {
            var result = JSArrayStatics.from(
                new List<string> { "a", "b", "c" },
                (value, index) => $"{index}:{value}"
            );

            Assert.IsType<JSArray<string>>(result);
            Assert.Equal(3, result.length);
            Assert.Equal("0:a", result[0]);
            Assert.Equal("1:b", result[1]);
            Assert.Equal("2:c", result[2]);
        }

        [Fact]
        public void from_WithString_ReturnsStringElements()
        {
            var result = JSArrayStatics.from("abc");

            Assert.IsType<JSArray<string>>(result);
            Assert.Equal(3, result.length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
            Assert.Equal("c", result[2]);
        }

        [Fact]
        public void from_WithStringAndMapFunction_ProjectsStringElements()
        {
            var result = JSArrayStatics.from(
                "abc",
                (value, index) => $"{index}:{value}"
            );

            Assert.IsType<JSArray<string>>(result);
            Assert.Equal(3, result.length);
            Assert.Equal("0:a", result[0]);
            Assert.Equal("1:b", result[1]);
            Assert.Equal("2:c", result[2]);
        }

        [Fact]
        public void of_ReturnsArray()
        {
            var result = JSArrayStatics.of(4, 5, 6);

            Assert.IsType<JSArray<int>>(result);
            Assert.Equal(3, result.length);
            Assert.Equal(4, result[0]);
            Assert.Equal(5, result[1]);
            Assert.Equal(6, result[2]);
        }

        [Fact]
        public void isArray_ReturnsTrue_ForArraysAndJSArray()
        {
            Assert.True(JSArrayStatics.isArray(new[] { 1, 2, 3 }));
            Assert.True(JSArrayStatics.isArray(new JSArray<int>(new[] { 1, 2, 3 })));
            Assert.False(JSArrayStatics.isArray("abc"));
            Assert.False(JSArrayStatics.isArray(null));
        }

        [Fact]
        public void slice_FromReadOnlyList_ReturnsJavaScriptArray()
        {
            IReadOnlyList<int> source = new[] { 1, 2, 3, 4 };

            var result = Tsonic.CSharp.Js.Array.slice(source, 1, 3);

            Assert.IsType<JSArray<int>>(result);
            Assert.Equal(2, result.length);
            Assert.Equal(2, result[0]);
            Assert.Equal(3, result[1]);
        }
    }
}
