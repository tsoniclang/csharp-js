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
        public void Constructor_Length_CreatesHoles()
        {
            var arr = new JSArray<int>(3);

            Assert.Equal(3, arr.length);
            Assert.False(arr.hasIndex(0));
            Assert.False(arr.hasIndex(1));
            Assert.False(arr.hasIndex(2));
            Assert.Equal(0, arr[0]);
            Assert.Equal("||", arr.join("|"));

            var numericLength = new JSArray<string>(2.0);
            Assert.Equal(2, numericLength.length);
            Assert.False(numericLength.hasIndex(0));
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
        public void FromSparse_CreatesArrayWithExplicitHoles()
        {
            var arr = JSArray<int>.fromSparse(3, (0, 1), (2, 3));

            Assert.Equal(3, arr.length);
            Assert.True(arr.hasIndex(0));
            Assert.False(arr.hasIndex(1));
            Assert.True(arr.hasIndex(2));
            Assert.Equal(1, arr[0]);
            Assert.Equal(0, arr[1]);
            Assert.Equal(3, arr[2]);
            Assert.Equal("1,,3", arr.join());
        }

        [Fact]
        public void Indexer_SparseArray_SupportsHoles()
        {
            var arr = new JSArray<int>();
            arr[10] = 42;

            Assert.Equal(11, arr.length);
            Assert.Equal(0, arr[0]); // Hole reads as undefined/default
            Assert.False(arr.hasIndex(0));
            Assert.True(arr.hasIndex(10));
            Assert.Equal(42, arr[10]);
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
        public void length_SetToLargerValue_ExtendsArray()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.setLength(5);

            Assert.Equal(5, result);
            Assert.Equal(5, arr.length);
            Assert.Equal(0, arr[4]); // New hole reads as undefined/default
            Assert.False(arr.hasIndex(4));
        }

        [Fact]
        public void tryGetAt_DistinguishesPresentDefaultsFromHoles()
        {
            var numbers = new JSArray<int>();
            numbers[0] = 0;
            numbers[2] = 42;

            Assert.True(numbers.tryGetAt(0, out var zero));
            Assert.Equal(0, zero);
            Assert.False(numbers.tryGetAt(1, out var holeNumber));
            Assert.Equal(0, holeNumber);
            Assert.False(numbers.tryGetAt(99, out var missingNumber));
            Assert.Equal(0, missingNumber);

            var booleans = new JSArray<bool>();
            booleans[0] = false;
            booleans.setLength(2);

            Assert.True(booleans.tryGetAt(0, out var falseValue));
            Assert.False(falseValue);
            Assert.False(booleans.tryGetAt(1, out _));

            var nullableStrings = new JSArray<string?>();
            nullableStrings[0] = null;
            nullableStrings.setLength(2);

            Assert.True(nullableStrings.tryGetAt(0, out var nullValue));
            Assert.Null(nullValue);
            Assert.False(nullableStrings.tryGetAt(1, out _));
        }

        [Fact]
        public void deleteAt_CreatesHoleWithoutChangingLength()
        {
            var arr = new JSArray<int>(new[] { 1, 0, 3 });

            Assert.True(arr.deleteAt(1));

            Assert.Equal(3, arr.length);
            Assert.Equal(0, arr[1]);
            Assert.False(arr.hasIndex(1));
            Assert.False(arr.tryGetAt(1, out var deletedValue));
            Assert.Equal(0, deletedValue);
            Assert.Equal(-1, arr.indexOf(0));
            Assert.Equal("1,,3", arr.join());
            Assert.True(arr.deleteAt(99));
            Assert.Equal(3, arr.length);

            arr[1] = 0;

            Assert.True(arr.hasIndex(1));
            Assert.Equal(1, arr.indexOf(0));
        }

        [Fact]
        public void setLength_ClearAndExtend_UsesHoles()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });

            arr.setLength(0);

            Assert.Equal(0, arr.length);
            Assert.False(arr.hasIndex(0));

            arr.setLength(3);

            Assert.Equal(3, arr.length);
            Assert.False(arr.hasIndex(0));
            Assert.False(arr.hasIndex(1));
            Assert.False(arr.hasIndex(2));

            arr[1] = 5;
            arr.push(7);

            Assert.Equal(4, arr.length);
            Assert.Equal(5, arr[1]);
            Assert.Equal(7, arr[3]);
            Assert.Equal(12, arr.reduce((sum, value) => sum + value, 0));
        }

        [Fact]
        public void SparseCallbacks_SkipHolesAndMapPreservesHoles()
        {
            var arr = new JSArray<int>();
            arr.setLength(5);
            arr[1] = 2;
            arr[3] = 4;

            var mapIndices = new List<int>();
            var mapped = arr.map((value, index, array) =>
            {
                mapIndices.Add(index);
                return value * 10;
            });

            Assert.Equal(new[] { 1, 3 }, mapIndices);
            Assert.Equal(5, mapped.length);
            Assert.False(mapped.hasIndex(0));
            Assert.True(mapped.hasIndex(1));
            Assert.Equal(20, mapped[1]);
            Assert.False(mapped.hasIndex(2));
            Assert.True(mapped.hasIndex(3));
            Assert.Equal(40, mapped[3]);
            Assert.False(mapped.hasIndex(4));

            var filterIndices = new List<int>();
            var filtered = arr.filter((value, index, array) =>
            {
                filterIndices.Add(index);
                return true;
            });

            Assert.Equal(new[] { 1, 3 }, filterIndices);
            Assert.Equal(2, filtered.length);
            Assert.Equal(2, filtered[0]);
            Assert.Equal(4, filtered[1]);

            var reduceIndices = new List<int>();
            var sum = arr.reduce((accumulator, value, index, array) =>
            {
                reduceIndices.Add(index);
                return accumulator + value;
            }, 0);

            Assert.Equal(6, sum);
            Assert.Equal(new[] { 1, 3 }, reduceIndices);

            var everyIndices = new List<int>();
            Assert.True(arr.every((value, index, array) =>
            {
                everyIndices.Add(index);
                return value % 2 == 0;
            }));
            Assert.Equal(new[] { 1, 3 }, everyIndices);

            var someIndices = new List<int>();
            Assert.True(arr.some((value, index, array) =>
            {
                someIndices.Add(index);
                return value == 4;
            }));
            Assert.Equal(new[] { 1, 3 }, someIndices);
        }

        [Fact]
        public void SparseSearch_DoesNotMatchHolesAsDefaultValues()
        {
            var numbers = new JSArray<int>();
            numbers.setLength(3);
            numbers[1] = 0;

            Assert.Equal(1, numbers.indexOf(0));
            Assert.Equal(1, numbers.lastIndexOf(0));
            Assert.True(numbers.includes(0));

            numbers.deleteAt(1);

            Assert.Equal(-1, numbers.indexOf(0));
            Assert.Equal(-1, numbers.lastIndexOf(0));
            Assert.False(numbers.includes(0));

            var nullableStrings = new JSArray<string?>();
            nullableStrings.setLength(2);
            nullableStrings[1] = null;

            Assert.Equal(1, nullableStrings.indexOf(null));
            Assert.True(nullableStrings.includes(null));

            nullableStrings.deleteAt(1);

            Assert.Equal(-1, nullableStrings.indexOf(null));
            Assert.False(nullableStrings.includes(null));
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

            var sparseNumbers = new JSArray<double>();
            sparseNumbers.setLength(3);
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
        public void Includes_TreatsSparseHolesAsUndefinedButIndexOfSkipsThem()
        {
            var values = new JSArray<object?>();
            values.setLength(2);
            values[1] = null;

            Assert.True(values.includes(JSUndefined.value));
            Assert.True(values.includes(TsValue.undefined()));
            Assert.Equal(-1, values.indexOf(JSUndefined.value));
            Assert.True(values.includes(null));
            Assert.Equal(1, values.indexOf(null));

            Assert.True(Tsonic.CSharp.Js.Array.includes(values, JSUndefined.value));
            Assert.Equal(-1, Tsonic.CSharp.Js.Array.indexOf(values, JSUndefined.value));

            var explicitUndefined = new JSArray<object?>();
            explicitUndefined.setLength(2);
            explicitUndefined[1] = JSUndefined.value;

            Assert.Same(JSUndefined.value, explicitUndefined.at(0));
            Assert.Same(JSUndefined.value, explicitUndefined.at(1));
            Assert.False(explicitUndefined.hasIndex(0));
            Assert.True(explicitUndefined.hasIndex(1));
            Assert.True(explicitUndefined.includes(JSUndefined.value));
            Assert.True(explicitUndefined.includes(TsValue.undefined()));
            Assert.Equal(1, explicitUndefined.indexOf(JSUndefined.value));
            Assert.Equal(1, explicitUndefined.indexOf(TsValue.undefined()));

            explicitUndefined.deleteAt(1);

            Assert.True(explicitUndefined.includes(JSUndefined.value));
            Assert.Equal(-1, explicitUndefined.indexOf(JSUndefined.value));
        }

        [Fact]
        public void StaticHelpers_OnJSArray_PreserveHoleSemantics()
        {
            var numbers = new JSArray<int>();
            numbers.setLength(4);
            numbers[1] = 0;
            numbers[2] = 5;

            Assert.Null(Tsonic.CSharp.Js.Array.atValue(numbers, 0));
            Assert.Equal(0, Tsonic.CSharp.Js.Array.atValue(numbers, 1));
            Assert.True(Tsonic.CSharp.Js.Array.includes(numbers, 0));
            Assert.Equal(1, Tsonic.CSharp.Js.Array.indexOf(numbers, 0));
            Assert.Equal(-1, Tsonic.CSharp.Js.Array.indexOf(numbers, 9));

            var visited = new List<int>();
            var mapped = Tsonic.CSharp.Js.Array.map(numbers, (int value, int index, JSArray<int> source) =>
            {
                visited.Add(index);
                return value + source.length;
            });

            Assert.Equal(new[] { 1, 2 }, visited);
            Assert.Equal(4, mapped.length);
            Assert.False(mapped.hasIndex(0));
            Assert.Equal(4, mapped[1]);
            Assert.Equal(9, mapped[2]);
            Assert.False(mapped.hasIndex(3));

            Assert.Null(Tsonic.CSharp.Js.Array.popValue(numbers));
            Assert.Equal(3, numbers.length);
            Assert.Equal(5, Tsonic.CSharp.Js.Array.popValue(numbers));
            Assert.Equal(2, numbers.length);
        }

        [Fact]
        public void SparseCopyingMethods_PreserveHoleState()
        {
            var source = new JSArray<string>();
            source.setLength(5);
            source[1] = "b";
            source[3] = "d";

            var slice = source.slice(0, 4);
            Assert.Equal(4, slice.length);
            Assert.False(slice.hasIndex(0));
            Assert.Equal("b", slice[1]);
            Assert.False(slice.hasIndex(2));
            Assert.Equal("d", slice[3]);

            var concat = source.slice(0, 2).concat(source.slice(2, 4));
            Assert.Equal(4, concat.length);
            Assert.False(concat.hasIndex(0));
            Assert.Equal("b", concat[1]);
            Assert.False(concat.hasIndex(2));
            Assert.Equal("d", concat[3]);

            var objectConcat = new JSArray<object>().concat(source);
            Assert.Equal(5, objectConcat.length);
            Assert.False(objectConcat.hasIndex(0));
            Assert.Equal("b", objectConcat[1]);
            Assert.False(objectConcat.hasIndex(2));
            Assert.Equal("d", objectConcat[3]);
            Assert.False(objectConcat.hasIndex(4));

            var copied = source.slice();
            copied.copyWithin(0, 1, 4);
            Assert.Equal("b", copied[0]);
            Assert.False(copied.hasIndex(1));
            Assert.Equal("d", copied[2]);
            Assert.Equal("d", copied[3]);
            Assert.False(copied.hasIndex(4));

            var reversed = source.toReversed();
            Assert.False(reversed.hasIndex(0));
            Assert.Equal("d", reversed[1]);
            Assert.False(reversed.hasIndex(2));
            Assert.Equal("b", reversed[3]);
            Assert.False(reversed.hasIndex(4));

            var sorted = source.toSorted();
            Assert.Equal("b", sorted[0]);
            Assert.Equal("d", sorted[1]);
            Assert.False(sorted.hasIndex(2));
            Assert.False(sorted.hasIndex(3));
            Assert.False(sorted.hasIndex(4));

            var spliced = source.toSpliced(1, 2, "x");
            Assert.Equal(4, spliced.length);
            Assert.False(spliced.hasIndex(0));
            Assert.Equal("x", spliced[1]);
            Assert.Equal("d", spliced[2]);
            Assert.False(spliced.hasIndex(3));

            var replaced = source.with(0, "a");
            Assert.Equal("a", replaced[0]);
            Assert.Equal("b", replaced[1]);
            Assert.False(replaced.hasIndex(2));
        }

        [Fact]
        public void SparseFlatAndFlatMap_SkipSourceAndNestedHoles()
        {
            var inner = new JSArray<object>();
            inner.setLength(3);
            inner[1] = 2;

            var outer = new JSArray<object>();
            outer.setLength(4);
            outer[0] = 1;
            outer[2] = inner;
            outer[3] = 3;

            var flattened = outer.flat(1);

            Assert.Equal(3, flattened.length);
            Assert.Equal(1, flattened[0]);
            Assert.Equal(2, flattened[1]);
            Assert.Equal(3, flattened[2]);

            var nonArray = new List<int> { 4, 5 };
            var nonArrayOuter = new JSArray<object>(new object[] { nonArray });
            var nonArrayFlattened = nonArrayOuter.flat(1);

            Assert.Equal(1, nonArrayFlattened.length);
            Assert.Same(nonArray, nonArrayFlattened[0]);

            var flatMapSource = new JSArray<int>();
            flatMapSource.setLength(3);
            flatMapSource[1] = 5;

            var callbackIndices = new List<int>();
            var flatMapped = flatMapSource.flatMap<int>((value, index, array) =>
            {
                callbackIndices.Add(index);
                var result = new JSArray<int>();
                result.setLength(3);
                result[1] = value;
                result[2] = value * 2;
                return result;
            });

            Assert.Equal(new[] { 1 }, callbackIndices);
            Assert.Equal(2, flatMapped.length);
            Assert.Equal(5, flatMapped[0]);
            Assert.Equal(10, flatMapped[1]);

            var objectFlatMapped = flatMapSource.flatMap<object>((value, index, array) =>
            {
                var result = new JSArray<int>();
                result.setLength(2);
                result[1] = value;
                return result;
            });

            Assert.Equal(1, objectFlatMapped.length);
            Assert.Equal(5, objectFlatMapped[0]);
        }
    }
}
