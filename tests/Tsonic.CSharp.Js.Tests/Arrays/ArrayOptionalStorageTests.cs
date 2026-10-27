using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class ArrayOptionalStorageTests
{
    [Fact]
    public void GenericFind_DistinguishesAFoundZeroFromNativeAbsence()
    {
        var array = new JSArray<int>(new[] { 0, 7, 0 });
        IReadOnlyList<int> list = new[] { 0, 7, 0 };
        Assert.Equal(0, Array.find<int, int?, ValueOptionalStorage<int>>(array, static (value, index, owner) => value == 0));
        Assert.Equal(0, Array.findLast<int, int?, ValueOptionalStorage<int>>(array, static (value, index, owner) => value == 0));
        Assert.Equal(0, Array.find<int, int?, ValueOptionalStorage<int>>(list, static (value, index, owner) => value == 0));
        Assert.Equal(0, Array.findLast<int, int?, ValueOptionalStorage<int>>(list, static (value, index, owner) => value == 0));
        Assert.Null(Array.find<int, int?, ValueOptionalStorage<int>>(array, static (value, index, owner) => false));
        Assert.Null(Array.findLast<int, int?, ValueOptionalStorage<int>>(array, static (value, index, owner) => false));
        Assert.Null(Array.find<int, int?, ValueOptionalStorage<int>>(list, static (value, index, owner) => false));
        Assert.Null(Array.findLast<int, int?, ValueOptionalStorage<int>>(list, static (value, index, owner) => false));
    }

    [Fact]
    public void GenericFind_PreservesReferenceIdentityAndCallbackOrder()
    {
        var first = new object();
        var last = new object();
        var array = new JSArray<object>(new[] { first, last });
        var positions = new List<int>();
        var selected = Array.findLast<object, object?, ReferenceOptionalStorage<object>>(array, (value, index, owner) =>
        {
            Assert.Same(array, owner);
            positions.Add(index);
            return ReferenceEquals(value, first);
        });
        Assert.Same(first, selected);
        Assert.Equal(new[] { 1, 0 }, positions);
        Assert.Same(last, array.find<object?, ReferenceOptionalStorage<object>>(value => ReferenceEquals(value, last)));
        Assert.Null(array.findLast<object?, ReferenceOptionalStorage<object>>(static value => false));
    }

    [Fact]
    public void GenericFind_UsesOnlyTheRequiredNativeStorageAndNoHeapBoxing()
    {
        var array = new JSArray<long>(new[] { 9007199254740993L });
        Func<long, int, JSArray<long>, bool> match = static (value, index, owner) => true;
        for (var iteration = 0; iteration < 4096; iteration++)
            _ = Array.find<long, long?, ValueOptionalStorage<long>>(array, match);
        var start = GC.GetAllocatedBytesForCurrentThread();
        long sum = 0;
        for (var iteration = 0; iteration < 8192; iteration++)
            sum = Array.find<long, long?, ValueOptionalStorage<long>>(array, match) ?? 0;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        Assert.Equal(9007199254740993L, sum);
        Assert.Equal(0L, allocated);
        Assert.All(typeof(JSArray<int>).GetMethods().Where(method => method.Name is "find" or "findLast"),
            method => Assert.Equal(2, method.GetGenericArguments().Length));
        Assert.All(typeof(Array).GetMethods().Where(method => method.Name is "find" or "findLast"),
            method => Assert.Equal(3, method.GetGenericArguments().Length));
    }
}
