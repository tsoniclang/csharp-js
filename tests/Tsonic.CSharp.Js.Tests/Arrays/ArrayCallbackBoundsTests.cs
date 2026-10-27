using System;
using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        public static IEnumerable<object[]> CallbackMutationCases()
        {
            foreach (var method in new[] { "filter", "forEach", "every", "some", "find", "findIndex", "findLast", "findLastIndex", "reduceRight" })
            for (int arity = 1; arity <= 3; arity++)
            foreach (var mutation in new[] { "append", "shrink" })
                yield return new object[] { method, arity, mutation };
        }

        [Theory]
        [MemberData(nameof(CallbackMutationCases))]
        public void callbackLoops_SnapshotLengthAndCheckCurrentPresence(string method, int arity, string mutation)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            var observed = new List<int>();
            bool descending = method == "findLast" || method == "findLastIndex" || method == "reduceRight";
            void Observe(int value, int? index, JSArray<int>? receiver)
            {
                if (index.HasValue) Assert.Equal(value - 1, index.Value);
                if (receiver != null) Assert.Same(values, receiver);
                observed.Add(value);
                if (observed.Count == 1)
                {
                    if (mutation == "append") values.push(99);
                    else
                    {
                        values.pop();
                        values.pop();
                    }
                }
            }
            bool Predicate(int value, int? index, JSArray<int>? receiver)
            {
                Observe(value, index, receiver);
                return method == "filter" || method == "every";
            }
            Func<int, bool> unary = value => Predicate(value, null, null);
            Func<int, int, bool> indexed = (value, index) => Predicate(value, index, null);
            Func<int, int, JSArray<int>, bool> complete = (value, index, receiver) => Predicate(value, index, receiver);
            switch (method)
            {
                case "filter":
                    var filtered = arity == 1 ? values.filter(unary) : arity == 2 ? values.filter(indexed) : values.filter(complete);
                    Assert.Equal(observed, filtered.ToArray());
                    break;
                case "forEach":
                    if (arity == 1) values.forEach(value => Observe(value, null, null));
                    else if (arity == 2) values.forEach((value, index) => Observe(value, index, null));
                    else values.forEach((value, index, receiver) => Observe(value, index, receiver));
                    break;
                case "every":
                    Assert.True(arity == 1 ? values.every(unary) : arity == 2 ? values.every(indexed) : values.every(complete));
                    break;
                case "some":
                    Assert.False(arity == 1 ? values.some(unary) : arity == 2 ? values.some(indexed) : values.some(complete));
                    break;
                case "find":
                    Assert.Null(arity == 1 ? values.find<int?, ValueOptionalStorage<int>>(unary)
                        : arity == 2 ? values.find<int?, ValueOptionalStorage<int>>(indexed)
                        : values.find<int?, ValueOptionalStorage<int>>(complete));
                    break;
                case "findIndex":
                    Assert.Equal(-1, arity == 1 ? values.findIndex(unary) : arity == 2 ? values.findIndex(indexed) : values.findIndex(complete));
                    break;
                case "findLast":
                    Assert.Null(arity == 1 ? values.findLast<int?, ValueOptionalStorage<int>>(unary)
                        : arity == 2 ? values.findLast<int?, ValueOptionalStorage<int>>(indexed)
                        : values.findLast<int?, ValueOptionalStorage<int>>(complete));
                    break;
                case "findLastIndex":
                    Assert.Equal(-1, arity == 1 ? values.findLastIndex(unary) : arity == 2 ? values.findLastIndex(indexed) : values.findLastIndex(complete));
                    break;
                case "reduceRight":
                    int Fold(int accumulator, int value, int? index, JSArray<int>? receiver)
                    {
                        Observe(value, index, receiver);
                        return accumulator + value;
                    }
                    var reduced = arity == 1 ? values.reduceRight((accumulator, value) => Fold(accumulator, value, null, null), 0)
                        : arity == 2 ? values.reduceRight((accumulator, value, index) => Fold(accumulator, value, index, null), 0)
                        : values.reduceRight((accumulator, value, index, receiver) => Fold(accumulator, value, index, receiver), 0);
                    Assert.Equal(observed.Sum(), reduced);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(method));
            }
            var expected = mutation == "append" ? descending ? new[] { 3, 2, 1 } : new[] { 1, 2, 3 }
                : descending ? new[] { 3, 1 } : new[] { 1 };
            Assert.Equal(expected, observed);
            Assert.Equal(mutation == "append" ? 4 : 1, values.length);
        }

        public static IEnumerable<object[]> CallbackMatchedValueCases()
        {
            foreach (var method in new[] { "filter", "find", "findLast" })
            for (int arity = 1; arity <= 3; arity++)
                yield return new object[] { method, arity };
        }

        [Theory]
        [MemberData(nameof(CallbackMatchedValueCases))]
        public void callbackMatches_ReturnTheValueReadBeforeTheCallback(string method, int arity)
        {
            var values = new JSArray<int>(new[] { 1, 2, 3 });
            var calls = 0;
            bool Predicate(int value, int? index, JSArray<int>? receiver)
            {
                Assert.Equal(method == "findLast" ? 3 : 1, value);
                if (index.HasValue) Assert.Equal(value - 1, index.Value);
                if (receiver != null) Assert.Same(values, receiver);
                calls++;
                values.Clear();
                return true;
            }
            Func<int, bool> unary = value => Predicate(value, null, null);
            Func<int, int, bool> indexed = (value, index) => Predicate(value, index, null);
            Func<int, int, JSArray<int>, bool> complete = (value, index, receiver) => Predicate(value, index, receiver);
            if (method == "filter")
            {
                var filtered = arity == 1 ? values.filter(unary) : arity == 2 ? values.filter(indexed) : values.filter(complete);
                Assert.Equal(new[] { 1 }, filtered.ToArray());
            }
            else if (method == "find")
                Assert.Equal(1, arity == 1 ? values.find<int?, ValueOptionalStorage<int>>(unary)
                    : arity == 2 ? values.find<int?, ValueOptionalStorage<int>>(indexed)
                    : values.find<int?, ValueOptionalStorage<int>>(complete));
            else
                Assert.Equal(3, arity == 1 ? values.findLast<int?, ValueOptionalStorage<int>>(unary)
                    : arity == 2 ? values.findLast<int?, ValueOptionalStorage<int>>(indexed)
                    : values.findLast<int?, ValueOptionalStorage<int>>(complete));
            Assert.Equal(1, calls);
            Assert.Equal(0, values.length);
        }
    }
}
