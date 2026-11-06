using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class ArrayLikePresenceTests
{
    [Fact]
    public void HasIndexUsesTheOriginalOrdinaryAndTypedViews()
    {
        var ordinary = new JSArray<double>(new double[] { 0, 2, 3 });
        var typed = Uint8Array.From<double>(new double[] { 0, 2, 3 });
        IArrayLike<double> ordinaryView = ordinary;
        IArrayLike<byte> typedView = typed;

        Assert.True(ArrayLike.HasIndex(0, ordinaryView));
        Assert.True(ArrayLike.HasIndex(0, typedView));
        Assert.True(ArrayLike.HasIndex(2, ordinaryView));
        Assert.True(ArrayLike.HasIndex(2, typedView));
        Assert.False(ArrayLike.HasIndex(3, ordinaryView));
        Assert.False(ArrayLike.HasIndex(3, typedView));
        ordinary[2] = 7;
        typed[2] = 9;
        Assert.Equal(7, ArrayLike.ReadValue(ordinaryView, 2));
        Assert.Equal((byte?)9, ArrayLike.ReadValue(typedView, 2));
        Assert.Equal(3, ordinaryView.Length);
        Assert.Equal(3, typedView.Length);
    }

    [Fact]
    public void HasIndexPreservesHolesAndNativeNumericKeyBoundaries()
    {
        var ordinary = new JSArray<double>(new double[] { 0, 2, 3 });
        IArrayLike<double> view = ordinary;
        var refusal = Assert.Throws<TypeError>(() => ordinary.deleteAt(1));
        Assert.Equal("Deleting a dense array element would create a hole; use splice", refusal.Message);
        Assert.True(ArrayLike.HasIndex(0, view));
        Assert.True(ArrayLike.HasIndex(1, view));
        Assert.True(ordinary.deleteAt(99));
        Assert.False(ArrayLike.HasIndex(99, view));
        ordinary[-1d] = 17;
        ordinary[double.NaN] = 19;
        Assert.True(ArrayLike.HasIndex(-1, view));
        Assert.True(ArrayLike.HasIndex(double.NaN, view));
        Assert.False(ArrayLike.HasIndex(0.5, view));
        Assert.False(ArrayLike.HasIndex(double.PositiveInfinity, view));
        Assert.False(ArrayLike.HasIndex((double)long.MaxValue, view));
        Assert.False(ArrayLike.HasIndex((double)ulong.MaxValue, view));
        Assert.Equal(3, view.Length);
        IArrayLike<byte> typed = Uint8Array.From<double>(new double[] { 0, 2, 3 });
        Assert.False(ArrayLike.HasIndex(-1, typed));
        Assert.False(ArrayLike.HasIndex(double.NaN, typed));
        Assert.False(ArrayLike.HasIndex(0.5, typed));
        Assert.False(ArrayLike.HasIndex(double.PositiveInfinity, typed));
    }

    [Fact]
    public void HasIndexAllocatesExactlyTheNativePresenceQuery()
    {
        IArrayLike<double> ordinary = new JSArray<double>(new double[] { 0, 2, 3 });
        IArrayLike<byte> typed = Uint8Array.From<double>(new double[] { 0, 2, 3 });
        Check(ordinary);
        Check(typed);
        static void Check<T>(IArrayLike<T> source)
        {
            for (var index = 0; index < 16384; index++)
            {
                _ = Selected(source, index & 1);
                _ = Native(source, index & 1);
            }
            var selected = 0;
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 16384; index++)
                if (Selected(source, index & 1)) selected++;
            var selectedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            var native = 0;
            before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 16384; index++)
                if (Native(source, index & 1)) native++;
            var nativeBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(16384, selected);
            Assert.Equal(native, selected);
            Assert.Equal(0, nativeBytes);
            Assert.Equal(nativeBytes, selectedBytes);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Selected<T>(IArrayLike<T> source, double index) => ArrayLike.HasIndex(index, source);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Native<T>(IArrayLike<T> source, double index) => source.TryGetNumericProperty(index, out _);
}
