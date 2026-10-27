using System;
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
        public void reduce_NoInitialFullCallback_PreservesDenseSeedIndicesAndReceiver()
        {
            var values = new JSArray<int>(6);
            values[1] = 4;
            values[3] = 7;
            values[5] = 9;
            Assert.Equal(0, values[0]);
            Assert.Equal(0, values[2]);
            Assert.Equal(0, values[4]);
            var indices = new List<int>();
            var result = values.reduce((accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                indices.Add(index);
                return accumulator + value;
            });
            Assert.Equal(20, result);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, indices);
        }

        [Fact]
        public void reduce_NoInitialFullCallback_SingleElementDoesNotInvokeCallback()
        {
            var values = new JSArray<int>(new[] { 13 });
            var calls = 0;
            var result = values.reduce((accumulator, value, index, receiver) =>
            {
                calls++;
                return accumulator + value;
            });
            Assert.Equal(13, result);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void reduce_NoInitialOverloads_EmptyThrowsNativeTypeError()
        {
            var values = new JSArray<int>();
            var direct = Assert.Throws<TypeError>(() => values.reduce((accumulator, value) => accumulator + value));
            var complete = Assert.Throws<TypeError>(() => values.reduce((accumulator, value, index, receiver) => accumulator + value));
            Assert.Equal("Reduce of empty array with no initial value", direct.Message);
            Assert.Equal(direct.Message, complete.Message);
        }

        [Fact]
        public void reduce_NoInitialOverloads_NativeDefaultsArePresentRatherThanHoles()
        {
            var values = new JSArray<int>(4);
            var directCalls = 0;
            var completeCalls = 0;
            Assert.Equal(0, values.reduce((accumulator, value) =>
            {
                directCalls++;
                Assert.Equal(0, value);
                return accumulator + value;
            }));
            Assert.Equal(0, values.reduce((accumulator, value, index, receiver) =>
            {
                completeCalls++;
                Assert.Same(values, receiver);
                Assert.Equal(completeCalls, index);
                Assert.True(values.hasIndex(index));
                Assert.Equal(0, value);
                return accumulator + value;
            }));
            Assert.Equal(3, directCalls);
            Assert.Equal(3, completeCalls);
            var failure = Assert.Throws<TypeError>(() => values.deleteAt(1));
            Assert.Equal("Deleting a dense array element would create a hole; use splice", failure.Message);
            Assert.Equal(4, values.length);
            Assert.True(values.hasIndex(1));
        }

        [Fact]
        public void reduce_NoInitialFullCallback_PreservesThrownExceptionIdentity()
        {
            var values = new JSArray<int>(new[] { 1, 2 });
            var failure = new InvalidOperationException("callback failure");
            var actual = Assert.Throws<InvalidOperationException>(() => values.reduce((accumulator, value, index, receiver) => throw failure));
            Assert.Same(failure, actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void reduce_NoInitialOverloads_IgnoreAppendedElements(bool fullCallback)
        {
            var values = new JSArray<int>(new[] { 1, 2 });
            var calls = 0;
            int Fold(int accumulator, int value)
            {
                calls++;
                if (calls == 1) values.push(99);
                return accumulator + value;
            }
            var result = fullCallback ? values.reduce((accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                Assert.Equal(1, index);
                return Fold(accumulator, value);
            }) : values.reduce(Fold);
            Assert.Equal(3, result);
            Assert.Equal(1, calls);
            Assert.Equal(3, values.length);
            Assert.Equal(99, values[2]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void reduce_NoInitialOverloads_ReadCurrentValuesWithinOriginalLength(bool fullCallback)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            var calls = 0;
            int Fold(int accumulator, int value)
            {
                calls++;
                if (calls == 1) values[2] = 7;
                return accumulator + value;
            }
            var result = fullCallback ? values.reduce((accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                Assert.Equal(calls + 1, index);
                return Fold(accumulator, value);
            }) : values.reduce(Fold);
            Assert.Equal(10, result);
            Assert.Equal(2, calls);
            Assert.Equal(7, values[2]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void reduce_NoInitialOverloads_SkipRemovedIndicesWithinOriginalLength(bool fullCallback)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3, 4 });
            var calls = 0;
            int Fold(int accumulator, int value)
            {
                calls++;
                values.pop();
                values.pop();
                return accumulator + value;
            }
            var result = fullCallback ? values.reduce((accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                Assert.Equal(1, index);
                return Fold(accumulator, value);
            }) : values.reduce(Fold);
            Assert.Equal(3, result);
            Assert.Equal(1, calls);
            Assert.Equal(2, values.length);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void reduce_NoInitialOverloads_ObserveDenseRegrowthWithinOriginalLength(bool fullCallback)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3, 4 });
            var calls = 0;
            int Fold(int accumulator, int value)
            {
                calls++;
                if (calls == 1)
                {
                    values.pop();
                    values.pop();
                    values.push(0, 9);
                }
                return accumulator + value;
            }
            var result = fullCallback ? values.reduce((accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                Assert.Equal(calls + 1, index);
                return Fold(accumulator, value);
            }) : values.reduce(Fold);
            Assert.Equal(12, result);
            Assert.Equal(3, calls);
            Assert.Equal(4, values.length);
            Assert.Equal(0, values[2]);
            Assert.Equal(9, values[3]);
        }

        [Theory]
        [InlineData(2, "append")]
        [InlineData(3, "append")]
        [InlineData(4, "append")]
        [InlineData(2, "overwrite")]
        [InlineData(3, "overwrite")]
        [InlineData(4, "overwrite")]
        [InlineData(2, "shrink")]
        [InlineData(3, "shrink")]
        [InlineData(4, "shrink")]
        public void reduce_InitialOverloads_SnapshotLengthAndReadCurrentValues(int arity, string mutation)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            var calls = 0;
            Func<int, int, int, JSArray<int>, int> callback = (accumulator, value, index, receiver) =>
            {
                Assert.Same(values, receiver);
                Assert.Equal(calls, index);
                calls++;
                if (calls == 1)
                {
                    if (mutation == "append") values.push(99);
                    else if (mutation == "overwrite") values[1] = 7;
                    else
                    {
                        values.pop();
                        values.pop();
                    }
                }
                return accumulator + value;
            };
            var result = arity switch
            {
                2 => values.reduce((accumulator, value) => callback(accumulator, value, calls, values), 0),
                3 => values.reduce((accumulator, value, index) => callback(accumulator, value, index, values), 0),
                4 => values.reduce(callback, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(arity))
            };
            Assert.Equal(mutation == "append" ? 6 : mutation == "overwrite" ? 11 : 1, result);
            Assert.Equal(mutation == "shrink" ? 1 : 3, calls);
            Assert.Equal(mutation == "append" ? 4 : mutation == "overwrite" ? 3 : 1, values.length);
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
            var result = arr.find<int?, ValueOptionalStorage<int>>((x, i, a) => x > 3);

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
            var result = arr.findLast<int?, ValueOptionalStorage<int>>((x, i, a) => x > 3);

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
