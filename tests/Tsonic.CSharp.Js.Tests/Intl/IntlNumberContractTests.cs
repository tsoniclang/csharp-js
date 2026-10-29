using Tsonic.CSharp.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace Tsonic.CSharp.Js.Tests
{
    public class IntlNumberContractTests
    {
        private readonly ITestOutputHelper _output;

        public IntlNumberContractTests(ITestOutputHelper output) => _output = output;

        private static IntlNumberFormat Create(params object?[] options) =>
            new(TsValue.from("en"), TsValue.from(JSON.createObject(options)));

        [Fact]
        public void DecimalIntegerOptionsKeepTheirExactNativeBoundsAndFraction()
        {
            Assert.Throws<RangeError>(() => Create("minimumIntegerDigits", 0.9999999999999999999999999999m));
            Assert.Equal(1, Create("minimumIntegerDigits", 1m).resolvedOptions().minimumIntegerDigits);
            Assert.Equal(1, Create("minimumIntegerDigits", 1.9999999999999999999999999999m).resolvedOptions().minimumIntegerDigits);
            Assert.Equal(20, Create("minimumIntegerDigits", 20.999999999999999999999999999m).resolvedOptions().minimumIntegerDigits);
            Assert.Equal(21, Create("minimumIntegerDigits", 21m).resolvedOptions().minimumIntegerDigits);
            Assert.Throws<RangeError>(() => Create("minimumIntegerDigits", 21.000000000000000000000000001m));
            Assert.Equal(99, Create("maximumFractionDigits", 99.99999999999999999999999999m).resolvedOptions().maximumFractionDigits);
            Assert.Equal(100, Create("maximumFractionDigits", 100m).resolvedOptions().maximumFractionDigits);
            Assert.Throws<RangeError>(() => Create("maximumFractionDigits", 100.00000000000000000000000001m));
            Assert.Equal(1, Create("roundingIncrement", 1.9999999999999999999999999999m).resolvedOptions().roundingIncrement);
        }

        public static System.Collections.Generic.IEnumerable<object[]> NativeOptionDomains()
        {
            yield return new object[] { "double", 17.9d, 0d, 22d };
            yield return new object[] { "float", 17.9f, 0f, 22f };
            yield return new object[] { "decimal", 17.9m, 0m, 22m };
            yield return new object[] { "long", 17L, long.MinValue, long.MaxValue };
            yield return new object[] { "ulong", 17UL, 0UL, ulong.MaxValue };
            yield return new object[] { "int", 17, int.MinValue, int.MaxValue };
            yield return new object[] { "uint", 17U, 0U, uint.MaxValue };
            yield return new object[] { "short", (short)17, short.MinValue, short.MaxValue };
            yield return new object[] { "ushort", (ushort)17, (ushort)0, ushort.MaxValue };
            yield return new object[] { "sbyte", (sbyte)17, sbyte.MinValue, sbyte.MaxValue };
            yield return new object[] { "byte", (byte)17, (byte)0, byte.MaxValue };
        }

        [Theory]
        [MemberData(nameof(NativeOptionDomains))]
        public void IntegerOptionDomainsRetainRangeAndProjection(string domain, object valid, object below, object above)
        {
            Assert.True(Create("minimumIntegerDigits", valid).resolvedOptions().minimumIntegerDigits == 17,
                $"{domain}: valid native payload truncates without changing its domain");
            Assert.Throws<RangeError>(() => Create("minimumIntegerDigits", below));
            Assert.Throws<RangeError>(() => Create("minimumIntegerDigits", above));
        }

        [Fact]
        public void IntegerOptionsRetainNonfiniteAndNonnumericRefusals()
        {
            foreach (var value in new object[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity,
                float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assert.Throws<RangeError>(() => Create("minimumIntegerDigits", value));
            Assert.Throws<TypeError>(() => Create("minimumIntegerDigits", true));
            Assert.Throws<TypeError>(() => Create("minimumIntegerDigits", "2"));
        }

        [Fact]
        public void TypedIntegerOptionsRetainDictionaryAndPayloadIdentity()
        {
            object payload = 1.9999999999999999999999999999m;
            var options = new System.Collections.Generic.Dictionary<string, TsValue>
            {
                ["minimumIntegerDigits"] = TsValue.from(payload),
            };
            var selected = TsValue.from(options);
            Assert.Same(options, selected.unwrap());
            var formatter = new IntlNumberFormat(TsValue.from("en"), selected);
            Assert.Equal(1, formatter.resolvedOptions().minimumIntegerDigits);
            Assert.Same(payload, options["minimumIntegerDigits"].unwrap());
            Assert.Same(options, selected.unwrap());
            options["minimumIntegerDigits"] = TsValue.from(0.9999999999999999999999999999m);
            Assert.Throws<RangeError>(() => new IntlNumberFormat(TsValue.from("en"), selected));
            options["minimumIntegerDigits"] = TsValue.from((object?)null);
            Assert.Equal(1, new IntlNumberFormat(TsValue.from("en"), selected).resolvedOptions().minimumIntegerDigits);
        }

        [Fact]
        public void DecimalIntegerOptionProjectionAddsNoAllocationToEquivalentNativeOptions()
        {
            var native = TsValue.from(new System.Collections.Generic.Dictionary<string, TsValue>
            {
                ["minimumIntegerDigits"] = TsValue.from(5),
            });
            var fraction = TsValue.from(new System.Collections.Generic.Dictionary<string, TsValue>
            {
                ["minimumIntegerDigits"] = TsValue.from(5.9m),
            });
            var locale = TsValue.from("en");
            IntlNumberFormat? latest = null;
            for (var iteration = 0; iteration < 10000; iteration++)
            {
                latest = new IntlNumberFormat(locale, native);
                latest = new IntlNumberFormat(locale, fraction);
            }
            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var iteration = 0; iteration < 10000; iteration++) latest = new IntlNumberFormat(locale, native);
            var nativeBytes = System.GC.GetAllocatedBytesForCurrentThread() - before;
            System.GC.KeepAlive(latest);
            before = System.GC.GetAllocatedBytesForCurrentThread();
            for (var iteration = 0; iteration < 10000; iteration++) latest = new IntlNumberFormat(locale, fraction);
            var fractionBytes = System.GC.GetAllocatedBytesForCurrentThread() - before;
            System.GC.KeepAlive(latest);
            Assert.Equal(5, latest!.resolvedOptions().minimumIntegerDigits);
            _output.WriteLine($"nativeBytes={nativeBytes}; decimalBytes={fractionBytes}; iterations=10000");
            Assert.Equal(nativeBytes, fractionBytes);
        }

        [Fact]
        public void SignificantPrecisionRetainsAbsenceInsteadOfInventingZero()
        {
            var formatter = Create("maximumSignificantDigits", 3.0);
            var result = formatter.resolvedOptions();
            Assert.Null(result.minimumFractionDigits);
            Assert.Null(result.maximumFractionDigits);
            Assert.Equal(1, result.minimumSignificantDigits);
            Assert.Equal(3, result.maximumSignificantDigits);
            Assert.Equal("auto", result.useGrouping.As2());
            Assert.Equal("1,230", formatter.format(1234.5));
            Assert.Equal("0.00123", formatter.format(0.0012345));
            var mixed = Create("maximumSignificantDigits", 3.9, "maximumFractionDigits", -1.0);
            Assert.Equal(3, mixed.resolvedOptions().maximumSignificantDigits);
            Assert.Null(mixed.resolvedOptions().maximumFractionDigits);
            Assert.Equal("1,230", mixed.format(1234.5));
        }

        [Fact]
        public void FractionDefaultsAndZeroRemainDistinctFromAbsence()
        {
            var result = Create().resolvedOptions();
            Assert.Equal(0, result.minimumFractionDigits);
            Assert.Equal(3, result.maximumFractionDigits);
            Assert.Null(result.maximumSignificantDigits);
            Assert.Equal("en-US", result.locale);
            Assert.Equal("latn", result.numberingSystem);
            Assert.Equal("decimal", result.style);
            Assert.Equal(1, result.minimumIntegerDigits);
            Assert.Null(result.currency);
            Assert.Null(result.currencyDisplay);
            Assert.Null(result.currencySign);
            Assert.Null(result.unit);
            Assert.Null(result.unitDisplay);
            Assert.Null(result.compactDisplay);
            Assert.Equal("standard", result.notation);
            Assert.Equal("auto", result.signDisplay);
            Assert.Equal("auto", result.roundingPriority);
            Assert.Equal(1, result.roundingIncrement);
            Assert.Equal("halfExpand", result.roundingMode);
            Assert.Equal("auto", result.trailingZeroDisplay);
            var currency = Create("style", "currency", "currency", "usd", "currencyDisplay", "code").resolvedOptions();
            Assert.Equal("USD", currency.currency);
            Assert.Equal("code", currency.currencyDisplay);
            Assert.Equal("standard", currency.currencySign);
            Assert.Equal(0, Create("maximumFractionDigits", 0.0).resolvedOptions().maximumFractionDigits);
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
