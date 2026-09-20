using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        [Fact]
        public void Constructor_Empty_CreatesEmptyArray()
        {
            var arr = new JSArray<int>();
            Assert.Equal(0, arr.length);
        }

        [Fact]
        public void Constructor_Length_InitializesNativeDefaults()
        {
            var values = new JSArray<int>(3);
            Assert.Equal(new[] { 0, 0, 0 }, values.ToArray());
            Assert.True(values.hasIndex(0) && values.hasIndex(1) && values.hasIndex(2));
            Assert.Equal("0|0|0", values.join("|"));
            var references = new JSArray<string?>(2.0);
            Assert.Equal(2, references.length);
            Assert.True(references.hasIndex(0));
            Assert.Null(references[0]);
        }

        [Fact]
        public void Constructor_FromNativeArray_CreatesArrayWithItems()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Equal(3, arr.length);
            Assert.Equal(1, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(3, arr[2]);
        }

        [Fact]
        public void ExplicitOptionalValuesAreDense()
        {
            var values = new JSArray<int?>(new int?[] { 1, null, 3 });
            Assert.Equal(3, values.length);
            Assert.True(values.hasIndex(0) && values.hasIndex(1) && values.hasIndex(2));
            Assert.Null(values[1]);
            Assert.Equal("1,,3", values.join());
        }

        [Fact]
        public void Indexer_RejectsGapsBeforeMutating()
        {
            var values = new JSArray<int>();
            Assert.Throws<RangeError>(() => values[10] = 42);
            Assert.Equal(0, values.length);
            values[0] = 42;
            Assert.Equal(42, values[0]);
        }

        [Fact]
        public void length_SetToSmallerValue_TruncatesArray()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.setLength(3);

            Assert.Equal(3, result);
            Assert.Equal(3, arr.length);
        }

        [Fact]
        public void length_RejectsUninitializedGrowth()
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            Assert.Throws<RangeError>(() => values.setLength(5));
            Assert.Equal(new[] { 1, 2, 3 }, values.ToArray());
        }

        [Fact]
        public void tryGetAt_DistinguishesDefaultsFromOutOfBounds()
        {
            var numbers = new JSArray<int>(3);
            numbers[2] = 42;
            Assert.True(numbers.tryGetAt(1, out var zero));
            Assert.Equal(0, zero);
            Assert.False(numbers.tryGetAt(99, out _));
            var booleans = new JSArray<bool>(2);
            Assert.True(booleans.tryGetAt(0, out var value));
            Assert.False(value);
            var references = new JSArray<string?>(2);
            Assert.True(references.tryGetAt(0, out var absent));
            Assert.Null(absent);
        }

        [Fact]
        public void deleteAt_RejectsPresentElementsWithoutMutation()
        {
            var values = new JSArray<int>(new[] { 1, 0, 3 });
            Assert.Throws<TypeError>(() => values.deleteAt(1));
            Assert.Equal(new[] { 1, 0, 3 }, values.ToArray());
            Assert.True(values.hasIndex(1));
            Assert.Equal(1, values.indexOf(0));
            Assert.Equal("1,0,3", values.join());
            Assert.True(values.deleteAt(99));
        }

        [Fact]
        public void setLength_ClearThenPushInitializesNewValues()
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            values.setLength(0);
            Assert.Equal(0, values.length);
            Assert.False(values.hasIndex(0));
            Assert.Throws<RangeError>(() => values.setLength(3));
            values.push(5);
            values.push(7);
            Assert.Equal(new[] { 5, 7 }, values.ToArray());
            Assert.Equal(12, values.reduce((sum, value) => sum + value, 0));
        }

        [Fact]
        public void DenseCallbacksVisitInitializedDefaults()
        {
            var values = new JSArray<int>(5);
            values[1] = 2;
            values[3] = 4;
            var visited = new List<int>();
            var mapped = values.map((value, index, array) => { Assert.Same(values, array); visited.Add(index); return value * 10; });
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, visited);
            Assert.Equal(new[] { 0, 20, 0, 40, 0 }, mapped.ToArray());
            visited.Clear();
            var filtered = values.filter((value, index, array) => { Assert.Same(values, array); visited.Add(index); return value > 0; });
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, visited);
            Assert.Equal(new[] { 2, 4 }, filtered.ToArray());
            visited.Clear();
            Assert.Equal(6, values.reduce((sum, value, index, array) => { Assert.Same(values, array); visited.Add(index); return sum + value; }, 0));
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, visited);
            visited.Clear();
            Assert.True(values.every((value, index, array) => { visited.Add(index); return value % 2 == 0; }));
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, visited);
            visited.Clear();
            Assert.True(values.some((value, index, array) => { visited.Add(index); return value == 4; }));
            Assert.Equal(new[] { 0, 1, 2, 3 }, visited);
        }

        [Fact]
        public void DenseSearchIncludesInitializedDefaults()
        {
            var numbers = new JSArray<int>(3);
            Assert.Equal(0, numbers.indexOf(0));
            Assert.Equal(2, numbers.lastIndexOf(0));
            Assert.True(numbers.includes(0));
            Assert.Throws<TypeError>(() => numbers.deleteAt(1));
            var references = new JSArray<string?>(2);
            Assert.Equal(0, references.indexOf(null));
            Assert.True(references.includes(null));
            references.setLength(0);
            Assert.False(references.includes(null));
        }

        [Fact]
        public void Search_UsesStrictEqualityForIndexOfAndSameValueZeroForIncludes()
        {
            var denseNumbers = new List<double> { double.NaN, 0.0, -0.0 };
            Assert.Equal(-1, Tsonic.CSharp.Js.Array.indexOf(denseNumbers, double.NaN));
            Assert.Equal(-1, Tsonic.CSharp.Js.Array.lastIndexOf(denseNumbers, double.NaN));
            Assert.True(Tsonic.CSharp.Js.Array.includes(denseNumbers, double.NaN));
            Assert.True(Tsonic.CSharp.Js.Array.includes(denseNumbers, 0.0));
            Assert.True(Tsonic.CSharp.Js.Array.includes(denseNumbers, -0.0));
            Assert.Equal(1, Tsonic.CSharp.Js.Array.indexOf(denseNumbers, 0.0));
            Assert.Equal(1, Tsonic.CSharp.Js.Array.indexOf(denseNumbers, -0.0));

            var sparseNumbers = new JSArray<double>(new[] { 1.0, 0.0, 0.0 });
            sparseNumbers[1] = double.NaN;

            Assert.Equal(-1, sparseNumbers.indexOf(double.NaN));
            Assert.Equal(-1, sparseNumbers.lastIndexOf(double.NaN));
            Assert.True(sparseNumbers.includes(double.NaN));
            Assert.True(sparseNumbers.includes(double.NaN, -2));

            sparseNumbers[2] = -0.0;

            Assert.True(sparseNumbers.includes(0.0));
            Assert.True(sparseNumbers.includes(-0.0));
            Assert.Equal(2, sparseNumbers.indexOf(0.0));
            Assert.Equal(2, sparseNumbers.indexOf(-0.0));
        }

        [Fact]
        public void IncludesAndIndexOfDistinguishExplicitUndefinedAndNull()
        {
            var values = new JSArray<object?>(new object?[] { null, Undefined.value });
            Assert.True(values.includes(Undefined.value));
            Assert.True(values.includes(TsValue.undefined()));
            Assert.Equal(1, values.indexOf(Undefined.value));
            Assert.Equal(0, values.indexOf(null));
            Assert.True(Tsonic.CSharp.Js.Array.includes(values, Undefined.value));
            Assert.Equal(1, Tsonic.CSharp.Js.Array.indexOf(values, Undefined.value));
            Assert.Null(values.at(0));
            Assert.Same(Undefined.value, values.at(1));
            Assert.True(values.hasIndex(0) && values.hasIndex(1));
            Assert.Throws<TypeError>(() => values.deleteAt(1));
        }

        [Fact]
        public void StaticHelpers_OnJSArray_PreserveNativeDefaults()
        {
            var values = new JSArray<int>(new[] { 0, 0, 5, 0 });
            Assert.Equal(0, Tsonic.CSharp.Js.Array.atValue(values, 0));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(values, 4));
            Assert.True(Tsonic.CSharp.Js.Array.includes(values, 0));
            Assert.Equal(0, Tsonic.CSharp.Js.Array.indexOf(values, 0));
            var visited = new List<int>();
            var mapped = Tsonic.CSharp.Js.Array.map(values, (int value, int index, JSArray<int> source) => { visited.Add(index); return value + source.length; });
            Assert.Equal(new[] { 0, 1, 2, 3 }, visited);
            Assert.Equal(new[] { 4, 4, 9, 4 }, mapped.ToArray());
            Assert.Equal(0, Tsonic.CSharp.Js.Array.popValue(values));
            Assert.Equal(5, Tsonic.CSharp.Js.Array.popValue(values));
            Assert.Equal(2, values.length);
        }

        [Fact]
        public void DenseCopyingMethods_PreserveExplicitNullsAndIndependence()
        {
            var source = new JSArray<string?>(new string?[] { null, "b", null, "d", null });
            Assert.Equal(new string?[] { null, "b", null, "d" }, source.slice(0, 4).ToArray());
            Assert.Equal(new string?[] { null, "b", null, "d" }, source.slice(0, 2).concat(source.slice(2, 4)).ToArray());
            Assert.Equal(source.ToArray(), new JSArray<object?>().concat(source).ToArray());
            var copied = source.slice();
            copied.copyWithin(0, 1, 4);
            Assert.Equal(new string?[] { "b", null, "d", "d", null }, copied.ToArray());
            Assert.Equal(new string?[] { null, "d", null, "b", null }, source.toReversed().ToArray());
            Assert.Equal(new string?[] { "b", "d", null, null, null }, source.toSorted().ToArray());
            Assert.Equal(new string?[] { null, "x", "d", null }, source.toSpliced(1, 2, "x").ToArray());
            Assert.Equal(new string?[] { "a", "b", null, "d", null }, source.with(0, "a").ToArray());
            Assert.Equal(new string?[] { null, "b", null, "d", null }, source.ToArray());
        }

        [Fact]
        public void DenseFlatAndFlatMapRetainExplicitValues()
        {
            var inner = new JSArray<object?>(new object?[] { null, 2, null });
            var outer = new JSArray<object?>(new object?[] { 1, null, inner, 3 });
            Assert.Equal(new object?[] { 1, null, null, 2, null, 3 }, outer.flat(1).ToArray());
            var nonArray = new List<int> { 4, 5 };
            Assert.Same(nonArray, new JSArray<object>(new object[] { nonArray }).flat(1)[0]);
            var source = new JSArray<int>(new[] { 0, 5, 0 });
            var visited = new List<int>();
            var mapped = source.flatMap<int>((value, index, array) => { visited.Add(index); return new JSArray<int>(new[] { 0, value, value * 2 }); });
            Assert.Equal(new[] { 0, 1, 2 }, visited);
            Assert.Equal(new[] { 0, 0, 0, 0, 5, 10, 0, 0, 0 }, mapped.ToArray());
            var objects = source.flatMap<object>((value, index, array) => new JSArray<int>(new[] { 0, value }));
            Assert.Equal(new object[] { 0, 0, 0, 5, 0, 0 }, objects.ToArray());
        }
    }
}
