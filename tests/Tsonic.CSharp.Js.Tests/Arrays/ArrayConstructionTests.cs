using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class ArrayConstructionTests
{
    [Fact]
    public void OrderedConstructionRetainsOneDestinationAndCopiesOnlyElements()
    {
        var value = new object();
        var source = new JSArray<object>(new[] { value });
        var result = new JSArray<object>();
        Assert.Same(result, result.AppendSequence(source));
        Assert.Same(result, result.AppendElement(value));
        Assert.NotSame(source, result);
        Assert.Same(value, result[0]);
        source[0] = new object();
        Assert.Same(value, result[0]);
        Assert.Same(value, result[1]);
        var exact = new JSArray<long>().AppendSequence(new[] { 9007199254740993L }).AppendElement(long.MaxValue);
        Assert.Equal(9007199254740993L, exact[0]);
        Assert.Equal(long.MaxValue, exact[1]);
    }

    [Fact]
    public void ConstructionAllocatesExactlyTheNativeListStorage()
    {
        var source = new JSArray<int>(new int[128]);
        GC.KeepAlive(new JSArray<int>().AppendSequence(source).AppendElement(7));
        var warm = new JSArray<int>();
        warm.AddRange(source);
        warm.Add(7);
        GC.KeepAlive(warm);
        var started = GC.GetAllocatedBytesForCurrentThread();
        var expected = new JSArray<int>();
        expected.AddRange(source);
        expected.Add(7);
        var nativeBytes = GC.GetAllocatedBytesForCurrentThread() - started;
        started = GC.GetAllocatedBytesForCurrentThread();
        var actual = new JSArray<int>().AppendSequence(source).AppendElement(7);
        var constructionBytes = GC.GetAllocatedBytesForCurrentThread() - started;
        GC.KeepAlive(expected);
        GC.KeepAlive(actual);
        Assert.Equal(nativeBytes, constructionBytes);
        Assert.Equal(expected, actual);
    }
}
