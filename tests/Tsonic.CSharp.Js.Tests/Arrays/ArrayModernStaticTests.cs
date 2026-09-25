using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        [Fact]
        public void flat_FlattensNestedArrays()
        {
            var arr = new JSArray<object>(new object[] { 1, 2, 3 });
            var result = arr.flat(1);

            Assert.Equal(3, result.length);
        }

        [Fact]
        public void flatMap_MapsAndFlattens()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.flatMap<int>((x, i, a) => new JSArray<int>(new[] { x, x * 2 }));

            Assert.Equal(6, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(2, result[2]);
            Assert.Equal(4, result[3]);
        }

        [Fact]
        public void fill_FillsWithValue()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            arr.fill(0, 1, 4);

            Assert.Equal(1, arr[0]);
            Assert.Equal(0, arr[1]);
            Assert.Equal(0, arr[2]);
            Assert.Equal(0, arr[3]);
            Assert.Equal(5, arr[4]);
        }

        [Fact]
        public void copyWithin_CopiesSection()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            arr.copyWithin(0, 3, 5);

            Assert.Equal(4, arr[0]);
            Assert.Equal(5, arr[1]);
            Assert.Equal(3, arr[2]);
            Assert.Equal(4, arr[3]);
            Assert.Equal(5, arr[4]);
        }

        [Fact]
        public void entries_ReturnsIndexValuePairs()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            var entries = arr.entries().ToList();

            Assert.Equal(3, entries.Count);
            Assert.Equal((0, "a"), entries[0]);
            Assert.Equal((1, "b"), entries[1]);
            Assert.Equal((2, "c"), entries[2]);
        }

        [Fact]
        public void keys_ReturnsIndices()
        {
            var arr = new JSArray<int>(new[] { 10, 20, 30 });
            var keys = arr.keys().ToList();

            Assert.Equal(3, keys.Count);
            Assert.Equal(0, keys[0]);
            Assert.Equal(1, keys[1]);
            Assert.Equal(2, keys[2]);
        }

        [Fact]
        public void values_ReturnsValues()
        {
            var arr = new JSArray<int>(new[] { 10, 20, 30 });
            var values = arr.values().ToList();

            Assert.Equal(3, values.Count);
            Assert.Equal(10, values[0]);
            Assert.Equal(20, values[1]);
            Assert.Equal(30, values[2]);
        }

        [Fact]
        public void ToString_ReturnsCommaSeparatedString()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal("1,2,3", arr.ToString());
        }

        [Fact]
        public void toLocaleString_ReturnsCommaSeparatedString()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal("1,2,3", arr.toLocaleString());
        }

        [Fact]
        public void with_ReplacesElement_Immutably()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.with(1, 99);

            Assert.Equal(3, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(99, result[1]);
            Assert.Equal(3, result[2]);

            // Original unchanged
            Assert.Equal(2, arr[1]);
        }

        [Fact]
        public void toReversed_ReversesImmutably()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.toReversed();

            Assert.Equal(3, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(1, result[2]);

            // Original unchanged
            Assert.Equal(1, arr[0]);
        }

        [Fact]
        public void toSorted_SortsImmutably()
        {
            var arr = new JSArray<int>(new[] { 3, 1, 2 });
            var result = arr.toSorted((a, b) => a - b);

            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);

            // Original unchanged
            Assert.Equal(3, arr[0]);
        }

        [Fact]
        public void toSpliced_SplicesImmutably()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4 });
            var result = arr.toSpliced(1, 2, 99);

            Assert.Equal(3, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(99, result[1]);
            Assert.Equal(4, result[2]);

            // Original unchanged
            Assert.Equal(4, arr.length);
        }

        [Fact]
        public void isArray_WithJSArray_ReturnsTrue()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.True(JSArray<int>.isArray(arr));
        }

        [Fact]
        public void isArray_WithNonArray_ReturnsFalse()
        {
            Assert.False(JSArray<int>.isArray("not an array"));
            Assert.False(JSArray<int>.isArray(null));
        }

        [Fact]
        public void from_CreatesArrayFromEnumerable()
        {
            var list = new[] { 1, 2, 3 };
            var arr = JSArray<int>.from(list);

            Assert.Equal(3, arr.length);
            Assert.Equal(1, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(3, arr[2]);
        }

        [Fact]
        public void from_WithMapFunc_TransformsElements()
        {
            var list = new[] { 1, 2, 3 };
            var arr = JSArray<int>.from<int, int>(list, (x, i) => x * 2);

            Assert.Equal(3, arr.length);
            Assert.Equal(2, arr[0]);
            Assert.Equal(4, arr[1]);
            Assert.Equal(6, arr[2]);
        }

        [Fact]
        public void of_CreatesArrayFromArguments()
        {
            var arr = JSArray<int>.of(1, 2, 3, 4);

            Assert.Equal(4, arr.length);
            Assert.Equal(1, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(3, arr[2]);
            Assert.Equal(4, arr[3]);
        }

        [Fact]
        public void setLength_Truncates()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            arr.setLength(3);

            Assert.Equal(3, arr.length);
            Assert.Equal(1, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(3, arr[2]);
        }
    }
}
