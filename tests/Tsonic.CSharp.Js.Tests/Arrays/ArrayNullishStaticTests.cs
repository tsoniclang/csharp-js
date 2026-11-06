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
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, 1));
        }

        [Fact]
        public void at_NegativeIndex_CountsFromEnd()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal(3, Tsonic.CSharp.Js.Array.atValue(arr, -1));
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, -2));
        }

        [Fact]
        public void at_OutOfBounds_ReturnsNullishCarrier()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal(TsValue.undefined().unwrap(), (object?)Tsonic.CSharp.Js.Array.atValue(arr, 3));
            Assert.Equal(TsValue.undefined().unwrap(), (object?)Tsonic.CSharp.Js.Array.atValue(arr, -4));
        }

        [Fact]
        public void at_CollapsesSourceAbsenceButRetainsSlots()
        {
            var arr = new JSArray<object?>(new object?[] { TsValue.undefined().unwrap(), null });

            Assert.Same(TsValue.undefined().unwrap(), Tsonic.CSharp.Js.Array.atReference(arr, 0));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(arr, 1));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(arr, 0));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(arr, 1));
        }

        [Fact]
        public void StaticAt_ReturnsElementOrNullishCarrier()
        {
            var arr = new List<int> { 1, 2, 3 };
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.atValue(arr, -1));
            Assert.Equal(TsValue.undefined().unwrap(), (object?)Tsonic.CSharp.Js.Array.atValue(arr, 3));
            Assert.Equal(TsValue.undefined().unwrap(), (object?)Tsonic.CSharp.Js.Array.atValue(arr, -4));
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.atValue(arr, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, 3));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, -4));
            var strings = new List<string> { "a", "b" };
            Assert.Equal("b", Tsonic.CSharp.Js.Array.atReference(strings, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(strings, 2));
        }

        [Fact]
        public void StaticAt_HasOnlyConstrainedNativeIntegerEndpoints()
        {
            var endpoints = typeof(Tsonic.CSharp.Js.Array).GetMethods()
                .Where(method => method.Name is "atValue" or "atReference").ToArray();
            Assert.Equal(4, endpoints.Length);
            foreach (var endpoint in endpoints)
                Assert.Equal(typeof(int), endpoint.GetParameters()[1].ParameterType);
            Assert.DoesNotContain(typeof(Tsonic.CSharp.Js.Array).GetMethods(), method => method.Name == "at");
            Assert.DoesNotContain(typeof(JSArray<int>).GetMethods(), method => method.Name is "at" or "atValue" or "atReference");
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(new List<int> { 0 }, int.MinValue));
            Assert.Equal(0, Tsonic.CSharp.Js.Array.atValue(new List<int> { 0 }, 0));
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
