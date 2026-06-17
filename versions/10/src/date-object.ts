import {
  Convert,
  DateTime,
  DateTimeOffset,
  Double,
  TimeSpan,
  TimeZoneInfo,
} from "@tsonic/dotnet/System.js";
import {
  CultureInfo,
  DateTimeStyles,
} from "@tsonic/dotnet/System.Globalization.js";
import type { int, long } from "@tsonic/core/types.js";

const epoch = new DateTimeOffset(
  1970 as int,
  1 as int,
  1 as int,
  0 as int,
  0 as int,
  0 as int,
  TimeSpan.Zero,
);

const invalidDate = (): DateTimeOffset => DateTimeOffset.MinValue;

const getLocalOffset = (): TimeSpan =>
  TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

export class Date {
  value: DateTimeOffset;

  constructor(
    valueOrYear?: number | string,
    month?: number,
    day?: number,
    hours?: number,
    minutes?: number,
    seconds?: number,
    milliseconds?: number,
  ) {
    if (valueOrYear === undefined) {
      this.value = DateTimeOffset.Now;
      return;
    }

    if (typeof valueOrYear === "string") {
      const parsed = Date.parse(valueOrYear);
      if (Double.IsNaN(parsed) || Double.IsInfinity(parsed)) {
        this.value = invalidDate();
      } else {
        this.value = epoch.AddMilliseconds(parsed);
      }
      return;
    }

    if (month === undefined) {
      if (Double.IsNaN(valueOrYear) || Double.IsInfinity(valueOrYear)) {
        this.value = invalidDate();
      } else {
        this.value = epoch.AddMilliseconds(valueOrYear);
      }
      return;
    }

    try {
      const actualDay = Convert.ToInt32(day ?? 1);
      const actualHours = Convert.ToInt32(hours ?? 0);
      const actualMinutes = Convert.ToInt32(minutes ?? 0);
      const actualSeconds = Convert.ToInt32(seconds ?? 0);
      const actualMilliseconds = Convert.ToInt32(milliseconds ?? 0);
      this.value = new DateTimeOffset(
        Convert.ToInt32(valueOrYear),
        Convert.ToInt32(month + 1),
        actualDay,
        actualHours,
        actualMinutes,
        actualSeconds,
        actualMilliseconds,
        getLocalOffset(),
      );
    } catch {
      this.value = invalidDate();
    }
  }

  static now(): long {
    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }

  static parse(dateString: string): number {
    try {
      const parsed = DateTimeOffset.Parse(
        dateString,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
      );
      return parsed.Subtract(epoch).TotalMilliseconds;
    } catch {
      return Double.NaN;
    }
  }

  static UTC(
    year: int,
    month: int,
    day: int = 1 as int,
    hours: int = 0 as int,
    minutes: int = 0 as int,
    seconds: int = 0 as int,
    milliseconds: int = 0 as int,
  ): number {
    try {
      const value = new DateTimeOffset(
        year,
        (month + 1) as int,
        day,
        hours,
        minutes,
        seconds,
        milliseconds,
        TimeSpan.Zero,
      );
      return value.Subtract(epoch).TotalMilliseconds;
    } catch {
      return Double.NaN;
    }
  }

  getTime(): long {
    return Convert.ToInt64(this.value.Subtract(epoch).TotalMilliseconds);
  }

  getFullYear(): int {
    return this.value.LocalDateTime.Year;
  }

  getMonth(): int {
    return (this.value.LocalDateTime.Month - 1) as int;
  }

  getDate(): int {
    return this.value.LocalDateTime.Day;
  }

  getDay(): int {
    return Convert.ToInt32(this.value.LocalDateTime.DayOfWeek);
  }

  getHours(): int {
    return this.value.LocalDateTime.Hour;
  }

  getMinutes(): int {
    return this.value.LocalDateTime.Minute;
  }

  getSeconds(): int {
    return this.value.LocalDateTime.Second;
  }

  getMilliseconds(): int {
    return this.value.LocalDateTime.Millisecond;
  }

  getTimezoneOffset(): int {
    return Convert.ToInt32(-this.value.Offset.TotalMinutes);
  }

  getUTCFullYear(): int {
    return this.value.UtcDateTime.Year;
  }

  getUTCMonth(): int {
    return (this.value.UtcDateTime.Month - 1) as int;
  }

  getUTCDate(): int {
    return this.value.UtcDateTime.Day;
  }

  getUTCDay(): int {
    return Convert.ToInt32(this.value.UtcDateTime.DayOfWeek);
  }

  getUTCHours(): int {
    return this.value.UtcDateTime.Hour;
  }

  getUTCMinutes(): int {
    return this.value.UtcDateTime.Minute;
  }

  getUTCSeconds(): int {
    return this.value.UtcDateTime.Second;
  }

  getUTCMilliseconds(): int {
    return this.value.UtcDateTime.Millisecond;
  }

  setTime(milliseconds: number): number {
    this.value = epoch.AddMilliseconds(milliseconds);
    return this.getTime();
  }

  setMilliseconds(ms: int): number {
    const local = this.value.LocalDateTime;
    this.value = new DateTimeOffset(
      local.Year,
      local.Month,
      local.Day,
      local.Hour,
      local.Minute,
      local.Second,
      ms,
      this.value.Offset,
    );
    return this.getTime();
  }

  setSeconds(sec: int, ms?: int): number {
    const local = this.value.LocalDateTime;
    const nextMs = ms ?? local.Millisecond;
    this.value = new DateTimeOffset(
      local.Year,
      local.Month,
      local.Day,
      local.Hour,
      local.Minute,
      sec,
      nextMs,
      this.value.Offset,
    );
    return this.getTime();
  }

  setMinutes(min: int, sec?: int, ms?: int): number {
    const local = this.value.LocalDateTime;
    const nextSec = sec ?? local.Second;
    const nextMs = ms ?? local.Millisecond;
    this.value = new DateTimeOffset(
      local.Year,
      local.Month,
      local.Day,
      local.Hour,
      min,
      nextSec,
      nextMs,
      this.value.Offset,
    );
    return this.getTime();
  }

  setHours(hour: int, min?: int, sec?: int, ms?: int): number {
    const local = this.value.LocalDateTime;
    const nextMin = min ?? local.Minute;
    const nextSec = sec ?? local.Second;
    const nextMs = ms ?? local.Millisecond;
    this.value = new DateTimeOffset(
      local.Year,
      local.Month,
      local.Day,
      hour,
      nextMin,
      nextSec,
      nextMs,
      this.value.Offset,
    );
    return this.getTime();
  }

  setDate(day: int): number {
    const local = this.value.LocalDateTime;
    this.value = new DateTimeOffset(
      local.Year,
      local.Month,
      day,
      local.Hour,
      local.Minute,
      local.Second,
      local.Millisecond,
      this.value.Offset,
    );
    return this.getTime();
  }

  setMonth(month: int, day?: int): number {
    const local = this.value.LocalDateTime;
    const nextDay = day ?? local.Day;
    this.value = new DateTimeOffset(
      local.Year,
      (month + 1) as int,
      nextDay,
      local.Hour,
      local.Minute,
      local.Second,
      local.Millisecond,
      this.value.Offset,
    );
    return this.getTime();
  }

  setFullYear(year: int, month?: int, day?: int): number {
    const local = this.value.LocalDateTime;
    const nextMonth = month === undefined ? local.Month : ((month + 1) as int);
    const nextDay = day ?? local.Day;
    this.value = new DateTimeOffset(
      year,
      nextMonth,
      nextDay,
      local.Hour,
      local.Minute,
      local.Second,
      local.Millisecond,
      this.value.Offset,
    );
    return this.getTime();
  }

  setUTCMilliseconds(ms: int): number {
    const utc = this.value.UtcDateTime;
    this.value = new DateTimeOffset(
      utc.Year,
      utc.Month,
      utc.Day,
      utc.Hour,
      utc.Minute,
      utc.Second,
      ms,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCSeconds(sec: int, ms?: int): number {
    const utc = this.value.UtcDateTime;
    const nextMs = ms ?? utc.Millisecond;
    this.value = new DateTimeOffset(
      utc.Year,
      utc.Month,
      utc.Day,
      utc.Hour,
      utc.Minute,
      sec,
      nextMs,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCMinutes(min: int, sec?: int, ms?: int): number {
    const utc = this.value.UtcDateTime;
    const nextSec = sec ?? utc.Second;
    const nextMs = ms ?? utc.Millisecond;
    this.value = new DateTimeOffset(
      utc.Year,
      utc.Month,
      utc.Day,
      utc.Hour,
      min,
      nextSec,
      nextMs,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCHours(hour: int, min?: int, sec?: int, ms?: int): number {
    const utc = this.value.UtcDateTime;
    const nextMin = min ?? utc.Minute;
    const nextSec = sec ?? utc.Second;
    const nextMs = ms ?? utc.Millisecond;
    this.value = new DateTimeOffset(
      utc.Year,
      utc.Month,
      utc.Day,
      hour,
      nextMin,
      nextSec,
      nextMs,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCDate(day: int): number {
    const utc = this.value.UtcDateTime;
    this.value = new DateTimeOffset(
      utc.Year,
      utc.Month,
      day,
      utc.Hour,
      utc.Minute,
      utc.Second,
      utc.Millisecond,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCMonth(month: int, day?: int): number {
    const utc = this.value.UtcDateTime;
    const nextDay = day ?? utc.Day;
    this.value = new DateTimeOffset(
      utc.Year,
      (month + 1) as int,
      nextDay,
      utc.Hour,
      utc.Minute,
      utc.Second,
      utc.Millisecond,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  setUTCFullYear(year: int, month?: int, day?: int): number {
    const utc = this.value.UtcDateTime;
    const nextMonth = month === undefined ? utc.Month : ((month + 1) as int);
    const nextDay = day ?? utc.Day;
    this.value = new DateTimeOffset(
      year,
      nextMonth,
      nextDay,
      utc.Hour,
      utc.Minute,
      utc.Second,
      utc.Millisecond,
      TimeSpan.Zero,
    );
    return this.getTime();
  }

  toString(): string {
    return this.value.LocalDateTime.ToString(
      "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz",
      CultureInfo.InvariantCulture,
    );
  }

  toDateString(): string {
    return this.value.LocalDateTime.ToString(
      "ddd MMM dd yyyy",
      CultureInfo.InvariantCulture,
    );
  }

  toTimeString(): string {
    return this.value.LocalDateTime.ToString(
      "HH:mm:ss 'GMT'zzz",
      CultureInfo.InvariantCulture,
    );
  }

  toISOString(): string {
    return this.value.UtcDateTime.ToString(
      "yyyy-MM-ddTHH:mm:ss.fffZ",
      CultureInfo.InvariantCulture,
    );
  }

  toUTCString(): string {
    return this.value.UtcDateTime.ToString(
      "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
      CultureInfo.InvariantCulture,
    );
  }

  toJSON(): string {
    return this.toISOString();
  }

  toLocaleDateString(): string {
    return this.value.LocalDateTime.ToShortDateString();
  }

  toLocaleTimeString(): string {
    return this.value.LocalDateTime.ToShortTimeString();
  }

  toLocaleString(): string {
    return this.value.LocalDateTime.ToString();
  }

  valueOf(): long {
    return this.getTime();
  }
}
