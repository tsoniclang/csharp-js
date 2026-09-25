using System;
using Xunit;
using Js = Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeResultCarrierTests
{
    [Fact]
    public void BinaryWritesKeepExactNativeIntegerBits()
    {
        var view = new Js.DataView(new Js.ArrayBuffer(8));
        view.setUint32(0, 9007199254740993L, true);
        Assert.Equal(1u, view.getUint32(0, true));
        view.setInt32(0, ulong.MaxValue, false);
        Assert.Equal(-1, view.getInt32(0, false));
        view.setUint16(0, UInt128.MaxValue, true);
        Assert.Equal(ushort.MaxValue, view.getUint16(0, true));
        view.setInt16(0, Int128.MinValue + 1, false);
        Assert.Equal((short)1, view.getInt16(0, false));
        view.setInt8(0, long.MinValue + 255);
        Assert.Equal((sbyte)-1, view.getInt8(0));
        view.setUint8(0, ulong.MaxValue);
        Assert.Equal(byte.MaxValue, view.getUint8(0));
        view.setUint32(0, -1.5);
        Assert.Equal(uint.MaxValue, view.getUint32(0));
    }

    [Fact]
    public void TypedWritesAndFillKeepNativeIntegersAndClamping()
    {
        var words = new Js.Uint32Array(2);
        Assert.Equal(9007199254740993L, words.Set(0, 9007199254740993L));
        Assert.Equal(1u, words.Get(0));
        words.fill(UInt128.MaxValue, 1);
        Assert.Equal(uint.MaxValue, words.Get(1));
        words.fill(-1.5);
        Assert.Equal(uint.MaxValue, words.Get(0));
        var clamped = new Js.Uint8ClampedArray(2);
        clamped.Set(0, UInt128.MaxValue);
        clamped.Set(1, Int128.MinValue);
        Assert.Equal(byte.MaxValue, clamped.Get(0));
        Assert.Equal((byte)0, clamped.Get(1));
        clamped.fill(2.5);
        Assert.Equal((byte)2, clamped.Get(0));
        Assert.Equal(17L, words.Set(-1, 17L));
        Assert.Equal(uint.MaxValue, words.Get(0));
    }

    [Fact]
    public void TypedUpdatesRetainNativeArithmeticAndClampedStorage()
    {
        var clamped = new Js.Uint8ClampedArray(1);
        clamped.Set(0, 255);
        Assert.Equal(256, clamped.Update<int, int>(0, true, true));
        Assert.Equal((byte)255, clamped.Get(0));
        Assert.Equal(255, clamped.Update<int, int>(0, false, false));
        Assert.Equal((byte)254, clamped.Get(0));
        var words = new Js.Uint32Array(1);
        words.Set(0, uint.MaxValue);
        Assert.Equal(uint.MaxValue, words.Update<uint, int>(0, true, false));
        Assert.Equal(0u, words.Get(0));
        Assert.Equal(1u, words.Update<uint, int>(-1, true, true));
        Assert.Equal(0u, words.Get(0));
    }

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
