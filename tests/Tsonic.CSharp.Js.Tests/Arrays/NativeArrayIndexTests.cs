using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeArrayIndexTests
{
    [Fact]
    public void NativeIntegerMembershipUsesTheOriginalOrdinaryAndTypedViews()
    {
        var original = new JSArray<double>(new double[] { 0, 7, 9 });
        var typed = new Uint8Array(new double[] { 0, 7, 9 });
        foreach (var view in new IArrayLike<double>[] { original, typed })
        {
            Assert.True(ArrayLike.HasIndex((char)1, view));
            Assert.True(ArrayLike.HasIndex((sbyte)1, view));
            Assert.True(ArrayLike.HasIndex((byte)1, view));
            Assert.True(ArrayLike.HasIndex((short)1, view));
            Assert.True(ArrayLike.HasIndex((ushort)1, view));
            Assert.True(ArrayLike.HasIndex(1, view));
            Assert.True(ArrayLike.HasIndex(1U, view));
            Assert.True(ArrayLike.HasIndex(1L, view));
            Assert.True(ArrayLike.HasIndex(1UL, view));
            Assert.True(ArrayLike.HasIndex((nint)1, view));
            Assert.True(ArrayLike.HasIndex((nuint)1, view));
            Assert.True(ArrayLike.HasIndex((Int128)1, view));
            Assert.True(ArrayLike.HasIndex((UInt128)1, view));
            Assert.False(ArrayLike.HasIndex(uint.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(long.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(ulong.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(nint.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(nuint.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(Int128.MaxValue, view));
            Assert.False(ArrayLike.HasIndex(UInt128.MaxValue, view));
        }
        original[1] = 17;
        typed[1] = 19;
        Assert.Equal(17, ArrayLike.ReadNumber(original, 1));
        Assert.Equal(19, ArrayLike.ReadNumber(typed, 1));
        var refusal = Assert.Throws<TypeError>(() => original.deleteAt(1));
        Assert.Equal("Deleting a dense array element would create a hole; use splice", refusal.Message);
        Assert.True(ArrayLike.HasIndex(1L, original));
        Assert.True(original.deleteAt(99));
        Assert.False(ArrayLike.HasIndex(99L, original));
        Assert.True(ArrayLike.HasIndex(0UL, original));
        Assert.Equal(3, original.length);
        Assert.Equal(3, typed.length);
    }

    [Fact]
    public void WideNegativeMembershipNeverRoundsIntoAnotherNativeProperty()
    {
        var original = new JSArray<double>(new double[] { 0, 7 });
        IArrayLike<double> view = original;
        original[-1d] = 11;
        original[-9007199254740992d] = 13;
        original[(double)Int128.MinValue] = 17;
        Assert.True(ArrayLike.HasIndex(-1L, view));
        Assert.True(ArrayLike.HasIndex((nint)(-1), view));
        Assert.True(ArrayLike.HasIndex((Int128)(-1), view));
        Assert.True(ArrayLike.HasIndex(-9007199254740992L, view));
        Assert.False(ArrayLike.HasIndex(-9007199254740993L, view));
        Assert.True(ArrayLike.HasIndex(Int128.MinValue, view));
        Assert.False(ArrayLike.HasIndex(Int128.MinValue + 1, view));
        IArrayLike<double> typed = new Uint8Array(new double[] { 0, 7 });
        Assert.False(ArrayLike.HasIndex(-1L, typed));
        Assert.False(ArrayLike.HasIndex(Int128.MinValue, typed));
        Assert.Equal(2, original.length);
        Assert.Equal(11, original[-1d]);
        Assert.Equal(13, original[-9007199254740992d]);
        Assert.Equal(17, original[(double)Int128.MinValue]);
    }

    [Fact]
    public void NativeIntegerPresenceAllocatesExactlyTheHandwrittenBoundedRead()
    {
        foreach (var source in new IArrayLike<double>[] {
            new JSArray<double>(new double[] { 0, 7 }), new Uint8Array(new double[] { 0, 7 }) })
        {
            for (var index = 0; index < 16384; index++)
            {
                _ = Selected(source, index & 1);
                _ = Native(source, index & 1);
            }
            var selectedCount = 0;
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 16384; index++) if (Selected(source, index & 1)) selectedCount++;
            var selectedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            var nativeCount = 0;
            before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 16384; index++) if (Native(source, index & 1)) nativeCount++;
            var nativeBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(16384, selectedCount);
            Assert.Equal(nativeCount, selectedCount);
            Assert.Equal(0, nativeBytes);
            Assert.Equal(nativeBytes, selectedBytes);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Selected(IArrayLike<double> source, long index) => ArrayLike.HasIndex(index, source);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Native(IArrayLike<double> source, long index) =>
        index >= 0 && index < source.Length && source.TryGet((int)index, out _);
}
