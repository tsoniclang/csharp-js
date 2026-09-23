using Xunit;
using Js = Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeResultCarrierTests
{
    [Fact]
    public void BinaryResultsRetainNativeWidths()
    {
        var buffer = new Js.ArrayBuffer(8);
        int count = buffer.byteLength;
        var view = new Js.DataView(buffer);
        view.setUint32(0, uint.MaxValue, true);
        uint word = view.getUint32(0, true);
        view.setFloat32(4, 1.5, true);
        float single = view.getFloat32(4, true);
        Assert.Equal(8, count);
        Assert.Equal(uint.MaxValue, word);
        Assert.Equal(1.5f, single);
        Assert.Equal(typeof(int), typeof(Js.Uint8Array).GetProperty("length")!.PropertyType);
        Assert.Equal(typeof(int), typeof(Js.Uint8Array).GetProperty("byteOffset")!.PropertyType);
    }

    [Fact]
    public void TypedReadsAndSortingRetainTheNativeElement()
    {
        var words = new Js.Uint32Array(new double[] { uint.MaxValue, 7, 0 });
        uint first = words.Get(0);
        uint? last = words.at(-1);
        Assert.Equal(uint.MaxValue, first);
        Assert.Equal(0u, last);
        words.sort((left, right) => left.CompareTo(right));
        Assert.Equal(new uint[] { 0, 7, uint.MaxValue }, words);
        words.sort((left, right) => double.NaN);
        Assert.Equal(new uint[] { 0, 7, uint.MaxValue }, words);
    }

    [Fact]
    public void NativeReadsDoNotChangeClampedWritesOrAliasing()
    {
        var clamped = new Js.Uint8ClampedArray(2);
        clamped[0] = 300;
        clamped[1] = 1.5;
        byte first = clamped.Get(0);
        Assert.Equal((byte)255, first);
        Assert.Equal((byte)2, clamped.Get(1));
        var alias = clamped.subarray(0);
        alias[0] -= 400;
        Assert.Equal((byte)0, clamped.Get(0));
        Assert.Equal((byte)0, clamped.Get(double.NaN));
    }
}
