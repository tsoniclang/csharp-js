using System;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class ArrayLikeTests
{
    [Fact]
    public void ReadsDistinguishInvalidIndexesFromInitializedZero()
    {
        var sparse = new JSArray<double>(3);
        sparse[1] = 0;
        IArrayLike<double>[] arrays = [sparse, new Uint8Array(new double[] { 0, 0, 255 })];
        foreach (var source in arrays)
        {
            Assert.Equal(3, source.Length);
            Assert.Equal(0, ArrayLike.ReadNumber(source, 1));
            foreach (var index in new[] { -1.0, 0.5, 3, double.NaN, double.PositiveInfinity, double.NegativeInfinity, 2147483648 })
                Assert.Null(ArrayLike.ReadNumber(source, index));
        }
        Assert.Equal(0, ArrayLike.ReadNumber(sparse, 0));
        Assert.Equal(0, ArrayLike.ReadNumber(arrays[1], 0));
        Assert.Equal(new[] { 0.0, 0.0, 0.0 }, ArrayLike.CopyDense(sparse).toArray());
    }

    [Fact]
    public void ProjectionRetainsAliasesAndExplicitCopiesOwnTheirStorage()
    {
        var bytes = new Uint8Array(new double[] { 1, 255 });
        var signed = new Int16Array(new double[] { -3, 32767 });
        var numbers = new JSArray<double>(new[] { 4.0, 5.0 });
        IArrayLike<double> view = bytes;
        var byteCopy = ArrayLike.CopyDense(view);
        var signedCopy = ArrayLike.CopyDense<double>(signed);
        var numberCopy = ArrayLike.CopyDense<double>(numbers);
        bytes[0] = 9;
        numbers[0] = 8;
        Assert.Equal(9, ArrayLike.ReadNumber(view, 0));
        Assert.Equal(1, byteCopy[0]);
        Assert.Equal(255, byteCopy[1]);
        Assert.Equal(4, numberCopy[0]);
        Assert.Equal(-3, signedCopy[0]);
        Assert.Equal(32767, signedCopy[1]);
        Assert.Equal(0, ArrayLike.CopyDense(new JSArray<double>()).length);
    }

    [Fact]
    public void NativeSignedMembershipDoesNotRoundAdjacentNegativeProperties()
    {
        var values = new JSArray<double>(new[] { 2d });
        values[-9007199254740992d] = 7;
        values[(double)long.MinValue] = 9;
        Assert.True(ArrayLike.HasIndex(-9007199254740992L, values));
        Assert.False(ArrayLike.HasIndex(-9007199254740993L, values));
        Assert.False(ArrayLike.HasIndex(-9007199254740991L, values));
        Assert.True(ArrayLike.HasIndex(long.MinValue, values));
        Assert.False(ArrayLike.HasIndex(long.MinValue + 1, values));
        Assert.Equal(1, values.length);
        Assert.Equal(7, values[-9007199254740992d]);
        Assert.Equal(9, values[(double)long.MinValue]);
    }

    [Fact]
    public void NativeIntegerBoundsRetainInitializedAbsenceAndDeletedSlots()
    {
        var values = new JSArray<double?>(2);
        Assert.True(ArrayLike.HasIndex(0L, values));
        Assert.True(ArrayLike.HasIndex(1UL, values));
        Assert.Null(values[0]);
        Assert.False(ArrayLike.HasIndex(2L, values));
        Assert.False(ArrayLike.HasIndex(long.MaxValue, values));
        Assert.False(ArrayLike.HasIndex(ulong.MaxValue, values));
        Assert.False(ArrayLike.HasIndex(uint.MaxValue, values));
        Assert.False(ArrayLike.HasIndex(9007199254740993UL, values));
        Assert.Throws<Tsonic.CSharp.Runtime.TypeError>(() => values.deleteAt(0));
        Assert.True(ArrayLike.HasIndex(0L, values));
        Assert.True(ArrayLike.HasIndex(1UL, values));
        values[-1d] = null;
        Assert.True(ArrayLike.HasIndex(-1L, values));
        Assert.True(values.deleteAt(-1d));
        Assert.False(ArrayLike.HasIndex(-1L, values));
    }

    [Fact]
    public void NativeInitializedMembershipDoesNotAddAllocation()
    {
        var values = new JSArray<double>(new[] { 0d, 1d });
        for (var index = 0; index < 10_000; index++)
        {
            if (!ArrayLike.HasIndex((long)(index & 1), values)) throw new InvalidOperationException();
            if (ArrayLike.HasIndex(long.MinValue + 1, values)) throw new InvalidOperationException();
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100_000; index++)
        {
            if (!ArrayLike.HasIndex((long)(index & 1), values)) throw new InvalidOperationException();
            if (ArrayLike.HasIndex(long.MinValue + 1, values)) throw new InvalidOperationException();
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }
}
