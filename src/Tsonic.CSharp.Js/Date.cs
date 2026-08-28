/**
 * JavaScript Date implementation
 * Wraps System.DateTimeOffset with JavaScript Date semantics
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

        // Unix epoch: January 1, 1970 00:00:00 UTC
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // ==================== Constructors ====================

        /// <summary>
        /// Create Date with current time
        /// </summary>
        public Date()
        {
            _value = DateTimeOffset.Now;
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
                    _value = DateTimeOffset.MinValue;
                    break;
            }
        }

        private void SetFromMilliseconds(double milliseconds)
        {
            if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds))
            {
                _value = DateTimeOffset.MinValue;
            }
            else
            {
                _value = Epoch.AddMilliseconds(milliseconds);
            }
        }

        private void SetFromString(string dateString)
        {
            if (DateTimeOffset.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                _value = parsed;
            }
            else
            {
                _value = DateTimeOffset.MinValue;
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
            }
            catch
            {
                _value = DateTimeOffset.MinValue;
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
            try
            {
                var date = new DateTimeOffset(year, month + 1, day, hours, minutes, seconds, milliseconds, TimeSpan.Zero);
                return (date - Epoch).TotalMilliseconds;
            }
            catch
            {
                return double.NaN;
            }
        }

        public static string call() => new Date().ToString();

        // ==================== Instance Methods - Getters (Local Time) ====================

        /// <summary>
        /// Get milliseconds since epoch
        /// </summary>
        public double getTime() => (_value - Epoch).TotalMilliseconds;

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
        public double getUTCFullYear() => _value.UtcDateTime.Year;

        /// <summary>
        /// Get UTC month (0-11)
        /// </summary>
        public double getUTCMonth() => _value.UtcDateTime.Month - 1;

        /// <summary>
        /// Get UTC day of month
        /// </summary>
        public double getUTCDate() => _value.UtcDateTime.Day;

        /// <summary>
        /// Get UTC day of week
        /// </summary>
        public double getUTCDay() => (int)_value.UtcDateTime.DayOfWeek;

        /// <summary>
        /// Get UTC hours
        /// </summary>
        public double getUTCHours() => _value.UtcDateTime.Hour;

        /// <summary>
        /// Get UTC minutes
        /// </summary>
        public double getUTCMinutes() => _value.UtcDateTime.Minute;

        /// <summary>
        /// Get UTC seconds
        /// </summary>
        public double getUTCSeconds() => _value.UtcDateTime.Second;

        /// <summary>
        /// Get UTC milliseconds
        /// </summary>
        public double getUTCMilliseconds() => _value.UtcDateTime.Millisecond;

        // ==================== Instance Methods - Setters (Local Time) ====================

        /// <summary>
        /// Set time in milliseconds since epoch
        /// </summary>
        public double setTime(double milliseconds)
        {
            _value = Epoch.AddMilliseconds(milliseconds);
            return getTime();
        }

        /// <summary>
        /// Set milliseconds
        /// </summary>
        public double setMilliseconds(int ms)
        {
            var local = _value.LocalDateTime;
            _value = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, local.Minute, local.Second, ms, _value.Offset);
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
            return getTime();
        }

        /// <summary>
        /// Set day of month
        /// </summary>
        public double setDate(int day)
        {
            var local = _value.LocalDateTime;
            _value = new DateTimeOffset(local.Year, local.Month, day, local.Hour, local.Minute, local.Second, local.Millisecond, _value.Offset);
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
            var utc = _value.UtcDateTime;
            var selectedYear = DateInteger(year ?? utc.Year, nameof(year));
            var selectedMonth = DateInteger(month ?? (utc.Month - 1), nameof(month));
            var selectedDate = DateInteger(date ?? utc.Day, nameof(date));
            var selectedHours = DateInteger(hours ?? utc.Hour, nameof(hours));
            var selectedMinutes = DateInteger(minutes ?? utc.Minute, nameof(minutes));
            var selectedSeconds = DateInteger(seconds ?? utc.Second, nameof(seconds));
            var selectedMilliseconds = DateInteger(milliseconds ?? utc.Millisecond, nameof(milliseconds));
            _value = new DateTimeOffset(selectedYear, 1, 1, 0, 0, 0, TimeSpan.Zero)
                .AddMonths(selectedMonth)
                .AddDays(selectedDate - 1)
                .AddHours(selectedHours)
                .AddMinutes(selectedMinutes)
                .AddSeconds(selectedSeconds)
                .AddMilliseconds(selectedMilliseconds);
            return getTime();
        }

        private static int DateInteger(double value, string parameterName)
        {
            if (!double.IsFinite(value) || value < int.MinValue || value > int.MaxValue)
                throw new RangeError($"Date component '{parameterName}' is outside the supported finite range.");
            return checked((int)System.Math.Truncate(value));
        }

        // ==================== Instance Methods - String Conversion ====================

        /// <summary>
        /// Convert to string representation
        /// </summary>
        public override string ToString() => _value.LocalDateTime.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", CultureInfo.InvariantCulture);

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
        public string toISOString() => _value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        /// <summary>
        /// Convert to UTC string
        /// </summary>
        public string toUTCString() => _value.UtcDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);

        /// <summary>
        /// Convert to JSON (same as toISOString)
        /// </summary>
        public string toJSON() => toISOString();

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
