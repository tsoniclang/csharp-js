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
    }
}
