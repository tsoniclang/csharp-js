using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeArrayLikeIndexTests
{
    [Fact]
    public void NativeReadsPresenceAndCopiesNeverEnterTheNumericPropertyPath()
    {
        var source = new NativeIndexView();
        Assert.Equal(9007199254740993L, ArrayLike.ReadValue(source, 0));
        Assert.True(ArrayLike.HasIndex(1, source));
        Assert.Equal(new[] { 9007199254740993L, 0L }, ArrayLike.CopyDense(source).toArray());
        Assert.Null(ArrayLike.ReadValue(source, int.MaxValue));
        Assert.Equal(5, source.NativeReads);
        Assert.NotNull(typeof(IArrayLike<long>).GetMethod("TryGet", new[] { typeof(int), typeof(long).MakeByRefType() }));
        Assert.Null(typeof(IArrayLike<long>).GetMethod("TryGet", new[] { typeof(double), typeof(long).MakeByRefType() }));
        var native = Assert.Single(typeof(ArrayLike).GetMethods(), method => method.Name == "ReadValue");
        var numeric = Assert.Single(typeof(ArrayLike).GetMethods(), method => method.Name == "ReadNumericProperty");
        Assert.Equal(typeof(int), native.GetParameters()[1].ParameterType);
        Assert.Equal(typeof(double), numeric.GetParameters()[1].ParameterType);
    }

    [Fact]
    public void NumericPropertiesKeepTheirExactKeysWithoutBecomingNativeIndexes()
    {
        var values = new JSArray<long>(new[] { 0L });
        values[-1] = 7;
        values[1.5] = 8;
        values[double.NaN] = 9;
        values[double.PositiveInfinity] = 10;
        IArrayLike<long> source = values;
        Assert.True(source.TryGet(0, out var zero));
        Assert.Equal(0L, zero);
        Assert.False(source.TryGet(-1, out _));
        Assert.Equal(7L, ArrayLike.ReadValue(source, -1));
        Assert.Equal(8L, ArrayLike.ReadNumericProperty(source, 1.5));
        Assert.Equal(9L, ArrayLike.ReadNumericProperty(source, double.NaN));
        Assert.Equal(10L, ArrayLike.ReadNumericProperty(source, double.PositiveInfinity));
        Assert.True(ArrayLike.HasIndex(-1, source));
        Assert.True(ArrayLike.HasIndex(double.NaN, source));
        IArrayLike<byte> typed = new Uint8Array(1);
        Assert.True(typed.TryGet(0, out var byteZero));
        Assert.Equal((byte)0, byteZero);
        Assert.Null(ArrayLike.ReadValue(typed, 1));
        Assert.Null(ArrayLike.ReadNumericProperty(typed, 1.5));
        Assert.Null(ArrayLike.ReadNumericProperty(typed, double.NaN));
        Assert.Null(ArrayLike.ReadNumericProperty(typed, double.PositiveInfinity));
    }

    private sealed class NativeIndexView : IArrayLike<long>
    {
        public int Length => 2;
        public int NativeReads { get; private set; }

        public bool TryGet(int index, out long value)
        {
            NativeReads++;
            value = index == 0 ? 9007199254740993L : 0L;
            return index >= 0 && index < Length;
        }

        public bool TryGetNumericProperty(double key, out long value) =>
            throw new InvalidOperationException("A native integer index entered the JS numeric-property path.");
    }
}
