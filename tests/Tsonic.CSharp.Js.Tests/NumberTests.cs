using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class NumberTests
    {
        [Fact]
        public void Constants_MatchJavaScriptNumberValues()
        {
            Assert.Equal(double.MaxValue, Number.MAX_VALUE);
            Assert.Equal(double.Epsilon, Number.MIN_VALUE);
            Assert.Equal(9007199254740991d, Number.MAX_SAFE_INTEGER);
            Assert.Equal(-9007199254740991d, Number.MIN_SAFE_INTEGER);
            Assert.Equal(double.PositiveInfinity, Number.POSITIVE_INFINITY);
            Assert.Equal(double.NegativeInfinity, Number.NEGATIVE_INFINITY);
            Assert.True(double.IsNaN(Number.NaN));
            Assert.Equal(2.220446049250313e-16, Number.EPSILON);
        }

        [Fact]
        public void toString_UsesInvariantFormatting()
        {
            Assert.Equal("42", 42d.toString());
            Assert.Equal("3.5", 3.5d.toString());
            Assert.Equal("0", (-0.0d).toString());
            Assert.Equal("NaN", double.NaN.toString());
            Assert.Equal("Infinity", double.PositiveInfinity.toString());
            Assert.Equal("-Infinity", double.NegativeInfinity.toString());
            Assert.Equal("7", 7.toString());
            Assert.Equal("9", 9L.toString());
            Assert.Equal("11", ((int?)11).toString());
            Assert.Equal("13", ((long?)13).toString());
            Assert.Equal(string.Empty, ((int?)null).toString());
        }

        [Fact]
        public void toString_WithRadix_FormatsIntegralValues()
        {
            Assert.Equal("101010", 42.toString(2));
            Assert.Equal("52", 42.toString(8));
            Assert.Equal("2a", 42.toString(16));
            Assert.Equal("21i3v9", 123456789L.toString(36));
            Assert.Equal("-2a", (-42).toString(16));
            Assert.Equal("0", 0.toString(2));
            Assert.Equal("ff", ((int?)255).toString(16));
            Assert.Equal(string.Empty, ((int?)null).toString(16));
        }

        [Fact]
        public void toString_WithRadix_RejectsInvalidRadix()
        {
            Assert.Throws<RangeError>(() => 1.toString(1));
            Assert.Throws<RangeError>(() => 1.toString(37));
        }

        [Fact]
        public void FormattingMethods_UseJavaScriptNumberShapes()
        {
            Assert.Equal("12.35", 12.345d.toFixed(2));
            Assert.Equal("12", 12.345d.toFixed());
            Assert.Equal("1.23e+4", 12345d.toExponential(2));
            Assert.Equal("1e+4", 12345d.toPrecision(1));
            Assert.Equal("12345", 12345d.toLocaleString());
            Assert.Equal("NaN", double.NaN.toFixed(2));
            Assert.Equal("Infinity", double.PositiveInfinity.toExponential(2));
        }

        [Fact]
        public void FormattingMethods_RejectInvalidPrecision()
        {
            Assert.Throws<RangeError>(() => 1d.toFixed(-1));
            Assert.Throws<RangeError>(() => 1d.toFixed(101));
            Assert.Throws<RangeError>(() => 1d.toExponential(-1));
            Assert.Throws<RangeError>(() => 1d.toPrecision(0));
            Assert.Throws<RangeError>(() => 1d.toPrecision(101));
        }

        [Fact]
        public void valueOf_ReturnsOriginalValue()
        {
            Assert.Equal(42d, 42d.valueOf());
            Assert.Equal(-1.25d, (-1.25d).valueOf());
            Assert.Equal(42, 42.valueOf());
            Assert.Equal(128L, 128L.valueOf());
            Assert.Equal((int?)64, ((int?)64).valueOf());
            Assert.Equal((long?)256, ((long?)256).valueOf());
        }

        [Fact]
        public void Static_Number_Predicates_AcceptIntegralReceivers()
        {
            Assert.False(Number.isNaN(7));
            Assert.False(Number.isNaN((int?)7));
            Assert.False(Number.isNaN(9L));
            Assert.False(Number.isNaN((long?)9));

            Assert.True(Number.isFinite(7));
            Assert.True(Number.isFinite((int?)7));
            Assert.True(Number.isFinite(9L));
            Assert.True(Number.isFinite((long?)9));
            Assert.False(Number.isFinite((int?)null));
            Assert.False(Number.isFinite((long?)null));

            Assert.True(Number.isInteger(7));
            Assert.True(Number.isInteger((int?)7));
            Assert.True(Number.isInteger(9L));
            Assert.True(Number.isInteger((long?)9));
            Assert.False(Number.isInteger((int?)null));
            Assert.False(Number.isInteger((long?)null));

            Assert.True(Number.isSafeInteger(7));
            Assert.True(Number.isSafeInteger((int?)7));
            Assert.True(Number.isSafeInteger(9L));
            Assert.True(Number.isSafeInteger((long?)9));
            Assert.False(Number.isSafeInteger((long?)null));
        }
    }
}
