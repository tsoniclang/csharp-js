using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        [Fact]
        public void push_AddsItemToEnd()
        {
            var arr = new JSArray<string>(new[] { "a", "b" });
            arr.push("c");

            Assert.Equal(3, arr.length);
            Assert.Equal("c", arr[2]);
        }

        [Fact]
        public void pop_RemovesAndReturnsLastItem()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            var result = arr.pop();

            Assert.Equal("c", result);
            Assert.Equal(2, arr.length);
        }

        [Fact]
        public void pop_EmptyArray_ReturnsDefault()
        {
            var arr = new JSArray<string>();
            var result = arr.pop();

            Assert.Null(result);
            Assert.Equal(0, arr.length);
        }

        [Fact]
        public void shift_RemovesAndReturnsFirstItem()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            var result = arr.shift();

            Assert.Equal("a", result);
            Assert.Equal(2, arr.length);
            Assert.Equal("b", arr[0]);
            Assert.Equal("c", arr[1]);
        }

        [Fact]
        public void shift_EmptyArray_ReturnsDefault()
        {
            var arr = new JSArray<string>();
            var result = arr.shift();

            Assert.Null(result);
            Assert.Equal(0, arr.length);
        }

        [Fact]
        public void unshift_AddsItemToBeginning()
        {
            var arr = new JSArray<string>(new[] { "b", "c" });
            arr.unshift("a");

            Assert.Equal(3, arr.length);
            Assert.Equal("a", arr[0]);
            Assert.Equal("b", arr[1]);
            Assert.Equal("c", arr[2]);
        }

        [Fact]
        public void slice_NoArguments_CopiesEntireArray()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.slice();

            Assert.Equal(3, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
        }

        [Fact]
        public void slice_WithStart_CopiesFromStart()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.slice(2);

            Assert.Equal(3, result.length);
            Assert.Equal(3, result[0]);
            Assert.Equal(4, result[1]);
            Assert.Equal(5, result[2]);
        }

        [Fact]
        public void slice_WithStartAndEnd_CopiesRange()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.slice(1, 4);

            Assert.Equal(3, result.length);
            Assert.Equal(2, result[0]);
            Assert.Equal(3, result[1]);
            Assert.Equal(4, result[2]);
        }

        [Fact]
        public void slice_NegativeIndices_CountsFromEnd()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.slice(-3, -1);

            Assert.Equal(2, result.length);
            Assert.Equal(3, result[0]);
            Assert.Equal(4, result[1]);
        }

        [Fact]
        public void indexOf_ItemExists_ReturnsIndex()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.Equal(1, arr.indexOf("b"));
        }

        [Fact]
        public void indexOf_ItemNotFound_ReturnsNegativeOne()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.Equal(-1, arr.indexOf("d"));
        }

        [Fact]
        public void indexOf_WithFromIndex_StartsSearch()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c", "b" });
            Assert.Equal(3, arr.indexOf("b", 2));
            Assert.Equal(3, arr.indexOf("b", -1));
            Assert.Equal(1, arr.indexOf("b", -99));
            Assert.Equal(-1, arr.indexOf("b", 99));
        }

        [Fact]
        public void includes_ItemExists_ReturnsTrue()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.True(arr.includes("b"));
            Assert.True(arr.includes("b", -2));
            Assert.False(arr.includes("b", 2));
        }

        [Fact]
        public void includes_ItemNotFound_ReturnsFalse()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.False(arr.includes("d"));
        }

        [Fact]
        public void join_DefaultSeparator_UsesComma()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.Equal("a,b,c", arr.join());
        }

        [Fact]
        public void join_CustomSeparator_UsesProvided()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            Assert.Equal("a-b-c", arr.join("-"));
        }

        [Fact]
        public void join_SparseArray_HandlesHoles()
        {
            var arr = new JSArray<string>();
            arr[0] = "a";
            arr[2] = "c";

            Assert.Equal("a,,c", arr.join(","));
        }

        [Fact]
        public void join_TreatsHolesUndefinedAndNullAsEmptyStrings()
        {
            var values = new JSArray<object?>();
            values.setLength(4);
            values[1] = JSUndefined.value;
            values[2] = null;
            values[3] = "x";

            Assert.Equal(",,,x", values.join());
            Assert.Equal("|||x", values.join("|"));

            var denseValues = new List<object?> { JSUndefined.value, null, "x", true, 3.5 };
            Assert.Equal(",,x,true,3.5", Tsonic.CSharp.Js.Array.join(denseValues));

            var broadValues = new JSArray<TsValue>(new[] { TsValue.undefined(), TsValue.from("x"), TsValue.from(false) });
            Assert.Equal(",x,false", broadValues.join());
        }

        [Fact]
        public void reverse_ReversesArrayInPlace()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            arr.reverse();

            Assert.Equal(3, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(1, arr[2]);
        }

        [Fact]
        public void toArray_ConvertsToNativeArray()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var native = arr.toArray();

            Assert.Equal(3, native.Length);
            Assert.Equal(1, native[0]);
            Assert.Equal(2, native[1]);
            Assert.Equal(3, native[2]);
        }

        [Fact]
        public void toArray_SparseArray_FillsHolesWithDefault()
        {
            var arr = new JSArray<int>();
            arr[0] = 1;
            arr[2] = 3;

            var native = arr.toArray();
            Assert.Equal(3, native.Length);
            Assert.Equal(1, native[0]);
            Assert.Equal(0, native[1]); // Hole filled with default
            Assert.Equal(3, native[2]);
        }

        [Fact]
        public void GetEnumerator_AllowsForeach()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var sum = 0;

            foreach (var item in arr)
            {
                sum += item;
            }

            Assert.Equal(6, sum);
        }

        [Fact]
        public void GetEnumerator_SparseArray_IncludesDefaultForHoles()
        {
            var arr = new JSArray<int>();
            arr[0] = 1;
            arr[2] = 3;

            var items = arr.toList();
            Assert.Equal(3, items.Count);
            Assert.Equal(1, items[0]);
            Assert.Equal(0, items[1]); // Hole yields default
            Assert.Equal(3, items[2]);
        }
    }
}
