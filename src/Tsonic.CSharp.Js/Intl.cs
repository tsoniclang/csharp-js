using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public sealed class IntlDateTimeFormatConstructor { }
    public sealed class IntlNumberFormatConstructor { }
    public sealed class IntlCollatorConstructor { }

    public static class Intl
    {
        public static IntlDateTimeFormatConstructor DateTimeFormat { get; } = new();
        public static IntlNumberFormatConstructor NumberFormat { get; } = new();
        public static IntlCollatorConstructor Collator { get; } = new();
    }

    public sealed class IntlDateTimeFormatPart
    {
        public IntlDateTimeFormatPart(string type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string type { get; }
        public string value { get; }
    }

    public sealed class IntlNumberFormatPart
    {
        public IntlNumberFormatPart(string type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string type { get; }
        public string value { get; }
    }

    public sealed class IntlResolvedDateTimeFormatOptions
    {
        public string locale => IntlRuntime.DefaultLocale;
        public string calendar => "gregory";
        public string numberingSystem => "latn";
        public string timeZone => IntlRuntime.DefaultTimeZone;
    }

    public sealed class IntlResolvedNumberFormatOptions
    {
        public string locale => IntlRuntime.DefaultLocale;
        public string numberingSystem => "latn";
        public required string style { get; init; }
        public required double minimumIntegerDigits { get; init; }
        public required double minimumFractionDigits { get; init; }
        public required double maximumFractionDigits { get; init; }
        public required bool useGrouping { get; init; }
    }

    public sealed class IntlResolvedCollatorOptions
    {
        public string locale => IntlRuntime.DefaultLocale;
        public required string usage { get; init; }
        public required string sensitivity { get; init; }
        public required bool ignorePunctuation { get; init; }
        public string collation => "default";
        public required bool numeric { get; init; }
        public required string caseFirst { get; init; }
    }

    public sealed class IntlDateTimeFormat
    {
        private readonly string? _weekday;
        private readonly string? _era;
        private readonly string? _year;
        private readonly string? _month;
        private readonly string? _day;
        private readonly string? _hour;
        private readonly string? _minute;
        private readonly string? _second;
        private readonly string? _timeZoneName;
        private readonly bool _hour12;

        public IntlDateTimeFormat(TsValue locales = default, TsValue options = default)
        {
            IntlRuntime.ValidateLocale(locales);
            IntlRuntime.ValidateLocaleMatcher(options);
            IntlRuntime.ValidateTimeZone(options);
            _weekday = IntlRuntime.EnumOption(options, "weekday", "long", "short", "narrow");
            _era = IntlRuntime.EnumOption(options, "era", "long", "short", "narrow");
            _year = IntlRuntime.EnumOption(options, "year", "numeric", "2-digit");
            _month = IntlRuntime.EnumOption(options, "month", "numeric", "2-digit", "long", "short", "narrow");
            _day = IntlRuntime.EnumOption(options, "day", "numeric", "2-digit");
            _hour = IntlRuntime.EnumOption(options, "hour", "numeric", "2-digit");
            _minute = IntlRuntime.EnumOption(options, "minute", "numeric", "2-digit");
            _second = IntlRuntime.EnumOption(options, "second", "numeric", "2-digit");
            _timeZoneName = IntlRuntime.EnumOption(options, "timeZoneName", "long", "short");
            _hour12 = IntlRuntime.BooleanOption(options, "hour12") ?? true;
            if (_weekday is null && _era is null && _year is null && _month is null && _day is null &&
                _hour is null && _minute is null && _second is null && _timeZoneName is null)
            {
                _year = "numeric";
                _month = "numeric";
                _day = "numeric";
            }
        }

        public string format(Date? value = null) => Format(value?.getTime() ?? Date.now());

        public string format(double value) => Format(value);

        public JSArray<IntlDateTimeFormatPart> formatToParts(Date? value = null) =>
            FormatToParts(value?.getTime() ?? Date.now());

        public JSArray<IntlDateTimeFormatPart> formatToParts(double value) => FormatToParts(value);

        public IntlResolvedDateTimeFormatOptions resolvedOptions() => new();

        private string Format(double milliseconds) => string.Concat(
            FormatToParts(milliseconds).Select(part => part.value));

        private JSArray<IntlDateTimeFormatPart> FormatToParts(double milliseconds)
        {
            if (!double.IsFinite(milliseconds))
            {
                return new JSArray<IntlDateTimeFormatPart>(new[]
                {
                    new IntlDateTimeFormatPart("literal", "Invalid Date"),
                });
            }

            DateTimeOffset value;
            try
            {
                value = DateTimeOffset.FromUnixTimeMilliseconds(checked((long)System.Math.Truncate(milliseconds)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return new JSArray<IntlDateTimeFormatPart>(new[]
                {
                    new IntlDateTimeFormatPart("literal", "Invalid Date"),
                });
            }

            var parts = new List<IntlDateTimeFormatPart>();
            if (_weekday is not null)
            {
                parts.Add(new IntlDateTimeFormatPart("weekday", IntlRuntime.Weekday(value.DayOfWeek, _weekday)));
                parts.Add(new IntlDateTimeFormatPart("literal", ", "));
            }

            var dateParts = new List<IntlDateTimeFormatPart>();
            if (_month is not null)
            {
                dateParts.Add(new IntlDateTimeFormatPart("month", IntlRuntime.Month(value.Month, _month)));
            }
            if (_day is not null)
            {
                dateParts.Add(new IntlDateTimeFormatPart("day", IntlRuntime.Number(value.Day, _day)));
            }
            if (_year is not null)
            {
                dateParts.Add(new IntlDateTimeFormatPart("year", IntlRuntime.Number(value.Year, _year)));
            }
            IntlRuntime.AppendSeparated(parts, dateParts, "/");

            if (_era is not null)
            {
                if (dateParts.Count > 0)
                {
                    parts.Add(new IntlDateTimeFormatPart("literal", " "));
                }
                parts.Add(new IntlDateTimeFormatPart("era", IntlRuntime.Era(_era)));
            }

            var timeParts = new List<IntlDateTimeFormatPart>();
            if (_hour is not null)
            {
                var hour = _hour12 ? value.Hour % 12 is 0 ? 12 : value.Hour % 12 : value.Hour;
                timeParts.Add(new IntlDateTimeFormatPart("hour", IntlRuntime.Number(hour, _hour)));
            }
            if (_minute is not null)
            {
                timeParts.Add(new IntlDateTimeFormatPart("minute", IntlRuntime.Number(value.Minute, _minute)));
            }
            if (_second is not null)
            {
                timeParts.Add(new IntlDateTimeFormatPart("second", IntlRuntime.Number(value.Second, _second)));
            }
            if (parts.Count > 0 && timeParts.Count > 0)
            {
                parts.Add(new IntlDateTimeFormatPart("literal", ", "));
            }
            IntlRuntime.AppendSeparated(parts, timeParts, ":");
            if (_hour is not null && _hour12)
            {
                parts.Add(new IntlDateTimeFormatPart("literal", " "));
                parts.Add(new IntlDateTimeFormatPart("dayPeriod", value.Hour < 12 ? "AM" : "PM"));
            }
            if (_timeZoneName is not null)
            {
                if (parts.Count > 0)
                {
                    parts.Add(new IntlDateTimeFormatPart("literal", " "));
                }
                parts.Add(new IntlDateTimeFormatPart(
                    "timeZoneName",
                    _timeZoneName == "long" ? "Coordinated Universal Time" : "UTC"));
            }
            return new JSArray<IntlDateTimeFormatPart>(parts);
        }
    }

    public sealed class IntlNumberFormat
    {
        private readonly string _style;
        private readonly string? _currency;
        private readonly string _currencyDisplay;
        private readonly bool _useGrouping;
        private readonly int _minimumIntegerDigits;
        private readonly int _minimumFractionDigits;
        private readonly int _maximumFractionDigits;

        public IntlNumberFormat(TsValue locales = default, TsValue options = default)
        {
            IntlRuntime.ValidateLocale(locales);
            IntlRuntime.ValidateLocaleMatcher(options);
            _style = IntlRuntime.EnumOption(options, "style", "decimal", "percent", "currency") ?? "decimal";
            _currency = IntlRuntime.StringOption(options, "currency");
            _currencyDisplay = IntlRuntime.EnumOption(options, "currencyDisplay", "symbol", "narrowSymbol", "code", "name") ?? "symbol";
            _useGrouping = IntlRuntime.BooleanOption(options, "useGrouping") ?? true;
            _minimumIntegerDigits = IntlRuntime.IntegerOption(options, "minimumIntegerDigits", 1, 21) ?? 1;
            _minimumFractionDigits = IntlRuntime.IntegerOption(options, "minimumFractionDigits", 0, 20) ?? 0;
            _maximumFractionDigits = IntlRuntime.IntegerOption(options, "maximumFractionDigits", _minimumFractionDigits, 20)
                ?? System.Math.Max(_minimumFractionDigits, _style == "currency" ? 2 : 3);
            if (_style == "currency" && string.IsNullOrWhiteSpace(_currency))
            {
                throw new TypeError("Intl.NumberFormat currency style requires a currency code.");
            }
        }

        public string format(double value) => string.Concat(formatToParts(value).Select(part => part.value));

        public JSArray<IntlNumberFormatPart> formatToParts(double value)
        {
            if (double.IsNaN(value))
            {
                return new JSArray<IntlNumberFormatPart>(new[] { new IntlNumberFormatPart("nan", "NaN") });
            }
            if (double.IsInfinity(value))
            {
                var infinity = new List<IntlNumberFormatPart>();
                if (double.IsNegative(value))
                {
                    infinity.Add(new IntlNumberFormatPart("minusSign", "-"));
                }
                infinity.Add(new IntlNumberFormatPart("infinity", "∞"));
                return new JSArray<IntlNumberFormatPart>(infinity);
            }

            var scaled = _style == "percent" ? value * 100 : value;
            var negative = scaled < 0;
            var absolute = System.Math.Abs(scaled);
            var rendered = absolute.ToString($"F{_maximumFractionDigits}", CultureInfo.InvariantCulture);
            var split = rendered.Split('.', 2);
            var integer = split[0].PadLeft(_minimumIntegerDigits, '0');
            var fraction = split.Length == 1 ? string.Empty : split[1];
            while (fraction.Length > _minimumFractionDigits && fraction.EndsWith('0'))
            {
                fraction = fraction[..^1];
            }

            var parts = new List<IntlNumberFormatPart>();
            if (negative)
            {
                parts.Add(new IntlNumberFormatPart("minusSign", "-"));
            }
            if (_style == "currency")
            {
                parts.Add(new IntlNumberFormatPart("currency", IntlRuntime.Currency(_currency!, _currencyDisplay)));
                if (_currencyDisplay is "code" or "name")
                {
                    parts.Add(new IntlNumberFormatPart("literal", "\u00a0"));
                }
            }

            var groups = IntlRuntime.GroupInteger(integer, _useGrouping);
            for (var index = 0; index < groups.Count; index++)
            {
                if (index > 0)
                {
                    parts.Add(new IntlNumberFormatPart("group", ","));
                }
                parts.Add(new IntlNumberFormatPart("integer", groups[index]));
            }
            if (fraction.Length > 0)
            {
                parts.Add(new IntlNumberFormatPart("decimal", "."));
                parts.Add(new IntlNumberFormatPart("fraction", fraction));
            }
            if (_style == "percent")
            {
                parts.Add(new IntlNumberFormatPart("percentSign", "%"));
            }
            return new JSArray<IntlNumberFormatPart>(parts);
        }

        public IntlResolvedNumberFormatOptions resolvedOptions() => new()
        {
            style = _style,
            minimumIntegerDigits = _minimumIntegerDigits,
            minimumFractionDigits = _minimumFractionDigits,
            maximumFractionDigits = _maximumFractionDigits,
            useGrouping = _useGrouping,
        };
    }

    public sealed class IntlCollator
    {
        private readonly string _usage;
        private readonly string _sensitivity;
        private readonly bool _ignorePunctuation;
        private readonly bool _numeric;
        private readonly string _caseFirst;

        public IntlCollator(TsValue locales = default, TsValue options = default)
        {
            IntlRuntime.ValidateLocale(locales);
            IntlRuntime.ValidateLocaleMatcher(options);
            _usage = IntlRuntime.EnumOption(options, "usage", "sort", "search") ?? "sort";
            _sensitivity = IntlRuntime.EnumOption(options, "sensitivity", "base", "accent", "case", "variant") ?? "variant";
            _ignorePunctuation = IntlRuntime.BooleanOption(options, "ignorePunctuation") ?? false;
            _numeric = IntlRuntime.BooleanOption(options, "numeric") ?? false;
            _caseFirst = IntlRuntime.EnumOption(options, "caseFirst", "upper", "lower", "false") ?? "false";
        }

        public double compare(string left, string right)
        {
            var leftKey = IntlRuntime.CollationKey(left, _sensitivity, _ignorePunctuation);
            var rightKey = IntlRuntime.CollationKey(right, _sensitivity, _ignorePunctuation);
            var result = _numeric
                ? IntlRuntime.CompareNumericStrings(leftKey, rightKey)
                : string.CompareOrdinal(leftKey, rightKey);
            if (result == 0 && _caseFirst != "false" && !string.Equals(left, right, StringComparison.Ordinal))
            {
                var ordinal = string.CompareOrdinal(left, right);
                result = _caseFirst == "upper" ? ordinal : -ordinal;
            }
            return System.Math.Sign(result);
        }

        public IntlResolvedCollatorOptions resolvedOptions() => new()
        {
            usage = _usage,
            sensitivity = _sensitivity,
            ignorePunctuation = _ignorePunctuation,
            numeric = _numeric,
            caseFirst = _caseFirst,
        };
    }

    internal static class IntlRuntime
    {
        public const string DefaultLocale = "en-US";
        public const string DefaultTimeZone = "UTC";

        private static readonly string[] LongMonths =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December",
        };

        private static readonly string[] ShortMonths =
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        };

        private static readonly string[] LongWeekdays =
        {
            "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday",
        };

        private static readonly string[] ShortWeekdays =
        {
            "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
        };

        public static void ValidateLocale(TsValue locales)
        {
            var value = locales.unwrap();
            switch (value)
            {
                case null:
                case Undefined:
                    return;
                case string locale:
                    ValidateLocaleName(locale);
                    return;
                case JSArray<string> list when list.length > 0:
                    ValidateLocaleName(list[0]);
                    return;
                default:
                    throw new TypeError("Intl locales must be a locale string or a non-empty locale array.");
            }
        }

        public static void ValidateLocaleMatcher(TsValue options)
        {
            _ = EnumOption(options, "localeMatcher", "lookup", "best fit");
        }

        public static void ValidateTimeZone(TsValue options)
        {
            var timeZone = StringOption(options, "timeZone") ?? DefaultTimeZone;
            if (timeZone != DefaultTimeZone)
            {
                throw new RangeError($"Intl.DateTimeFormat supports only the deterministic '{DefaultTimeZone}' time zone.");
            }
        }

        public static string? StringOption(TsValue options, string name)
        {
            var value = options.ReadDynamicSlotOptional(name).unwrap();
            return value switch
            {
                null => null,
                Undefined => null,
                string text => text,
                _ => throw new TypeError($"Intl option '{name}' must be a string."),
            };
        }

        public static string? EnumOption(TsValue options, string name, params string[] accepted)
        {
            var value = StringOption(options, name);
            if (value is not null && !accepted.Contains(value, StringComparer.Ordinal))
            {
                throw new RangeError($"Intl option '{name}' has an unsupported value.");
            }
            return value;
        }

        public static bool? BooleanOption(TsValue options, string name)
        {
            var value = options.ReadDynamicSlotOptional(name).unwrap();
            return value switch
            {
                null => null,
                Undefined => null,
                bool boolean => boolean,
                _ => throw new TypeError($"Intl option '{name}' must be boolean."),
            };
        }

        public static int? IntegerOption(TsValue options, string name, int minimum, int maximum)
        {
            var value = options.ReadDynamicSlotOptional(name).unwrap();
            if (value is null or Undefined)
            {
                return null;
            }
            var number = value switch
            {
                double numberValue => numberValue,
                float numberValue => numberValue,
                decimal numberValue => (double)numberValue,
                long numberValue => numberValue,
                ulong numberValue => numberValue,
                int numberValue => numberValue,
                uint numberValue => numberValue,
                short numberValue => numberValue,
                ushort numberValue => numberValue,
                sbyte numberValue => numberValue,
                byte numberValue => numberValue,
                _ => throw new TypeError($"Intl option '{name}' must be numeric."),
            };
            if (!double.IsFinite(number) || System.Math.Truncate(number) != number || number < minimum || number > maximum)
            {
                throw new RangeError($"Intl option '{name}' is outside its supported range.");
            }
            return checked((int)number);
        }

        public static string Number(int value, string style)
        {
            var rendered = value.ToString(CultureInfo.InvariantCulture);
            return style == "2-digit" && rendered.Length < 2 ? "0" + rendered : rendered;
        }

        public static string Month(int month, string style) => style switch
        {
            "long" => LongMonths[month - 1],
            "short" => ShortMonths[month - 1],
            "narrow" => LongMonths[month - 1][..1],
            _ => Number(month, style),
        };

        public static string Weekday(DayOfWeek weekday, string style) => style switch
        {
            "long" => LongWeekdays[(int)weekday],
            "short" => ShortWeekdays[(int)weekday],
            _ => LongWeekdays[(int)weekday][..1],
        };

        public static string Era(string style) => style switch
        {
            "long" => "Anno Domini",
            "narrow" => "A",
            _ => "AD",
        };

        public static void AppendSeparated(
            List<IntlDateTimeFormatPart> target,
            List<IntlDateTimeFormatPart> values,
            string separator)
        {
            for (var index = 0; index < values.Count; index++)
            {
                if (index > 0)
                {
                    target.Add(new IntlDateTimeFormatPart("literal", separator));
                }
                target.Add(values[index]);
            }
        }

        public static List<string> GroupInteger(string value, bool enabled)
        {
            if (!enabled || value.Length <= 3)
            {
                return new List<string> { value };
            }
            var groups = new List<string>();
            var first = value.Length % 3;
            var index = 0;
            if (first > 0)
            {
                groups.Add(value[..first]);
                index = first;
            }
            while (index < value.Length)
            {
                groups.Add(value.Substring(index, 3));
                index += 3;
            }
            return groups;
        }

        public static string Currency(string currency, string display)
        {
            var normalized = currency.ToUpperInvariant();
            return display switch
            {
                "code" => normalized,
                "name" => normalized switch
                {
                    "USD" => "US dollars",
                    "EUR" => "euros",
                    "GBP" => "British pounds",
                    "JPY" => "Japanese yen",
                    _ => throw new RangeError($"Intl currency name data is unavailable for '{normalized}'."),
                },
                _ => normalized switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "GBP" => "£",
                    "JPY" => "¥",
                    "CNY" => "CN¥",
                    "INR" => "₹",
                    _ => normalized,
                },
            };
        }

        public static string CollationKey(string value, string sensitivity, bool ignorePunctuation)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (!ignorePunctuation || char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
                {
                    builder.Append(character);
                }
            }
            var filtered = builder.ToString();
            return sensitivity is "base" or "accent" ? filtered.ToLowerInvariant() : filtered;
        }

        public static int CompareNumericStrings(string left, string right)
        {
            var leftIndex = 0;
            var rightIndex = 0;
            while (leftIndex < left.Length && rightIndex < right.Length)
            {
                if (IsAsciiDigit(left[leftIndex]) && IsAsciiDigit(right[rightIndex]))
                {
                    var leftEnd = DigitRunEnd(left, leftIndex);
                    var rightEnd = DigitRunEnd(right, rightIndex);
                    var leftSignificant = SignificantDigitStart(left, leftIndex, leftEnd);
                    var rightSignificant = SignificantDigitStart(right, rightIndex, rightEnd);
                    var leftLength = leftEnd - leftSignificant;
                    var rightLength = rightEnd - rightSignificant;
                    if (leftLength != rightLength)
                    {
                        return leftLength.CompareTo(rightLength);
                    }
                    var numeric = string.CompareOrdinal(
                        left,
                        leftSignificant,
                        right,
                        rightSignificant,
                        leftLength);
                    if (numeric != 0)
                    {
                        return numeric;
                    }
                    leftIndex = leftEnd;
                    rightIndex = rightEnd;
                    continue;
                }

                var character = left[leftIndex].CompareTo(right[rightIndex]);
                if (character != 0)
                {
                    return character;
                }
                leftIndex += 1;
                rightIndex += 1;
            }
            return (left.Length - leftIndex).CompareTo(right.Length - rightIndex);
        }

        private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';

        private static int DigitRunEnd(string value, int start)
        {
            var index = start;
            while (index < value.Length && IsAsciiDigit(value[index]))
            {
                index += 1;
            }
            return index;
        }

        private static int SignificantDigitStart(string value, int start, int end)
        {
            var index = start;
            while (index < end - 1 && value[index] == '0')
            {
                index += 1;
            }
            return index;
        }

        private static void ValidateLocaleName(string locale)
        {
            if (locale is not "en" and not DefaultLocale)
            {
                throw new RangeError($"Intl locale '{locale}' is outside the deterministic locale set.");
            }
        }
    }
}
