using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class ArrayTests
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
            arr.setLength(3);

            Assert.Equal(3, arr.length);
        }

        [Fact]
        public void length_SetToLargerValue_ExtendsArray()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            arr.setLength(5);

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

        [Fact]
        public void map_TransformsElements()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var result = arr.map((x, i, a) => x * 2);

            Assert.Equal(3, result.length);
            Assert.Equal(2, result[0]);
            Assert.Equal(4, result[1]);
            Assert.Equal(6, result[2]);
        }

        [Fact]
        public void filter_FiltersElements()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.filter((x, i, a) => x % 2 == 0);

            Assert.Equal(2, result.length);
            Assert.Equal(2, result[0]);
            Assert.Equal(4, result[1]);
        }

        [Fact]
        public void reduce_WithInitialValue_ReducesToSum()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4 });
            var result = arr.reduce((acc, x, i, a) => acc + x, 0);

            Assert.Equal(10, result);
        }

        [Fact]
        public void reduce_NoInitialValue_ReducesToSum()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4 });
            var result = arr.reduce((acc, x) => acc + x);

            Assert.Equal(10, result);
        }

        [Fact]
        public void reduceRight_WithInitialValue_ReducesFromRight()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            var result = arr.reduceRight((acc, x, i, a) => acc + x, "");

            Assert.Equal("cba", result);
        }

        [Fact]
        public void forEach_IteratesOverElements()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3 });
            var sum = 0;
            arr.forEach((x, i, a) => { sum += x; });

            Assert.Equal(6, sum);
        }

        [Fact]
        public void splice_RemovesAndInsertsElements()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var deleted = arr.splice(2, 2, 99, 100);

            Assert.Equal(2, deleted.length);
            Assert.Equal(3, deleted[0]);
            Assert.Equal(4, deleted[1]);

            Assert.Equal(5, arr.length);
            Assert.Equal(1, arr[0]);
            Assert.Equal(2, arr[1]);
            Assert.Equal(99, arr[2]);
            Assert.Equal(100, arr[3]);
            Assert.Equal(5, arr[4]);
        }

        [Fact]
        public void concat_MergesArrays()
        {
            var arr1 = new JSArray<int>(new[] { 1, 2 });
            var arr2 = new JSArray<int>(new[] { 3, 4 });
            var result = arr1.concat(arr2, 5);

            Assert.Equal(5, result.length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
            Assert.Equal(4, result[3]);
            Assert.Equal(5, result[4]);
        }

        [Fact]
        public void find_FindsFirstMatch()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.find((x, i, a) => x > 3);

            Assert.Equal(4, result);
        }

        [Fact]
        public void findIndex_FindsFirstMatchIndex()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.findIndex((x, i, a) => x > 3);

            Assert.Equal(3, result);
        }

        [Fact]
        public void findLast_FindsLastMatch()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.findLast((x, i, a) => x > 3);

            Assert.Equal(5, result);
        }

        [Fact]
        public void findLastIndex_FindsLastMatchIndex()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 4, 5 });
            var result = arr.findLastIndex((x, i, a) => x > 3);

            Assert.Equal(4, result);
        }

        [Fact]
        public void every_AllMatch_ReturnsTrue()
        {
            var arr = new JSArray<int>(new[] { 2, 4, 6 });
            var result = arr.every((x, i, a) => x % 2 == 0);

            Assert.True(result);
        }

        [Fact]
        public void every_NotAllMatch_ReturnsFalse()
        {
            var arr = new JSArray<int>(new[] { 2, 3, 6 });
            var result = arr.every((x, i, a) => x % 2 == 0);

            Assert.False(result);
        }

        [Fact]
        public void some_AnyMatch_ReturnsTrue()
        {
            var arr = new JSArray<int>(new[] { 1, 3, 4 });
            var result = arr.some((x, i, a) => x % 2 == 0);

            Assert.True(result);
        }

        [Fact]
        public void some_NoneMatch_ReturnsFalse()
        {
            var arr = new JSArray<int>(new[] { 1, 3, 5 });
            var result = arr.some((x, i, a) => x % 2 == 0);

            Assert.False(result);
        }

        [Fact]
        public void lastIndexOf_FindsLastOccurrence()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 2, 1 });
            var result = arr.lastIndexOf(2);

            Assert.Equal(3, result);
        }

        [Fact]
        public void lastIndexOf_WithFromIndex_SearchesFromPosition()
        {
            var arr = new JSArray<int>(new[] { 1, 2, 3, 2, 1 });
            var result = arr.lastIndexOf(2, 2);

            Assert.Equal(1, result);
        }

        [Fact]
        public void sort_WithoutCompare_SortsLexicographically()
        {
            var arr = new JSArray<int>(new[] { 3, 1, 4, 1, 5 });
            arr.sort();

            Assert.Equal(1, arr[0]);
            Assert.Equal(1, arr[1]);
            Assert.Equal(3, arr[2]);
            Assert.Equal(4, arr[3]);
            Assert.Equal(5, arr[4]);
        }

        [Fact]
        public void sort_WithCompare_SortsNumerically()
        {
            var arr = new JSArray<int>(new[] { 10, 2, 30, 4 });
            arr.sort((a, b) => a - b);

            Assert.Equal(2, arr[0]);
            Assert.Equal(4, arr[1]);
            Assert.Equal(10, arr[2]);
            Assert.Equal(30, arr[3]);
        }

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
            Assert.Null(arr.at(3));
            Assert.Null(arr.at(-4));
        }

        [Fact]
        public void StaticAt_ReturnsElementOrNullishCarrier()
        {
            var arr = new List<int> { 1, 2, 3 };
            Assert.Equal(2, Tsonic.CSharp.Js.Array.at(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.at(arr, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.at(arr, 3));
            Assert.Null(Tsonic.CSharp.Js.Array.at(arr, -4));
            Assert.Equal(2, Tsonic.CSharp.Js.Array.atValue(arr, 1));
            Assert.Equal(3, Tsonic.CSharp.Js.Array.atValue(arr, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, 3));
            Assert.Null(Tsonic.CSharp.Js.Array.atValue(arr, -4));
            var strings = new List<string> { "a", "b" };
            Assert.Equal("b", Tsonic.CSharp.Js.Array.atReference(strings, -1));
            Assert.Null(Tsonic.CSharp.Js.Array.atReference(strings, 2));
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
