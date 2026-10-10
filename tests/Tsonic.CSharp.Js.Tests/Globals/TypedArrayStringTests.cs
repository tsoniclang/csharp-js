using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class TypedArrayStringTests
{
    [Fact]
    public void TypedArraysRetainAllNativeNumericWidthsAndOneAbsence() => TypedArrayStringChecks.CheckValues();

    [Fact]
    public void TypedNumericArrayWritingAllocatesOnlyItsNativeBuilderAndResult() => TypedArrayStringChecks.CheckCosts();
}
