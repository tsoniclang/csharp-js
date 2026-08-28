/**
 * JavaScript Date implementation
 * Retains the ECMAScript epoch-millisecond scalar and uses DateTimeOffset only
 * for representable local-time operations.
 */

using System;
using System.Globalization;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Date - date and time handling
    /// </summary>
    public class Date
    {
        private DateTimeOffset _value;
        private double _milliseconds;
        private bool _hasLocalValue;

        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private const long MillisecondsPerDay = 86_400_000;
        private const double MaximumTimeMilliseconds = 8_640_000_000_000_000.0;
        private static readonly string[] Weekdays = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // ==================== Constructors ====================

        /// <summary>
        /// Create Date with current time
        /// </summary>
        public Date()
        {
            SetFromMilliseconds(now());
        }

        /// <summary>
        /// Create Date from milliseconds since epoch
        /// </summary>
        public Date(double milliseconds)
        {
            SetFromMilliseconds(milliseconds);
        }

        /// <summary>
        /// Create Date from date string
        /// </summary>
        public Date(string dateString)
        {
            SetFromString(dateString);
        }

        /// <summary>
        /// Create Date from the closed JavaScript Date constructor carrier.
        /// </summary>
        public Date(object value)
        {
            switch (value)
            {
                case Date date:
                    _value = date._value;
                    _milliseconds = date._milliseconds;
                    _hasLocalValue = date._hasLocalValue;
                    break;
                case string text:
                    SetFromString(text);
                    break;
                case double number:
                    SetFromMilliseconds(number);
                    break;
                case float number:
                    SetFromMilliseconds(number);
                    break;
                case decimal number:
                    SetFromMilliseconds((double)number);
                    break;
                case long number:
                    SetFromMilliseconds(number);
                    break;
                case ulong number:
                    SetFromMilliseconds(number);
                    break;
                case int number:
                    SetFromMilliseconds(number);
                    break;
                case uint number:
                    SetFromMilliseconds(number);
                    break;
                case short number:
                    SetFromMilliseconds(number);
                    break;
                case ushort number:
                    SetFromMilliseconds(number);
                    break;
                case sbyte number:
                    SetFromMilliseconds(number);
                    break;
                case byte number:
                    SetFromMilliseconds(number);
                    break;
                case null:
                    SetFromMilliseconds(0);
                    break;
                default:
                    SetInvalid();
                    break;
            }
        }

        private void SetFromMilliseconds(double milliseconds)
        {
            _milliseconds = TimeClip(milliseconds);
            if (!double.IsFinite(_milliseconds))
            {
                _value = DateTimeOffset.MinValue;
                _hasLocalValue = false;
            }
            else
            {
                try
                {
                    _value = Epoch.AddMilliseconds(_milliseconds);
                    _hasLocalValue = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    _value = DateTimeOffset.MinValue;
                    _hasLocalValue = false;
                }
            }
        }

        private void SetFromString(string dateString)
        {
            if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                _value = parsed;
                _milliseconds = TimeClip((parsed.ToUniversalTime() - Epoch).TotalMilliseconds);
                _hasLocalValue = true;
            }
            else
            {
                SetInvalid();
            }
        }

        /// <summary>
        /// Create Date from year, month, day, etc.
        /// Month is 0-indexed (0 = January) per JavaScript convention
        /// </summary>
        public Date(int year, int month, int day = 1, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            try
            {
                // JavaScript months are 0-indexed, DateTimeOffset months are 1-indexed
                _value = new DateTimeOffset(year, month + 1, day, hours, minutes, seconds, milliseconds, TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
                _hasLocalValue = true;
                SyncMillisecondsFromValue();
            }
            catch
            {
                SetInvalid();
            }
        }

        // ==================== Static Methods ====================

        /// <summary>
        /// Returns current time in milliseconds since epoch
        /// </summary>
        public static double now() => (DateTimeOffset.UtcNow - Epoch).TotalMilliseconds;

        /// <summary>
        /// Parse date string and return milliseconds since epoch
        /// </summary>
        public static double parse(string dateString)
        {
            if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return (parsed - Epoch).TotalMilliseconds;
            }
            return double.NaN;
        }

        /// <summary>
        /// Create UTC date and return milliseconds since epoch
        /// Month is 0-indexed per JavaScript convention
        /// </summary>
        public static double UTC(int year, int month, int day = 1, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            var normalizedYear = year >= 0 && year <= 99 ? year + 1900 : year;
            return MakeUtcMilliseconds(normalizedYear, month, day, hours, minutes, seconds, milliseconds);
        }

        public static string call() => new Date().ToString();

        // ==================== Instance Methods - Getters (Local Time) ====================

        /// <summary>
        /// Get milliseconds since epoch
        /// </summary>
        public double getTime() => _milliseconds;

        /// <summary>
        /// Get full year (4 digits)
        /// </summary>
        public int getFullYear() => _value.LocalDateTime.Year;

        /// <summary>
        /// Get month (0-11, 0 = January)
        /// </summary>
        public int getMonth() => _value.LocalDateTime.Month - 1;

        /// <summary>
        /// Get day of month (1-31)
        /// </summary>
        public int getDate() => _value.LocalDateTime.Day;

        /// <summary>
        /// Get day of week (0-6, 0 = Sunday)
        /// </summary>
        public int getDay() => (int)_value.LocalDateTime.DayOfWeek;

        /// <summary>
        /// Get hours (0-23)
        /// </summary>
        public int getHours() => _value.LocalDateTime.Hour;

        /// <summary>
        /// Get minutes (0-59)
        /// </summary>
        public int getMinutes() => _value.LocalDateTime.Minute;

        /// <summary>
        /// Get seconds (0-59)
        /// </summary>
        public int getSeconds() => _value.LocalDateTime.Second;

        /// <summary>
        /// Get milliseconds (0-999)
        /// </summary>
        public int getMilliseconds() => _value.LocalDateTime.Millisecond;

        /// <summary>
        /// Get timezone offset in minutes
        /// </summary>
        public int getTimezoneOffset() => -(int)_value.Offset.TotalMinutes;

        // ==================== Instance Methods - Getters (UTC) ====================

        /// <summary>
        /// Get UTC full year
        /// </summary>
        public double getUTCFullYear() => TryGetUtcParts(out var parts) ? parts.Year : double.NaN;

        /// <summary>
        /// Get UTC month (0-11)
        /// </summary>
        public double getUTCMonth() => TryGetUtcParts(out var parts) ? parts.Month - 1 : double.NaN;

        /// <summary>
        /// Get UTC day of month
        /// </summary>
        public double getUTCDate() => TryGetUtcParts(out var parts) ? parts.Day : double.NaN;

        /// <summary>
        /// Get UTC day of week
        /// </summary>
        public double getUTCDay()
        {
            if (!double.IsFinite(_milliseconds))
                return double.NaN;
            var days = FloorDiv(checked((long)Math.Truncate(_milliseconds)), MillisecondsPerDay);
            return Modulo(days + 4, 7);
        }

        /// <summary>
        /// Get UTC hours
        /// </summary>
        public double getUTCHours() => TryGetUtcParts(out var parts) ? parts.Hour : double.NaN;

        /// <summary>
        /// Get UTC minutes
        /// </summary>
        public double getUTCMinutes() => TryGetUtcParts(out var parts) ? parts.Minute : double.NaN;

        /// <summary>
        /// Get UTC seconds
        /// </summary>
        public double getUTCSeconds() => TryGetUtcParts(out var parts) ? parts.Second : double.NaN;

        /// <summary>
        /// Get UTC milliseconds
        /// </summary>
        public double getUTCMilliseconds() => TryGetUtcParts(out var parts) ? parts.Millisecond : double.NaN;

        // ==================== Instance Methods - Setters (Local Time) ====================

        /// <summary>
        /// Set time in milliseconds since epoch
        /// </summary>
        public double setTime(double milliseconds)
        {
            SetFromMilliseconds(milliseconds);
            return getTime();
        }

        /// <summary>
        /// Set milliseconds
        /// </summary>
        public double setMilliseconds(int ms)
        {
            var local = _value.LocalDateTime;
            _value = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second, ms, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set seconds (and optionally milliseconds)
        /// </summary>
        public double setSeconds(int sec, int? ms = null)
        {
            var local = _value.LocalDateTime;
            var newMs = ms ?? local.Millisecond;
            _value = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, local.Minute, sec, newMs, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set minutes (and optionally seconds, milliseconds)
        /// </summary>
        public double setMinutes(int min, int? sec = null, int? ms = null)
        {
            var local = _value.LocalDateTime;
            var newSec = sec ?? local.Second;
            var newMs = ms ?? local.Millisecond;
            _value = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, min, newSec, newMs, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set hours (and optionally minutes, seconds, milliseconds)
        /// </summary>
        public double setHours(int hour, int? min = null, int? sec = null, int? ms = null)
        {
            var local = _value.LocalDateTime;
            var newMin = min ?? local.Minute;
            var newSec = sec ?? local.Second;
            var newMs = ms ?? local.Millisecond;
            _value = new DateTimeOffset(local.Year, local.Month, local.Day, hour, newMin, newSec, newMs, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set day of month
        /// </summary>
        public double setDate(int day)
        {
            var local = _value.LocalDateTime;
            _value = new DateTimeOffset(local.Year, local.Month, day, local.Hour, local.Minute, local.Second, local.Millisecond, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set month (0-11) and optionally day
        /// </summary>
        public double setMonth(int month, int? day = null)
        {
            var local = _value.LocalDateTime;
            var newDay = day ?? local.Day;
            // JavaScript months are 0-indexed
            _value = new DateTimeOffset(local.Year, month + 1, newDay, local.Hour, local.Minute, local.Second, local.Millisecond, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        /// <summary>
        /// Set full year (and optionally month, day)
        /// </summary>
        public double setFullYear(int year, int? month = null, int? day = null)
        {
            var local = _value.LocalDateTime;
            // JavaScript months are 0-indexed, need to add 1
            var newMonth = month.HasValue ? month.Value + 1 : local.Month;
            var newDay = day ?? local.Day;
            _value = new DateTimeOffset(year, newMonth, newDay, local.Hour, local.Minute, local.Second, local.Millisecond, _value.Offset);
            SyncMillisecondsFromValue();
            return getTime();
        }

        // ==================== Instance Methods - Setters (UTC) ====================

        /// <summary>
        /// Set UTC milliseconds
        /// </summary>
        public double setUTCMilliseconds(double ms)
        {
            return MutateUtc(milliseconds: ms);
        }

        /// <summary>
        /// Set UTC seconds
        /// </summary>
        public double setUTCSeconds(double sec, double? ms = null)
        {
            return MutateUtc(seconds: sec, milliseconds: ms);
        }

        /// <summary>
        /// Set UTC minutes
        /// </summary>
        public double setUTCMinutes(double min, double? sec = null, double? ms = null)
        {
            return MutateUtc(minutes: min, seconds: sec, milliseconds: ms);
        }

        /// <summary>
        /// Set UTC hours
        /// </summary>
        public double setUTCHours(double hour, double? min = null, double? sec = null, double? ms = null)
        {
            return MutateUtc(hours: hour, minutes: min, seconds: sec, milliseconds: ms);
        }

        /// <summary>
        /// Set UTC day of month
        /// </summary>
        public double setUTCDate(double day)
        {
            return MutateUtc(date: day);
        }

        /// <summary>
        /// Set UTC month (0-11)
        /// </summary>
        public double setUTCMonth(double month, double? day = null)
        {
            return MutateUtc(month: month, date: day);
        }

        /// <summary>
        /// Set UTC full year
        /// </summary>
        public double setUTCFullYear(double year, double? month = null, double? day = null)
        {
            return MutateUtc(year: year, month: month, date: day);
        }

        private double MutateUtc(
            double? year = null,
            double? month = null,
            double? date = null,
            double? hours = null,
            double? minutes = null,
            double? seconds = null,
            double? milliseconds = null)
        {
            if (!TryGetUtcParts(out var parts))
            {
                if (!year.HasValue)
                {
                    SetInvalid();
                    return double.NaN;
                }
                parts = new UtcParts(1970, 1, 1, 0, 0, 0, 0);
            }

            var result = MakeUtcMilliseconds(
                year ?? parts.Year,
                month ?? (parts.Month - 1),
                date ?? parts.Day,
                hours ?? parts.Hour,
                minutes ?? parts.Minute,
                seconds ?? parts.Second,
                milliseconds ?? parts.Millisecond);
            SetFromMilliseconds(result);
            return getTime();
        }

        private void SetInvalid()
        {
            _milliseconds = double.NaN;
            _value = DateTimeOffset.MinValue;
            _hasLocalValue = false;
        }

        private void SyncMillisecondsFromValue()
        {
            _milliseconds = TimeClip((_value.ToUniversalTime() - Epoch).TotalMilliseconds);
            _hasLocalValue = true;
        }

        private bool TryGetUtcParts(out UtcParts parts)
        {
            if (!double.IsFinite(_milliseconds))
            {
                parts = default;
                return false;
            }

            var milliseconds = checked((long)Math.Truncate(_milliseconds));
            var days = FloorDiv(milliseconds, MillisecondsPerDay);
            var millisecondsInDay = Modulo(milliseconds, MillisecondsPerDay);
            var (year, month, day) = CivilFromDays(days);
            parts = new UtcParts(
                year,
                month,
                day,
                millisecondsInDay / 3_600_000,
                (millisecondsInDay % 3_600_000) / 60_000,
                (millisecondsInDay % 60_000) / 1_000,
                millisecondsInDay % 1_000);
            return true;
        }

        private static double MakeUtcMilliseconds(
            double year,
            double month,
            double day,
            double hours,
            double minutes,
            double seconds,
            double milliseconds)
        {
            if (!double.IsFinite(year)
                || !double.IsFinite(month)
                || !double.IsFinite(day)
                || !double.IsFinite(hours)
                || !double.IsFinite(minutes)
                || !double.IsFinite(seconds)
                || !double.IsFinite(milliseconds))
                return double.NaN;

            year = Math.Truncate(year);
            month = Math.Truncate(month);
            if (Math.Abs(year) > 1_000_000 || Math.Abs(month) > 10_000_000)
                return double.NaN;

            var totalMonths = checked((long)year * 12 + (long)month);
            var civilYear = FloorDiv(totalMonths, 12);
            if (Math.Abs(civilYear) > 1_000_000)
                return double.NaN;
            var civilMonth = checked((int)Modulo(totalMonths, 12) + 1);
            var dayNumber = DaysFromCivil(civilYear, civilMonth, 1);
            return TimeClip(
                (dayNumber + Math.Truncate(day) - 1) * MillisecondsPerDay
                + Math.Truncate(hours) * 3_600_000
                + Math.Truncate(minutes) * 60_000
                + Math.Truncate(seconds) * 1_000
                + Math.Truncate(milliseconds));
        }

        private static double TimeClip(double value)
        {
            if (!double.IsFinite(value) || Math.Abs(value) > MaximumTimeMilliseconds)
                return double.NaN;
            return value == 0 ? 0 : Math.Truncate(value);
        }

        private static long DaysFromCivil(long year, int month, int day)
        {
            year -= month <= 2 ? 1 : 0;
            var era = FloorDiv(year, 400);
            var yearOfEra = year - era * 400;
            var dayOfYear = (153 * (month + (month > 2 ? -3 : 9)) + 2) / 5 + day - 1;
            var dayOfEra = yearOfEra * 365 + yearOfEra / 4 - yearOfEra / 100 + dayOfYear;
            return era * 146_097 + dayOfEra - 719_468;
        }

        private static (int Year, int Month, int Day) CivilFromDays(long days)
        {
            var zeroDay = days + 719_468;
            var era = FloorDiv(zeroDay, 146_097);
            var dayOfEra = zeroDay - era * 146_097;
            var yearOfEra = (dayOfEra - dayOfEra / 1_460 + dayOfEra / 36_524 - dayOfEra / 146_096) / 365;
            var year = yearOfEra + era * 400;
            var dayOfYear = dayOfEra - (365 * yearOfEra + yearOfEra / 4 - yearOfEra / 100);
            var monthPart = (5 * dayOfYear + 2) / 153;
            var day = dayOfYear - (153 * monthPart + 2) / 5 + 1;
            var month = monthPart + (monthPart < 10 ? 3 : -9);
            year += month <= 2 ? 1 : 0;
            return (checked((int)year), checked((int)month), checked((int)day));
        }

        private static long FloorDiv(long dividend, long divisor)
        {
            var quotient = dividend / divisor;
            return dividend % divisor < 0 ? quotient - 1 : quotient;
        }

        private static long Modulo(long value, long modulus)
        {
            var remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        private static string IsoYear(int year) => year switch
        {
            >= 0 and <= 9999 => year.ToString("0000", CultureInfo.InvariantCulture),
            < 0 => $"-{Math.Abs((long)year):000000}",
            _ => $"+{year:000000}",
        };

        private static string UtcStringYear(int year) => year >= 0
            ? year.ToString("0000", CultureInfo.InvariantCulture)
            : $"-{Math.Abs((long)year):0000}";

        private readonly record struct UtcParts(
            int Year,
            int Month,
            int Day,
            long Hour,
            long Minute,
            long Second,
            long Millisecond);

        // ==================== Instance Methods - String Conversion ====================

        /// <summary>
        /// Convert to string representation
        /// </summary>
        public override string ToString() => double.IsFinite(_milliseconds) && _hasLocalValue
            ? _value.LocalDateTime.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", CultureInfo.InvariantCulture)
            : "Invalid Date";

        /// <summary>
        /// Convert to date string
        /// </summary>
        public string toDateString() => _value.LocalDateTime.ToString("ddd MMM dd yyyy", CultureInfo.InvariantCulture);

        /// <summary>
        /// Convert to time string
        /// </summary>
        public string toTimeString() => _value.LocalDateTime.ToString("HH:mm:ss 'GMT'zzz", CultureInfo.InvariantCulture);

        /// <summary>
        /// Convert to ISO 8601 string
        /// </summary>
        public string toISOString()
        {
            if (!TryGetUtcParts(out var parts))
                throw new RangeError("Invalid Date");
            return $"{IsoYear(parts.Year)}-{parts.Month:00}-{parts.Day:00}T{parts.Hour:00}:{parts.Minute:00}:{parts.Second:00}.{parts.Millisecond:000}Z";
        }

        /// <summary>
        /// Convert to UTC string
        /// </summary>
        public string toUTCString()
        {
            if (!TryGetUtcParts(out var parts))
                return "Invalid Date";
            var weekday = checked((int)getUTCDay());
            return $"{Weekdays[weekday]}, {parts.Day:00} {Months[parts.Month - 1]} {UtcStringYear(parts.Year)} {parts.Hour:00}:{parts.Minute:00}:{parts.Second:00} GMT";
        }

        /// <summary>
        /// Convert to JSON using the ECMAScript invalid-date null contract
        /// </summary>
        public string? toJSON() => double.IsFinite(_milliseconds) ? toISOString() : null;

        /// <summary>
        /// Convert to locale date string
        /// </summary>
        public string toLocaleDateString() => _value.LocalDateTime.ToShortDateString();

        /// <summary>
        /// Convert to locale time string
        /// </summary>
        public string toLocaleTimeString() => _value.LocalDateTime.ToShortTimeString();

        /// <summary>
        /// Convert to locale string
        /// </summary>
        public string toLocaleString() => _value.LocalDateTime.ToString();

        // ==================== Primitive Value ====================

        /// <summary>
        /// Get primitive value (milliseconds since epoch)
        /// </summary>
        public double valueOf() => getTime();
    }
}
