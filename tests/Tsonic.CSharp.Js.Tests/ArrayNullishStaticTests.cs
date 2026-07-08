using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        [Fact]
        public void at_PositiveIndex_ReturnsElement()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal(2, arr.at(1));
        }

        [Fact]
        public void at_NegativeIndex_CountsFromEnd()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal(3, arr.at(-1));
            Assert.Equal(2, arr.at(-2));
        }

        [Fact]
        public void at_OutOfBounds_ReturnsNullishCarrier()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Same(JSUndefined.value, arr.at(3));
            Assert.Same(JSUndefined.value, arr.at(-4));
        }

        [Fact]
        public void at_DistinguishesSparseUndefinedFromPresentNull()
        {
            var arr = new JSArray<object?>();
            arr.setLength(2);
            arr[1] = null;

            Assert.Same(JSUndefined.value, arr.at(0));
            Assert.Null(arr.at(1));
            Assert.Null(arr.atReference<object>(0));
            Assert.Null(arr.atReference<object>(1));
        }

        [Fact]
        public void StaticAt_ReturnsElementOrNullishCarrier()
        {
            var arr = new List<int> { 1, 2, 3 };
            Assert.Equal(2, Tsonic.CSharp.Js.Array.at(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.at(arr, -1));
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(arr, 3));
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(arr, -4));
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.atValue(arr, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, 3));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, -4));
            var strings = new List<string> { "a", "b" };
            Assert.Equal("b", Tsonic.CSharp.Js.Array.atReference(strings, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(strings, 2));
        }

        [Fact]
        public void StaticAt_DoubleIndexes_FollowJsIntegerConversion()
        {
            var numbers = new List<int> { 10, 20, 30 };
            Assert.Equal(10, Tsonic.CSharp.Js.Array.at(numbers, double.NaN));
            Assert.Equal(20, Tsonic.CSharp.Js.Array.at(numbers, 1.8));
            Assert.Equal(30, Tsonic.CSharp.Js.Array.at(numbers, -1.8));
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(numbers, double.PositiveInfinity));
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(numbers, double.NegativeInfinity));
            Assert.Equal(10, Tsonic.CSharp.Js.Array.atValue(numbers, double.NaN));
            Assert.Equal(20, Tsonic.CSharp.Js.Array.atValue(numbers, 1.8));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(numbers, double.PositiveInfinity));

            var strings = new List<string> { "a", "b", "c" };
            Assert.Equal("a", Tsonic.CSharp.Js.Array.atReference(strings, double.NaN));
            Assert.Equal("b", Tsonic.CSharp.Js.Array.atReference(strings, 1.8));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(strings, double.NegativeInfinity));

            var sparse = new JSArray<object?>();
            sparse.setLength(2);
            sparse[1] = "present";
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(sparse, double.NaN));
            Assert.Equal("present", Tsonic.CSharp.Js.Array.at(sparse, 1.8));
            Assert.Same(JSUndefined.value, Tsonic.CSharp.Js.Array.at(sparse, double.PositiveInfinity));
        }

        [Fact]
        public void StaticPopShift_ReturnTypedNullishCarriers()
        {
            var numbers = new List<int> { 1, 2 };
            Assert.Equal(2, Tsonic.CSharp.Js.Array.popValue(numbers));
            Assert.Equal(1, Tsonic.CSharp.Js.Array.shiftValue(numbers));
            Assert.Null(Tsonic.CSharp.Js.Array.popValue(numbers));
            Assert.Null(Tsonic.CSharp.Js.Array.shiftValue(numbers));

            var strings = new List<string> { "a", "b" };
            Assert.Equal("b", Tsonic.CSharp.Js.Array.popReference(strings));
            Assert.Equal("a", Tsonic.CSharp.Js.Array.shiftReference(strings));
            Assert.Null(Tsonic.CSharp.Js.Array.popReference(strings));
            Assert.Null(Tsonic.CSharp.Js.Array.shiftReference(strings));
        }

        [Fact]
        public void StaticFind_ReturnTypedNullishCarriers()
        {
            var numbers = new List<int> { 1, 2, 3, 4 };
            Assert.Equal(3, Tsonic.CSharp.Js.Array.findValue(numbers, value => value > 2));
            Assert.Equal(4, Tsonic.CSharp.Js.Array.findValue(numbers, (value, index) => value + index > 5));
            Assert.Equal(4, Tsonic.CSharp.Js.Array.findValue(numbers, (value, index, array) => index == array.Count - 1 && value == 4));
            Assert.Null(Tsonic.CSharp.Js.Array.findValue(numbers, value => value > 9));

            var strings = new List<string> { "a", "bb", "ccc" };
            Assert.Equal("bb", Tsonic.CSharp.Js.Array.findReference(strings, value => value.Length == 2));
            Assert.Equal("ccc", Tsonic.CSharp.Js.Array.findReference(strings, (value, index) => index == 2 && value.Length == 3));
            Assert.Equal("ccc", Tsonic.CSharp.Js.Array.findReference(strings, (value, index, array) => index == array.Count - 1));
            Assert.Null(Tsonic.CSharp.Js.Array.findReference(strings, value => value.Length > 9));
        }

        [Fact]
        public void StaticFindLast_ReturnTypedNullishCarriers()
        {
            var numbers = new List<int> { 1, 2, 3, 4 };
            Assert.Equal(4, Tsonic.CSharp.Js.Array.findLastValue(numbers, value => value > 2));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.findLastValue(numbers, (value, index) => value + index < 6));
            Assert.Equal(4, Tsonic.CSharp.Js.Array.findLastValue(numbers, (value, index, array) => index == array.Count - 1 && value == 4));
            Assert.Null(Tsonic.CSharp.Js.Array.findLastValue(numbers, value => value > 9));

            var strings = new List<string> { "a", "bb", "ccc", "dd" };
            Assert.Equal("dd", Tsonic.CSharp.Js.Array.findLastReference(strings, value => value.Length == 2));
            Assert.Equal("ccc", Tsonic.CSharp.Js.Array.findLastReference(strings, (value, index) => index < 3 && value.Length == 3));
            Assert.Equal("dd", Tsonic.CSharp.Js.Array.findLastReference(strings, (value, index, array) => index == array.Count - 1));
            Assert.Null(Tsonic.CSharp.Js.Array.findLastReference(strings, value => value.Length > 9));
        }
    }
}
