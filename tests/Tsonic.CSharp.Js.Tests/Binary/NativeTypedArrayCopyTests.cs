using System;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("Native allocation cost")]
public sealed class NativeTypedArrayCopyTests
{
    [Fact]
    public void SearchDoesNotAllocateForNativeOrUnrepresentableQueries()
    {
        var bytes = Uint8Array.From<double>(new double[] { 0, 1, 255 });
        var doubles = new Float64Array(new double[] { 9007199254740992, (double)ulong.MaxValue });
        var matches = 0;
        void Search()
        {
            if (bytes.includes((byte)255)) matches++;
            if (bytes.indexOf(ulong.MaxValue) != -1) matches++;
            if (doubles.includes(9007199254740993UL)) matches++;
            if (doubles.indexOf(ulong.MaxValue) != -1) matches++;
        }
        for (var count = 0; count < 1000; count++) Search();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var count = 0; count < 1000; count++) Search();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(2000, matches);
    }

    [Fact]
    public void OrdinaryNumericArraysPreserveExactNativeElementsWithoutTemporaryStorage()
    {
        ulong[] values = [9_007_199_254_740_993, ulong.MaxValue];
        var bytes = Uint8Array.From(values);
        Assert.Equal(new byte[] { 1, 255 }, bytes);
        Assert.Equal(new uint[] { 1, uint.MaxValue }, Uint32Array.From(values));
        Assert.Equal(new byte[] { 255, 255 }, Uint8ClampedArray.From(values));
        var list = new JSArray<ulong>(values);
        bytes.set(list);
        values[0] = 7;
        Assert.Equal((byte)1, bytes.Get(0));
        Assert.Throws<RangeError>(() => bytes.set(values, 1));
        Assert.Equal(new byte[] { 1, 255 }, bytes);
        for (var count = 0; count < 1000; count++) bytes.set(list);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var count = 0; count < 1000; count++)
        {
            bytes.set(values);
            bytes.set(list);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(new byte[] { 1, 255 }, bytes);
    }

    [Fact]
    public void EveryElementPairSupportsNativeConstructionAndSet()
    {
        var values = new[] { 0d, 1d, 127d };
        CheckSource(Int8Array.From<double>(values));
        CheckSource(Uint8Array.From<double>(values));
        CheckSource(Uint8ClampedArray.From<double>(values));
        CheckSource(Int16Array.From<double>(values));
        CheckSource(Uint16Array.From<double>(values));
        CheckSource(Int32Array.From<double>(values));
        CheckSource(Uint32Array.From<double>(values));
        CheckSource(Float32Array.From<double>(values));
        CheckSource(new Float64Array(values));
    }

    private static void CheckSource<TArray, TElement>(TypedArray<TArray, TElement> source)
        where TArray : TypedArray<TArray, TElement>
        where TElement : unmanaged, INumberBase<TElement>
    {
        var signedByteCopy = Int8Array.From(source);
        var unsignedByteCopy = Uint8Array.From(source);
        var clampedCopy = Uint8ClampedArray.From(source);
        var signedShortCopy = Int16Array.From(source);
        var unsignedShortCopy = Uint16Array.From(source);
        var signedWordCopy = Int32Array.From(source);
        var unsignedWordCopy = Uint32Array.From(source);
        var singleCopy = Float32Array.From(source);
        var wideCopy = Float64Array.From(source);
        var signedByte = new Int8Array(3); signedByte.set(source);
        var unsignedByte = new Uint8Array(3); unsignedByte.set(source);
        var clamped = new Uint8ClampedArray(3); clamped.set(source);
        var signedShort = new Int16Array(3); signedShort.set(source);
        var unsignedShort = new Uint16Array(3); unsignedShort.set(source);
        var signedWord = new Int32Array(3); signedWord.set(source);
        var unsignedWord = new Uint32Array(3); unsignedWord.set(source);
        var single = new Float32Array(3); single.set(source);
        var wide = new Float64Array(3); wide.set(source);
        source.Set(0, 7);
        AssertValues(signedByteCopy);
        AssertValues(unsignedByteCopy);
        AssertValues(clampedCopy);
        AssertValues(signedShortCopy);
        AssertValues(unsignedShortCopy);
        AssertValues(signedWordCopy);
        AssertValues(unsignedWordCopy);
        AssertValues(singleCopy);
        AssertValues(wideCopy);
        AssertValues(signedByte);
        AssertValues(unsignedByte);
        AssertValues(clamped);
        AssertValues(signedShort);
        AssertValues(unsignedShort);
        AssertValues(signedWord);
        AssertValues(unsignedWord);
        AssertValues(single);
        AssertValues(wide);
    }

    private static void AssertValues<T>(IArrayLike<T> copy) where T : INumberBase<T>
    {
        var expected = new[] { 0d, 1d, 127d };
        Assert.Equal(expected.Length, copy.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.True(copy.TryGet(index, out var value));
            Assert.Equal(T.CreateChecked(expected[index]), value);
        }
    }

    [Fact]
    public void TruncatingNarrowingAndExplicitClampingRemainDifferentOperations()
    {
        var source = Int16Array.From<double>(new[] { -1d, 256d });
        Assert.Equal(new byte[] { 255, 0 }, Uint8Array.From(source));
        var destination = new Uint8Array(2);
        destination.set(source);
        Assert.Equal(new byte[] { 255, 0 }, destination);
        Assert.Equal(new byte[] { 0, 255 }, Uint8ClampedArray.From(source));
        var fractions = new Float64Array(new[] { 0.5, 1.5, 254.5, double.NaN, double.PositiveInfinity });
        Assert.Equal(new byte[] { 0, 2, 254, 0, 255 }, Uint8ClampedArray.From(fractions));
        var wide = Uint32Array.From<double>(new[] { (double)uint.MaxValue });
        Assert.Equal((double)uint.MaxValue, Float64Array.From(wide)[0]);
        Assert.Equal(-1, Int32Array.From(wide)[0]);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(0, 8)]
    [InlineData(8, 0)]
    public void ConversionPreservesAliasingWithOverlappingAndDisjointViews(int sourceOffset, int destinationOffset)
    {
        var buffer = new ArrayBuffer(16);
        var source = new Int16Array(buffer, sourceOffset, 4);
        for (var index = 0; index < 4; index++) source.Set(index, index + 1);
        var target = new Uint8Array(buffer, destinationOffset, 4);
        target.set(source);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, target);
        Assert.Same(buffer, target.buffer);
        Assert.Same(buffer, source.buffer);
    }

    [Fact]
    public void SameElementCopyHandlesOverlapAndRejectsInvalidDestinationBeforeWriting()
    {
        var values = Uint8Array.From<double>(new[] { 1d, 2d, 3d, 4d });
        values.set(values.subarray(0, 3), 1);
        Assert.Equal(new byte[] { 1, 1, 2, 3 }, values);
        Assert.Throws<RangeError>(() => values.set(new Uint8Array(5)));
        Assert.Equal(new byte[] { 1, 1, 2, 3 }, values);
    }
}
