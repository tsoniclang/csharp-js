using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("Native allocation cost")]
public sealed class TypedArrayStringTests
{
    [Fact]
    public void TypedArraysRetainAllNativeNumericWidthsAndOneAbsence() => TypedArrayStringChecks.CheckValues();

    [Fact]
    public void TypedNumericArrayWritingAllocatesOnlyItsNativeBuilderAndResult() => TypedArrayStringChecks.CheckCosts();
}
