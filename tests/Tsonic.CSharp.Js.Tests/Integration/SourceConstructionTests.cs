using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class SourceConstructionTests
{
    [Fact]
    public void InitializedLengthAndFillPreserveEmptyObjectReferences()
    {
        var first = new EmptyObject();
        var alias = first;
        var second = new EmptyObject();
        Assert.Same(alias, first);
        Assert.NotSame(first, second);
        var identities = new HashSet<EmptyObject> { first };
        Assert.Contains(alias, identities);
        Assert.DoesNotContain(second, identities);
        var values = JSArrayStatics.withLength<EmptyObject>(3);
        var visits = 0;
        values.forEach(value => { Assert.Null(value); visits++; });
        Assert.Equal(3, visits);
        Assert.Equal(3, values.length);
        values.fill(first);
        Assert.Same(alias, values[0]);
        Assert.Same(alias, values[2]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NativeLengthRejectsNegativeCounts(int length)
    {
        Assert.Throws<RangeError>(() => JSArrayStatics.withLength<EmptyObject>(length));
        Assert.Throws<RangeError>(() => new JSArray<EmptyObject>(length));
    }

    [Fact]
    public void ArrayLengthHasOnlyTheNativeIntegerApi()
    {
        Assert.Null(typeof(JSArray<EmptyObject>).GetConstructor(new[] { typeof(double) }));
        var factory = typeof(JSArrayStatics).GetMethod("withLength");
        Assert.NotNull(factory);
        Assert.Equal(typeof(int), Assert.Single(factory.GetParameters()).ParameterType);
        Assert.DoesNotContain(typeof(JSArray<EmptyObject>).GetMethods(), method =>
            method.Name == "setLength" && Assert.Single(method.GetParameters()).ParameterType == typeof(double));
    }
}
