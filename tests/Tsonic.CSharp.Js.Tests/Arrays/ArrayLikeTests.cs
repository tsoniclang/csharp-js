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
        var typed = Uint8Array.From<double>(new double[] { 0, 0, 255 });
        Check(sparse);
        Check(typed);
        static void Check<T>(IArrayLike<T> source) where T : struct, System.Numerics.INumberBase<T>
        {
            Assert.Equal(3, source.Length);
            Assert.Equal(T.Zero, ArrayLike.ReadValue(source, 1));
            foreach (var index in new[] { -1.0, 0.5, 3, double.NaN, double.PositiveInfinity, double.NegativeInfinity, 2147483648 })
                Assert.Null(ArrayLike.ReadNumericProperty(source, index));
        }
        Assert.Equal(0, ArrayLike.ReadValue(sparse, 0));
        Assert.Equal((byte)0, ArrayLike.ReadValue(typed, 0));
        Assert.Equal(new[] { 0.0, 0.0, 0.0 }, ArrayLike.CopyDense(sparse).toArray());
    }

    [Fact]
    public void ProjectionRetainsAliasesAndExplicitCopiesOwnTheirStorage()
    {
        var bytes = Uint8Array.From<double>(new double[] { 1, 255 });
        var signed = Int16Array.From<double>(new double[] { -3, 32767 });
        var numbers = new JSArray<double>(new[] { 4.0, 5.0 });
        IArrayLike<byte> view = bytes;
        var byteCopy = ArrayLike.CopyDense(view);
        var signedCopy = ArrayLike.CopyDense<short>(signed);
        var numberCopy = ArrayLike.CopyDense<double>(numbers);
        bytes[0] = 9;
        numbers[0] = 8;
        Assert.Equal(9, ArrayLike.ReadValue(view, 0));
        Assert.Equal(1, byteCopy[0]);
        Assert.Equal(255, byteCopy[1]);
        Assert.Equal(4, numberCopy[0]);
        Assert.Equal(-3, signedCopy[0]);
        Assert.Equal(32767, signedCopy[1]);
        Assert.Equal(0, ArrayLike.CopyDense(new JSArray<double>()).length);
    }
}
