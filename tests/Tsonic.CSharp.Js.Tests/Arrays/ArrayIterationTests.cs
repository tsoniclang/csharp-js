using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
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
        public void reduce_NoInitialValue_PreservesFullCallbackIdentityOrderAndSeed()
        {
            var values = new JSArray<int>(new[] { 2, 3, 4 });
            var visited = new List<int>();
            var result = values.reduce((accumulator, value, index, array) =>
            {
                Assert.Same(values, array);
                visited.Add(index);
                return accumulator * 10 + value;
            });

            Assert.Equal(234, result);
            Assert.Equal(new[] { 1, 2 }, visited);
        }

        [Fact]
        public void reduce_NoInitialValue_SingleElementDoesNotInvokeCallback()
        {
            var values = new JSArray<int>(new[] { 7 });
            var calls = 0;
            Assert.Equal(7, values.reduce((accumulator, value, index, array) =>
            {
                calls++;
                return accumulator + value + index + array.length;
            }));
            Assert.Equal(0, calls);
        }

        [Fact]
        public void reduce_NoInitialValue_EmptyArrayKeepsExactTypeError()
        {
            var values = new JSArray<int>();
            var calls = 0;
            var compact = Assert.Throws<TypeError>(() => values.reduce((accumulator, value) =>
            {
                calls++;
                return accumulator + value;
            }));
            var complete = Assert.Throws<TypeError>(() => values.reduce((accumulator, value, index, array) =>
            {
                calls++;
                return accumulator + value + index + array.length;
            }));

            Assert.Equal("Reduce of empty array with no initial value", compact.Message);
            Assert.Equal(compact.Message, complete.Message);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void reduce_NoInitialValue_PreservesCallbackErrorIdentity()
        {
            var values = new JSArray<int>(new[] { 1, 2 });
            var expected = new TypeError("original callback error");
            var actual = Assert.Throws<TypeError>(() => values.reduce((_accumulator, _value, _index, _array) => throw expected));
            Assert.Same(expected, actual);
        }

        [Fact]
        public void reduce_NoInitialValue_ObservesReplacementButNeverAppendedElements()
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            var visited = new List<int>();
            var result = values.reduce((accumulator, value, index, array) =>
            {
                visited.Add(index);
                if (index == 1)
                {
                    array[2] = 4;
                    array.push(100);
                }
                return accumulator + value;
            });

            Assert.Equal(7, result);
            Assert.Equal(new[] { 1, 2 }, visited);
            Assert.Equal(4, values.length);
            Assert.Equal(100, values[3]);
        }

        [Fact]
        public void reduce_InitializedVariantsCaptureTheirOriginalIterationLength()
        {
            var compact = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, compact.reduce((accumulator, value) =>
            {
                if (value == 1) compact.push(100);
                return accumulator + value;
            }, 0));
            var indexed = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, indexed.reduce((accumulator, value, index) =>
            {
                if (index == 0) indexed.push(100);
                return accumulator + value;
            }, 0));
            var complete = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, complete.reduce((accumulator, value, index, array) =>
            {
                Assert.Same(complete, array);
                if (index == 0) array.push(100);
                return accumulator + value;
            }, 0));
        }

        [Fact]
        public void reduce_NoInitialValue_UsesNoPerCallAdapterOrAllocation()
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            System.Func<int, int, int, JSArray<int>, int> callback = static (accumulator, value, _index, _array) => accumulator + value;
            Assert.Equal(6, values.reduce(callback));
            var before = System.GC.GetAllocatedBytesForCurrentThread();
            var result = values.reduce(callback);
            var allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.Equal(6, result);
            Assert.Equal(0, allocated);
        }

        [Fact]
        public void reduceRight_WithInitialValue_ReducesFromRight()
        {
            var arr = new JSArray<string>(new[] { "a", "b", "c" });
            var result = arr.reduceRight((acc, x, i, a) => acc + x, "");

            Assert.Equal("cba", result);
        }

        [Fact]
        public void reduce_NoInitialValue_IndexedCallbackKeepsItsFirstSeed()
        {
            var values = new JSArray<int>(new[] { 2, 3, 4 });
            var visited = new List<int>();
            var result = values.reduce((accumulator, value, index) =>
            {
                visited.Add(index);
                return accumulator * 10 + value;
            });
            Assert.Equal(234, result);
            Assert.Equal(new[] { 1, 2 }, visited);
        }

        [Fact]
        public void reduceRight_NoInitialValue_CompactCallbackKeepsItsLastSeed()
        {
            var values = new JSArray<int>(new[] { 2, 3, 4 });
            var calls = 0;
            var result = values.reduceRight((accumulator, value) =>
            {
                calls++;
                return accumulator * 10 + value;
            });
            Assert.Equal(432, result);
            Assert.Equal(2, calls);
        }

        [Fact]
        public void reduceRight_NoInitialValue_IndexedCallbackKeepsReverseOrder()
        {
            var values = new JSArray<int>(new[] { 2, 3, 4 });
            var visited = new List<int>();
            var result = values.reduceRight((accumulator, value, index) =>
            {
                visited.Add(index);
                return accumulator * 10 + value;
            });
            Assert.Equal(432, result);
            Assert.Equal(new[] { 1, 0 }, visited);
        }

        [Fact]
        public void reduceRight_NoInitialValue_FullCallbackKeepsIdentityAndOriginalBounds()
        {
            var values = new JSArray<int>(new[] { 2, 3, 4 });
            var visited = new List<int>();
            var result = values.reduceRight((accumulator, value, index, array) =>
            {
                Assert.Same(values, array);
                visited.Add(index);
                if (index == 1)
                {
                    array[0] = 5;
                    array.push(100);
                }
                return accumulator * 10 + value;
            });
            Assert.Equal(435, result);
            Assert.Equal(new[] { 1, 0 }, visited);
            Assert.Equal(100, values[3]);
        }

        [Fact]
        public void reduceRight_InitializedVariantsKeepTheirOriginalBounds()
        {
            var compact = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, compact.reduceRight((accumulator, value) =>
            {
                if (value == 2) compact.push(100);
                return accumulator + value;
            }, 0));
            var indexed = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, indexed.reduceRight((accumulator, value, index) =>
            {
                if (index == 1) indexed.push(100);
                return accumulator + value;
            }, 0));
            var complete = new JSArray<int>(new[] { 1, 2 });
            Assert.Equal(3, complete.reduceRight((accumulator, value, index, array) =>
            {
                Assert.Same(complete, array);
                if (index == 1) array.push(100);
                return accumulator + value;
            }, 0));
        }

        [Fact]
        public void reduce_NoInitialValue_AllNewAritiesKeepEmptyTypeErrorAndSingleSeed()
        {
            var empty = new JSArray<int>();
            var calls = 0;
            var left = Assert.Throws<TypeError>(() => empty.reduce((accumulator, value, index) =>
            {
                calls++;
                return accumulator + value + index;
            }));
            var compact = Assert.Throws<TypeError>(() => empty.reduceRight((accumulator, value) =>
            {
                calls++;
                return accumulator + value;
            }));
            var indexed = Assert.Throws<TypeError>(() => empty.reduceRight((accumulator, value, index) =>
            {
                calls++;
                return accumulator + value + index;
            }));
            var complete = Assert.Throws<TypeError>(() => empty.reduceRight((accumulator, value, index, array) =>
            {
                calls++;
                return accumulator + value + index + array.length;
            }));
            Assert.Equal("Reduce of empty array with no initial value", left.Message);
            Assert.Equal(left.Message, compact.Message);
            Assert.Equal(left.Message, indexed.Message);
            Assert.Equal(left.Message, complete.Message);
            Assert.Equal(0, calls);
            var single = new JSArray<int>(new[] { 7 });
            Assert.Equal(7, single.reduce((accumulator, value, index) => { calls++; return accumulator + value + index; }));
            Assert.Equal(7, single.reduceRight((accumulator, value) => { calls++; return accumulator + value; }));
            Assert.Equal(7, single.reduceRight((accumulator, value, index) => { calls++; return accumulator + value + index; }));
            Assert.Equal(7, single.reduceRight((accumulator, value, index, array) => { calls++; return accumulator + value + index + array.length; }));
            Assert.Equal(0, calls);
        }

        [Fact]
        public void reduceRight_NoInitialValue_PreservesEveryCallbackErrorIdentity()
        {
            var values = new JSArray<int>(new[] { 1, 2 });
            var expected = new TypeError("original right callback");
            Assert.Same(expected, Assert.Throws<TypeError>(() => values.reduceRight((_accumulator, _value) => throw expected)));
            Assert.Same(expected, Assert.Throws<TypeError>(() => values.reduceRight((_accumulator, _value, _index) => throw expected)));
            Assert.Same(expected, Assert.Throws<TypeError>(() => values.reduceRight((_accumulator, _value, _index, _array) => throw expected)));
            Assert.Same(expected, Assert.Throws<TypeError>(() => values.reduce((_accumulator, _value, _index) => throw expected)));
        }

        [Fact]
        public void reduce_NoInitialValue_PreservesDenseDefaultsAndSkipsRemovedElements()
        {
            var defaults = new JSArray<int>(3);
            var calls = 0;
            Assert.Equal(0, defaults.reduce((accumulator, value, _index) => { calls++; return accumulator + value; }));
            Assert.Equal(2, calls);
            calls = 0;
            Assert.Equal(0, defaults.reduceRight((accumulator, value, _index) => { calls++; return accumulator + value; }));
            Assert.Equal(2, calls);
            Assert.True(defaults.hasIndex(0));
            Assert.True(defaults.hasIndex(2));
            Assert.Throws<TypeError>(() => defaults.deleteAt(1));
            var left = new JSArray<int>(new[] { 1, 2, 3 });
            var leftIndexes = new List<int>();
            Assert.Equal(3, left.reduce((accumulator, value, index, array) =>
            {
                leftIndexes.Add(index);
                array.splice(0, array.length);
                return accumulator + value;
            }));
            Assert.Equal(new[] { 1 }, leftIndexes);
            var right = new JSArray<int>(new[] { 1, 2, 3 });
            var rightIndexes = new List<int>();
            Assert.Equal(5, right.reduceRight((accumulator, value, index, array) =>
            {
                rightIndexes.Add(index);
                array.splice(0, array.length);
                return accumulator + value;
            }));
            Assert.Equal(new[] { 1 }, rightIndexes);
        }

        [Fact]
        public void reduce_NoInitialValue_AllNativeAritiesUseNoPerCallAllocation()
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            System.Func<int, int, int> compact = static (accumulator, value) => accumulator + value;
            System.Func<int, int, int, int> indexed = static (accumulator, value, _index) => accumulator + value;
            System.Func<int, int, int, JSArray<int>, int> complete = static (accumulator, value, _index, _array) => accumulator + value;
            Assert.Equal(6, values.reduce(compact));
            Assert.Equal(6, values.reduce(indexed));
            Assert.Equal(6, values.reduce(complete));
            Assert.Equal(6, values.reduceRight(compact));
            Assert.Equal(6, values.reduceRight(indexed));
            Assert.Equal(6, values.reduceRight(complete));
            var before = System.GC.GetAllocatedBytesForCurrentThread();
            var leftCompact = values.reduce(compact);
            var leftIndexed = values.reduce(indexed);
            var leftComplete = values.reduce(complete);
            var rightCompact = values.reduceRight(compact);
            var rightIndexed = values.reduceRight(indexed);
            var rightComplete = values.reduceRight(complete);
            var allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(6, leftCompact);
            Assert.Equal(6, leftIndexed);
            Assert.Equal(6, leftComplete);
            Assert.Equal(6, rightCompact);
            Assert.Equal(6, rightIndexed);
            Assert.Equal(6, rightComplete);
            Assert.Equal(0, allocated);
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
    }
}
