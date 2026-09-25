using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class TypedArrayMemoryTests
{
    [Fact]
    public void Uint8ArrayMemoryPreservesTheExactAliasedView()
    {
        var source = new Uint8Array(new double[] { 9, 0, 255, 42, 9 });
        var view = new Uint8Array(source.buffer, 1, 3);
        var memory = view.AsMemory();
        source[3] = 43;
        Assert.Equal(new byte[] { 0, 255, 43 }, memory.ToArray());
        Assert.Empty(new Uint8Array(source.buffer, 5, 0).AsMemory().ToArray());
    }

    [Fact]
    public void Uint8ArrayMemoryDoesNotAllocateOrCopy()
    {
        var source = new Uint8Array(128);
        for (var index = 0; index < 100; index++) _ = source.AsMemory();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var length = 0;
        for (var index = 0; index < 1000; index++) length += source.AsMemory().Length;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(128_000, length);
        Assert.Equal(0, allocated);
    }
}
