import type { int, long } from "@tsonic/core/types.js";
import { DateTimeOffset, Double, TimeSpan } from "@tsonic/dotnet/System.js";

const unixEpoch = DateTimeOffset.UnixEpoch;

export const now = (): long => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

export const parse = (value: string): number => {
  try {
    const parsed = DateTimeOffset.Parse(value);
    return parsed.Subtract(unixEpoch).TotalMilliseconds;
  } catch {
    return Double.NaN;
  }
};

export const UTC = (
  year: int,
  month: int,
  day: int = 1 as int,
  hours: int = 0 as int,
  minutes: int = 0 as int,
  seconds: int = 0 as int,
  milliseconds: int = 0 as int
): number => {
  try {
    const offset = TimeSpan.Zero;
    const value = new DateTimeOffset(
      year,
      month + (1 as int),
      day,
      hours,
      minutes,
      seconds,
      milliseconds,
      offset
    );
    return value.Subtract(unixEpoch).TotalMilliseconds;
  } catch {
    return Double.NaN;
  }
};
