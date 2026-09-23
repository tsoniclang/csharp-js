using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeTypedArrayCopyTests
{
    [Fact]
    public void EveryElementPairSupportsNativeConstructionAndSet()
    {
        var values = new[] { 0d, 1d, 127d };
        CheckSource(new Int8Array(values));
        CheckSource(new Uint8Array(values));
        CheckSource(new Uint8ClampedArray(values));
        CheckSource(new Int16Array(values));
        CheckSource(new Uint16Array(values));
        CheckSource(new Int32Array(values));
        CheckSource(new Uint32Array(values));
        CheckSource(new Float32Array(values));
        CheckSource(new Float64Array(values));
    }

    private static void CheckSource<TArray, TElement>(TypedArray<TArray, TElement> source)
        where TArray : TypedArray<TArray, TElement>
        where TElement : unmanaged, INumberBase<TElement>
    {
        IEnumerable<double>[] copies = {
            Int8Array.From(source), Uint8Array.From(source), Uint8ClampedArray.From(source),
            Int16Array.From(source), Uint16Array.From(source), Int32Array.From(source),
            Uint32Array.From(source), Float32Array.From(source), Float64Array.From(source),
        };
        var signedByte = new Int8Array(3); signedByte.set(source);
        var unsignedByte = new Uint8Array(3); unsignedByte.set(source);
        var clamped = new Uint8ClampedArray(3); clamped.set(source);
        var signedShort = new Int16Array(3); signedShort.set(source);
        var unsignedShort = new Uint16Array(3); unsignedShort.set(source);
        var signedWord = new Int32Array(3); signedWord.set(source);
        var unsignedWord = new Uint32Array(3); unsignedWord.set(source);
        var single = new Float32Array(3); single.set(source);
        var wide = new Float64Array(3); wide.set(source);
        IEnumerable<double>[] assigned = {
            signedByte, unsignedByte, clamped, signedShort, unsignedShort,
            signedWord, unsignedWord, single, wide,
        };
        source[0] = 7;
        foreach (var copy in copies) Assert.Equal(new[] { 0d, 1d, 127d }, copy);
        foreach (var copy in assigned) Assert.Equal(new[] { 0d, 1d, 127d }, copy);
    }

    [Fact]
    public void CheckedNarrowingAndExplicitClampingRemainDifferentOperations()
    {
        var source = new Int16Array(new[] { -1d, 256d });
        Assert.Throws<OverflowException>(() => Uint8Array.From(source));
        Assert.Throws<OverflowException>(() => new Uint8Array(2).set(source));
        Assert.Equal(new[] { 0d, 255d }, Uint8ClampedArray.From(source));
        var fractions = new Float64Array(new[] { 0.5, 1.5, 254.5, double.NaN, double.PositiveInfinity });
        Assert.Equal(new[] { 0d, 2d, 254d, 0d, 255d }, Uint8ClampedArray.From(fractions));
        var wide = new Uint32Array(new[] { (double)uint.MaxValue });
        Assert.Equal((double)uint.MaxValue, Float64Array.From(wide)[0]);
        Assert.Throws<OverflowException>(() => Int32Array.From(wide));
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(0, 8)]
    [InlineData(8, 0)]
    public void ConversionPreservesAliasingWithOverlappingAndDisjointViews(int sourceOffset, int destinationOffset)
    {
        var buffer = new ArrayBuffer(16);
        var source = new Int16Array(buffer, sourceOffset, 4);
        for (var index = 0; index < 4; index++) source[index] = index + 1;
        var target = new Uint8Array(buffer, destinationOffset, 4);
        target.set(source);
        Assert.Equal(new[] { 1d, 2d, 3d, 4d }, target);
        Assert.Same(buffer, target.buffer);
        Assert.Same(buffer, source.buffer);
    }

    [Fact]
    public void SameElementCopyHandlesOverlapAndRejectsInvalidDestinationBeforeWriting()
    {
        var values = new Uint8Array(new[] { 1d, 2d, 3d, 4d });
        values.set(values.subarray(0, 3), 1);
        Assert.Equal(new[] { 1d, 1d, 2d, 3d }, values);
        Assert.Throws<RangeError>(() => values.set(new Uint8Array(5)));
        Assert.Equal(new[] { 1d, 1d, 2d, 3d }, values);
    }
}
