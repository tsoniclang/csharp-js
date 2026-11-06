using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NumericComparatorTests
{
    private static void NativeResult<TResult>(Func<long, long, TResult> compare) where TResult : INumber<TResult>
    {
        long[] input = [9007199254740995L, 9007199254740993L, 9007199254740994L];
        long[] expected = [9007199254740993L, 9007199254740994L, 9007199254740995L];
        var values = new JSArray<long>(input);
        var copy = values.toSorted(compare);
        Assert.Equal(input, values.ToArray());
        Assert.Equal(expected, copy.ToArray());
        Assert.Same(values, values.sort(compare));
        Assert.Equal(expected, values.ToArray());
        var list = new List<long>(input);
        Assert.Same(list, Array.sort(list, compare));
        Assert.Equal(expected, list);
        Assert.Equal(expected, Array.toSorted(new List<long>(input), compare));
    }

    [Fact]
    public void NumericResultsPreserveEveryNativePayloadWithoutFloatingConversion()
    {
        NativeResult<int>((left, right) => left.CompareTo(right));
        NativeResult<long>((left, right) => left - right);
        NativeResult<double>((left, right) => left.CompareTo(right));
        NativeResult<float>((left, right) => left.CompareTo(right));
        NativeResult<Half>((left, right) => (Half)left.CompareTo(right));
        NativeResult<decimal>((left, right) => left.CompareTo(right));
        NativeResult<Int128>((left, right) => left.CompareTo(right));
        NativeResult<BigInteger>((left, right) => new BigInteger(left) - right);
        var equal = new JSArray<long>(new long[] { 3, 1, 2 });
        equal.sort((left, right) => 0UL);
        Assert.Equal(new long[] { 3, 1, 2 }, equal.ToArray());
    }

    [Fact]
    public void StableComparisonsKeepEqualOrderAndCallbackAppendedTail()
    {
        var values = new JSArray<(int rank, string name)>(new[] { (2, "a"), (1, "b"), (2, "c"), (1, "d") });
        var appended = false;
        values.sort((left, right) =>
        {
            if (!appended) { appended = true; values.push((3, "tail")); }
            return (long)left.rank - right.rank;
        });
        Assert.Equal(new[] { "b", "d", "a", "c", "tail" }, values.Select(value => value.name));
        var original = Enumerable.Range(0, 40).ToArray();
        var equal = new JSArray<int>(original);
        equal.sort((left, right) => double.NaN);
        Assert.Equal(original, equal.ToArray());
        equal.sort((left, right) => -0.0);
        Assert.Equal(original, equal.ToArray());
    }

    [Fact]
    public void TypedArraysAndListHelpersUseNativeNumericSignIncludingNaN()
    {
        var words = Uint32Array.From<uint>(new uint[] { uint.MaxValue, 7, 0 });
        words.sort((left, right) => (long)left - right);
        Assert.Equal(new uint[] { 0, 7, uint.MaxValue }, words);
        words.sort((left, right) => double.NaN);
        Assert.Equal(new uint[] { 0, 7, uint.MaxValue }, words);
        var list = new List<int> { 3, 1, 2 };
        Array.sort(list, (left, right) => double.NaN);
        Assert.Equal(new[] { 3, 1, 2 }, list);
        var defaults = new JSArray<int>(new[] { 2, 11, 1 });
        defaults.sort();
        Assert.Equal(new[] { 1, 11, 2 }, defaults.ToArray());
    }

    [Fact]
    public void ComparatorEndpointsAreGenericRatherThanFixedDoubleDelegates()
    {
        foreach (var type in new[] { typeof(JSArray<long>), typeof(Uint32Array), typeof(Array) })
        foreach (var method in type.GetMethods().Where(method => method.Name is "sort" or "toSorted"))
        {
            var callbacks = method.GetParameters().Where(parameter => parameter.ParameterType.IsGenericType
                && parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<,,>)).ToArray();
            foreach (var callback in callbacks)
            {
                Assert.True(method.IsGenericMethodDefinition);
                Assert.True(callback.ParameterType.GetGenericArguments()[2].IsGenericParameter);
            }
        }
    }
}
