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
}
