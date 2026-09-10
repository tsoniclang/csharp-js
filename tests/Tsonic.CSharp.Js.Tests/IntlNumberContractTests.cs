using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class IntlNumberContractTests
    {
        private static IntlNumberFormat Create(params object?[] options) =>
            new(TsValue.from("en"), TsValue.from(JSON.createObject(options)));

        [Fact]
        public void SignificantPrecisionRetainsAbsenceInsteadOfInventingZero()
        {
            var formatter = Create("maximumSignificantDigits", 3.0);
            var result = formatter.resolvedOptions();
            Assert.Null(result.minimumFractionDigits);
            Assert.Null(result.maximumFractionDigits);
            Assert.Equal(1.0, result.minimumSignificantDigits);
            Assert.Equal(3.0, result.maximumSignificantDigits);
            Assert.Equal("auto", result.useGrouping.As2());
            Assert.Equal("1,230", formatter.format(1234.5));
            Assert.Equal("0.00123", formatter.format(0.0012345));
            var mixed = Create("maximumSignificantDigits", 3.9, "maximumFractionDigits", -1.0);
            Assert.Equal(3.0, mixed.resolvedOptions().maximumSignificantDigits);
            Assert.Null(mixed.resolvedOptions().maximumFractionDigits);
            Assert.Equal("1,230", mixed.format(1234.5));
        }

        [Fact]
        public void FractionDefaultsAndZeroRemainDistinctFromAbsence()
        {
            var result = Create().resolvedOptions();
            Assert.Equal(0.0, result.minimumFractionDigits);
            Assert.Equal(3.0, result.maximumFractionDigits);
            Assert.Null(result.maximumSignificantDigits);
            Assert.Equal(0.0, Create("maximumFractionDigits", 0.0).resolvedOptions().maximumFractionDigits);
            Assert.Equal("1.01", Create("maximumFractionDigits", 2.0).format(1.005));
            Assert.Equal("1.01", Create("maximumFractionDigits", 2.9).format(1.005));
            Assert.Equal("-0", Create().format(-0.0));
            Assert.Equal("$1.00", Create("style", "currency", "currency", "USD").format(1.0));
            Assert.Equal("13%", Create("style", "percent").format(0.125));
            Assert.Equal("0.00", Create("minimumSignificantDigits", 3.0).format(0.0));
            Assert.Equal("1.25", Create("roundingIncrement", 1.9).format(1.25));
        }

        [Theory]
        [InlineData(false, "off", "1234")]
        [InlineData(true, "always", "1,234")]
        [InlineData("auto", "auto", "1,234")]
        [InlineData("always", "always", "1,234")]
        [InlineData("min2", "min2", "1234")]
        public void GroupingRetainsSelectedStrategy(object input, string selected, string expected)
        {
            var formatter = Create("useGrouping", input);
            var grouping = formatter.resolvedOptions().useGrouping;
            Assert.Equal(selected, grouping.Is1() ? "off" : grouping.As2());
            if (grouping.Is1()) Assert.False(grouping.As1());
            Assert.Equal(expected, formatter.format(1234.0));
            Assert.Equal(selected == "off" ? "12345" : "12,345", formatter.format(12345.0));
        }

        [Fact]
        public void IntegerInputsAndPartsNeverPassThroughFloatingPoint()
        {
            var formatter = Create();
            Assert.Equal("9,007,199,254,740,993", formatter.formatInteger(9007199254740993L));
            Assert.Equal("-9,223,372,036,854,775,808", formatter.formatInteger(long.MinValue));
            Assert.Equal("18,446,744,073,709,551,615", formatter.formatInteger(ulong.MaxValue));
            Assert.Equal("9,007,199,254,740,993", string.Concat(System.Linq.Enumerable.Select(formatter.formatToPartsInteger(9007199254740993UL), part => part.value)));
            Assert.Equal("9,007,199,254,740,993", Intl.formatInteger(9007199254740993L));
            Assert.Equal("9007199254740993", Intl.formatInteger(9007199254740993UL, TsValue.from("en-US"), TsValue.from(JSON.createObject("useGrouping", false))));
            Assert.Equal("900,719,925,474,099,300%", Create("style", "percent").formatInteger(9007199254740993L));
            var plain = Create("useGrouping", false);
            Assert.Equal("-170141183460469231731687303715884105728", plain.formatInteger(System.Int128.MinValue));
            Assert.Equal("340282366920938463463374607431768211455", plain.formatInteger(System.UInt128.MaxValue));
            var arbitrary = System.Numerics.BigInteger.Parse("340282366920938463463374607431768211456123");
            Assert.Equal("340282366920938463463374607431768211456123", plain.formatInteger(arbitrary));
            Assert.Equal("-340282366920938463463374607431768211456123", plain.formatInteger(-arbitrary));
        }

        [Theory]
        [InlineData("useGrouping", "unknown")]
        [InlineData("notation", "compact")]
        [InlineData("notation", "scientific")]
        [InlineData("style", "unit")]
        [InlineData("unit", "not-a-unit")]
        [InlineData("unit", "meter")]
        [InlineData("currencySign", "accounting")]
        [InlineData("roundingMode", "halfEven")]
        [InlineData("roundingPriority", "morePrecision")]
        [InlineData("roundingIncrement", 5.0)]
        [InlineData("signDisplay", "always")]
        [InlineData("trailingZeroDisplay", "stripIfInteger")]
        [InlineData("maximumSignificantDigits", 22.0)]
        [InlineData("maximumFractionDigits", 101.0)]
        public void UnsupportedAndInvalidOptionsRejectAtConstruction(string option, object value) =>
            Assert.Throws<RangeError>(() => Create(option, value));

        [Fact]
        public void InvertedDigitBoundsReject()
        {
            Assert.Throws<RangeError>(() => Create("minimumSignificantDigits", 4.0, "maximumSignificantDigits", 3.0));
            Assert.Throws<RangeError>(() => Create("minimumFractionDigits", 4.0, "maximumFractionDigits", 3.0));
        }
    }
}
