using System;
using System.Linq;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class NativePerformanceTests
{
    [Fact]
    public void DenseLengthConstructionInitializesEveryElementAndRejectsHoles()
    {
        var values = new JSArray<int>(4);
        Assert.Equal(new[] { 0, 0, 0, 0 }, values.ToArray());
        Assert.All(Enumerable.Range(0, 4), index => Assert.True(values.hasIndex(index)));
        values[4] = 5;
        Assert.Throws<RangeError>(() => values[6] = 7);
        Assert.Throws<TypeError>(() => values.deleteAt(0));
        Assert.Throws<RangeError>(() => values.setLength(8));
        Assert.Equal(new[] { 0, 0, 0, 0, 5 }, values.ToArray());
        values.setLength(2);
        Assert.Equal(new[] { 0, 0 }, values.ToArray());
    }

    [Fact]
    public void StableSortRetainsEqualOrderAndCallbackAppends()
    {
        var values = new JSArray<(int rank, string name)>(new[] { (2, "a"), (1, "b"), (2, "c"), (1, "d") });
        var appended = false;
        values.sort((left, right) =>
        {
            if (!appended) { appended = true; values.push((3, "tail")); }
            return left.rank - right.rank;
        });
        Assert.Equal(new[] { "b", "d", "a", "c", "tail" }, values.Select(value => value.name));
    }

    [Fact]
    public void SteadyStatePrimitiveMapLookupAndOverwriteDoNotBoxOrAllocate()
    {
        var values = new Map<double, int>();
        for (var index = 0; index < 2000; index++) values.set(index, index);
        values.set(double.NaN, 3);
        values.set(-0.0, 9);
        for (var index = 0; index < 2000; index++) values.tryGet(index, out _);
        var warmup = System.Diagnostics.Stopwatch.StartNew();
        long allocated;
        var found = 0;
        do
        {
            found = 0;
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 2000; index++)
            {
                values.set(index, index);
                if (values.tryGet(index, out var value)) found += value;
            }
            allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            if (allocated != 0) System.Threading.Thread.Sleep(10);
        }
        while (allocated != 0 && warmup.Elapsed < TimeSpan.FromSeconds(5));
        Assert.Equal(1999000, found);
        Assert.Equal(0, allocated);
        Assert.True(values.has(double.NaN));
        Assert.Equal(0, values.get(-0.0));
    }

    [Fact]
    public void MapIteratorsObserveDeletionClearAndInsertion()
    {
        var values = new Map<int, string>();
        values.set(1, "a").set(2, "b");
        var seen = new System.Collections.Generic.List<string>();
        values.forEach((value, key) =>
        {
            seen.Add(value);
            if (key == 1) { values.delete(1); values.delete(2); values.set(3, "c"); }
            else if (key == 3) { values.clear(); values.set(4, "d"); }
        });
        Assert.Equal(new[] { "a", "c", "d" }, seen);
        Assert.Equal(new[] { 4 }, values.keys());
    }

    [Fact]
    public void TypedArrayBulkCopyPreservesOverlappingViews()
    {
        var values = new Uint8Array(new double[] { 1, 2, 3, 4, 5 });
        values.set(values.subarray(0, 4), 1);
        Assert.Equal(new double[] { 1, 1, 2, 3, 4 }, values.ToArray());
        var selected = values.subarray(1, 4);
        selected.fill(7);
        Assert.Equal(new double[] { 1, 7, 7, 7, 4 }, values.ToArray());
        var copy = selected.slice(0, 2);
        selected[0] = 9;
        Assert.Equal((byte)7, copy[0]);
    }

    [Fact]
    public void BooleanRegExpPreservesStateAndOptionalSplitCaptures()
    {
        var expression = new RegExp("a", "g");
        Assert.True(expression.testNative("aba"));
        Assert.Equal(1, expression.lastIndex);
        Assert.True(expression.testNative("aba"));
        Assert.Equal(3, expression.lastIndex);
        Assert.False(expression.testNative("aba"));
        Assert.Equal(0, expression.lastIndex);
        var parts = new RegExp("(a)?b").split("xbz");
        Assert.Equal(new string?[] { "x", null, "z" }, parts.ToArray());
        Assert.True(parts.hasIndex(1));
    }
}
