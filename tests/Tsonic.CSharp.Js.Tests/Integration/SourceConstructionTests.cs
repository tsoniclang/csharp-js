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
        var second = EmptyObject.Freeze(new EmptyObject());
        Assert.False(EmptyObject.IsFrozen(alias));
        Assert.Same(alias, EmptyObject.Freeze(first));
        Assert.True(EmptyObject.IsFrozen(alias));
        Assert.NotSame(first, second);
        var identities = new HashSet<EmptyObject> { first };
        Assert.Contains(alias, identities);
        Assert.DoesNotContain(second, identities);
        var values = JSArrayStatics.withLength<EmptyObject>(3d);
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
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(4294967296d)]
    public void LengthValidationPrecedesIntegerNarrowing(double length)
    {
        Assert.Throws<RangeError>(() => JSArrayStatics.withLength<EmptyObject>(length));
        Assert.Throws<RangeError>(() => new JSArray<EmptyObject>(length));
    }
}
