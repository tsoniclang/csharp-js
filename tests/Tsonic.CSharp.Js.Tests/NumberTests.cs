using System;
using System.Globalization;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class NumberTests
    {
        [Fact]
        public void Constants_DescribeTheBinary64Carrier()
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
            Assert.Equal((-0.0d).ToString(CultureInfo.InvariantCulture), (-0.0d).toString());
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

        [Theory]
        [InlineData(1e-7)]
        [InlineData(-1e-7)]
        [InlineData(1e-6)]
        [InlineData(-1.234e-6)]
        [InlineData(1e20)]
        [InlineData(-1.2345678901234568e20)]
        [InlineData(1e21)]
        [InlineData(-1e21)]
        [InlineData(double.Epsilon)]
        [InlineData(double.MaxValue)]
        public void toString_UsesNativeInvariantNotation(double value)
        {
            Assert.Equal(value.ToString(CultureInfo.InvariantCulture), value.toString());
        }

        [Fact]
        public void toString_WithRadix_RejectsInvalidRadix()
        {
            Assert.Throws<RangeError>(() => 1.toString(1));
            Assert.Throws<RangeError>(() => 1.toString(37));
        }

        [Fact]
        public void FormattingMethods_UseNativeNumericFormatters()
        {
            var culture = CultureInfo.InvariantCulture;
            foreach (var value in new[] { -0d, 12.5, 12.345, 1e21, double.Epsilon, double.MaxValue,
                double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                Assert.Equal(value.ToString("F0", culture), value.toFixed());
                Assert.Equal(value.ToString("E", culture), value.toExponential());
                Assert.Equal(value.ToString("G", culture), value.toPrecision());
                foreach (var digits in new[] { 0, 1, 2, 100, 101 })
                {
                    Assert.Equal(value.ToString($"F{digits}", culture), value.toFixed(digits));
                    Assert.Equal(value.ToString($"E{digits}", culture), value.toExponential(digits));
                    Assert.Equal(value.ToString($"G{digits + 1}", culture), value.toPrecision(digits + 1));
                }
            }
            Assert.Equal(0.1f.ToString(culture), 0.1f.toString());
            Assert.Equal("9007199254740993", 9007199254740993L.toString());
            Assert.Equal("9007199254740993.00", 9007199254740993L.toFixed(2));
            Assert.Equal(Int128.MinValue.ToString(culture), Int128.MinValue.toString());
            Assert.Equal(UInt128.MaxValue.ToString(culture), UInt128.MaxValue.toString());
            Assert.Equal("-80000000000000000000000000000000", Int128.MinValue.toString(16));
            Assert.Equal("ffffffffffffffffffffffffffffffff", UInt128.MaxValue.toString(16));
        }

        [Fact]
        public void FormattingMethods_RejectInvalidNativeFormatCounts()
        {
            Assert.Throws<RangeError>(() => 1d.toFixed(-1));
            Assert.Throws<RangeError>(() => 1d.toExponential(-1));
            Assert.Throws<RangeError>(() => 1d.toPrecision(0));
            Assert.Throws<FormatException>(() => 1d.toFixed(int.MaxValue));
            Assert.Throws<FormatException>(() => 1d.toExponential(int.MaxValue));
            Assert.Throws<FormatException>(() => 1d.toPrecision(int.MaxValue));
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
            Assert.True(Number.isSafeInteger(9007199254740993L));
            Assert.True(Number.isSafeInteger(long.MaxValue));
            Assert.True(Number.isSafeInteger(System.Int128.MinValue));
            Assert.True(Number.isSafeInteger(System.UInt128.MaxValue));
            Assert.True(Number.isSafeInteger(9007199254740991d));
            Assert.False(Number.isSafeInteger(9007199254740992d));
            Assert.True(Number.isSafeInteger(16777215f));
            Assert.False(Number.isSafeInteger(16777216f));
            Assert.True(Number.isSafeInteger((Half)2047));
            Assert.False(Number.isSafeInteger((Half)2048));
            Assert.True(Number.isSafeInteger(decimal.MaxValue));
            Assert.False(Number.isSafeInteger(0.5));
            Assert.False(Number.isSafeInteger(double.NaN));
            Assert.True(Number.isSafeInteger((long?)9));
            Assert.False(Number.isSafeInteger((long?)null));
        }
    }
}
