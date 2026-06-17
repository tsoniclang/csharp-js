import { asinterface } from "@tsonic/core/lang.js";
import type { char, int } from "@tsonic/core/types.js";
import type { IReadOnlyList } from "@tsonic/dotnet/System.Collections.Generic.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import {
  Char,
  Convert,
  Double,
  Math as DotnetMath,
  String as DotnetString,
  StringComparison,
  StringSplitOptions,
} from "@tsonic/dotnet/System.js";
import {
  CompareOptions,
  CultureInfo,
} from "@tsonic/dotnet/System.Globalization.js";
import { NormalizationForm, StringBuilder } from "@tsonic/dotnet/System.Text.js";
import {
  Group,
  Match,
  Regex,
} from "@tsonic/dotnet/System.Text.RegularExpressions.js";
import { toInt } from "./int32.js";
import { RangeError } from "./range-error.js";
import { RegExp as JsRegExp } from "./regexp-object.js";
import { overloads as O } from "@tsonic/core/lang.js";

const asDotnetString = (value: string): DotnetString =>
  asinterface<DotnetString>(value);

const toRegex = (pattern: string | JsRegExp): Regex =>
  pattern instanceof JsRegExp ? pattern.regex : new Regex(pattern);

const toChars = (value: string): char[] => asDotnetString(value).ToCharArray();

type StringReplaceCallback = (
  match: string,
  ...captures: string[]
) => string;

const getLength = (value: string): int => {
  const chars = toChars(value);
  return chars.length;
};

const toJsInteger = (value: number): number =>
  Double.IsNaN(value) ? 0.0 : DotnetMath.Truncate(value);

const toUint16CodeUnit = (value: number): int => {
  const integer = toJsInteger(value);
  if (integer === 0 || Double.IsInfinity(integer)) {
    return 0 as int;
  }
  const modulo = integer % 65536;
  const normalized = modulo < 0 ? modulo + 65536 : modulo;
  return toInt(normalized);
};

const fromUtf16CodeUnit = (codeUnit: int): string => {
  let hex = Convert.ToString(codeUnit, 16);
  while (hex.length < 4) {
    hex = "0" + hex;
  }
  return Regex.Unescape("\\u" + hex);
};

const toValidCodePoint = (value: number): int => {
  const integer = toJsInteger(value);
  if (
    integer !== value ||
    !Double.IsFinite(integer) ||
    integer < 0 ||
    integer > 1114111
  ) {
    throw new RangeError("Invalid code point");
  }
  return toInt(integer);
};

const clampJsInteger = (
  value: number,
  minimum: int,
  maximum: int
): int => {
  const integer = toJsInteger(value);
  if (Double.IsNegativeInfinity(integer) || integer < minimum) {
    return minimum;
  }
  if (Double.IsPositiveInfinity(integer) || integer > maximum) {
    return maximum;
  }
  return toInt(integer);
};

const normalizeRelativeIndex = (
  value: number | undefined,
  lengthValue: int,
  fallback: int
): int => {
  if (value === undefined) {
    return fallback;
  }
  const integer = toJsInteger(value);
  if (Double.IsNegativeInfinity(integer)) {
    return 0 as int;
  }
  if (Double.IsPositiveInfinity(integer)) {
    return lengthValue;
  }
  if (integer < 0) {
    const fromEnd = lengthValue + integer;
    return fromEnd < 0 ? (0 as int) : toInt(fromEnd);
  }
  return integer > lengthValue ? lengthValue : toInt(integer);
};

const normalizeStringElementIndex = (
  value: number,
  lengthValue: int,
  allowNegativeRelative: boolean
): int => {
  const integer = toJsInteger(value);
  if (
    Double.IsNegativeInfinity(integer) ||
    Double.IsPositiveInfinity(integer)
  ) {
    return -1 as int;
  }
  const actual = allowNegativeRelative && integer < 0
    ? lengthValue + integer
    : integer;
  if (actual < 0 || actual >= lengthValue) {
    return -1 as int;
  }
  return toInt(actual);
};

const normalizeForm = (form?: string): NormalizationForm => {
  switch (form) {
    case "NFD":
      return NormalizationForm.FormD;
    case "NFKC":
      return NormalizationForm.FormKC;
    case "NFKD":
      return NormalizationForm.FormKD;
    case "NFC":
    default:
      return NormalizationForm.FormC;
  }
};

const toSinglePadChar = (value: string): char => {
  if (getLength(value) === 0) {
    return Convert.ToChar(" ");
  }
  return Convert.ToChar(asDotnetString(value).Substring(0 as int, 1 as int));
};

export const length = (value: string): int => getLength(value);

export const at = (value: string, index: number): string => {
  const lengthValue = getLength(value);
  const actualIndex = normalizeStringElementIndex(index, lengthValue, true);
  if (actualIndex < 0) {
    return "";
  }
  return asDotnetString(value).Substring(actualIndex, 1 as int);
};

export const charAt = (value: string, index: number): string => {
  const actualIndex = normalizeStringElementIndex(index, getLength(value), false);
  if (actualIndex < 0) {
    return "";
  }
  return asDotnetString(value).Substring(actualIndex, 1 as int);
};

export const charCodeAt = (value: string, index: number): int => {
  const actualIndex = normalizeStringElementIndex(index, getLength(value), false);
  return actualIndex < 0 ? (-1 as int) : Char.ConvertToUtf32(value, actualIndex);
};

export const codePointAt = (value: string, index: number): int => {
  const actualIndex = normalizeStringElementIndex(index, getLength(value), false);
  if (actualIndex < 0) {
    return -1 as int;
  }
  return Char.ConvertToUtf32(value, actualIndex);
};

export const concat = (value: string, ...strings: string[]): string => {
  let result = value;
  for (let i = 0; i < strings.length; i += 1) {
    result += strings[i]!;
  }
  return result;
};

export const endsWith = (value: string, searchString: string): boolean =>
  asDotnetString(value).EndsWith(searchString);

export const includes = (value: string, searchString: string): boolean =>
  asDotnetString(value).Contains(searchString);

export const indexOf = (
  value: string,
  searchString: string,
  position: number = 0
): int =>
  asDotnetString(value).IndexOf(
    searchString,
    clampJsInteger(position, 0 as int, getLength(value))
  );

export const isWellFormed = (value: string): boolean => {
  const chars = toChars(value);

  for (let i = 0 as int; i < chars.length; i += 1) {
    const current = chars[i]!;
    if (Char.IsSurrogate(current)) {
      if (Char.IsHighSurrogate(current)) {
        if (i + 1 >= chars.length || !Char.IsLowSurrogate(chars[i + 1]!)) {
          return false;
        }
        i += 1;
      } else {
        return false;
      }
    }
  }

  return true;
};

export const lastIndexOf = (
  value: string,
  searchString: string,
  position?: number
): int => {
  if (position === undefined || Double.IsNaN(position)) {
    return asDotnetString(value).LastIndexOf(searchString);
  }
  const actualPosition = clampJsInteger(position, 0 as int, getLength(value));
  return actualPosition >= getLength(value)
    ? asDotnetString(value).LastIndexOf(searchString)
    : asDotnetString(value).LastIndexOf(searchString, actualPosition);
};

export const localeCompare = (value: string, compareString: string): int =>
  CultureInfo.CurrentCulture.CompareInfo.Compare(
    value,
    compareString,
    CompareOptions.None
  );

export const match = (
  value: string,
  pattern: string | JsRegExp
): string[] | undefined => {
  const regex = toRegex(pattern);
  const matched = regex.Match(value);
  if (!matched.Success) {
    return undefined;
  }
  const result = new List<string>();
  const groups = matched.Groups.GetEnumerator();
  while (groups.MoveNext()) {
    result.Add((groups.Current as Group).Value);
  }
  return result.ToArray();
};

export const matchAll = (
  value: string,
  pattern: string | JsRegExp
): string[][] => {
  const regex = toRegex(pattern);
  const matches = regex.Matches(value);
  const results = new List<string[]>();
  for (let i = 0; i < matches.Count; i += 1) {
    const match = matches[i]!;
    const groups = new List<string>();
    const matchGroups = match.Groups.GetEnumerator();
    while (matchGroups.MoveNext()) {
      groups.Add((matchGroups.Current as Group).Value);
    }
    results.Add(groups.ToArray());
  }
  return results.ToArray();
};

export const normalize = (value: string, form?: string): string =>
  asDotnetString(value).Normalize(normalizeForm(form));

export const padEnd = (
  value: string,
  targetLength: number,
  padString: string = " "
): string =>
  asDotnetString(value).PadRight(
    clampJsInteger(targetLength, 0 as int, 2147483647 as int),
    toSinglePadChar(padString)
  );

export const padStart = (
  value: string,
  targetLength: number,
  padString: string = " "
): string =>
  asDotnetString(value).PadLeft(
    clampJsInteger(targetLength, 0 as int, 2147483647 as int),
    toSinglePadChar(padString)
  );

export const repeat = (value: string, count: number): string => {
  const repeatCount = clampJsInteger(count, 0 as int, 2147483647 as int);
  let result = "";
  for (let i = 0 as int; i < repeatCount; i += 1) {
    result += value;
  }
  return result;
};

export function replace(
  value: string,
  searchValue: string | JsRegExp,
  replaceValue: string
): string;
export function replace(
  value: string,
  searchValue: string | JsRegExp,
  replaceValue: StringReplaceCallback
): string;
export function replace(
  _value: string,
  _searchValue: string | JsRegExp,
  _replaceValue: any
): any {
  throw new Error("Unreachable overload stub");
}

export function replace_string(
  value: string,
  searchValue: string | JsRegExp,
  replaceValue: string
): string {
  if (searchValue instanceof JsRegExp) {
    return searchValue.flags.indexOf("g") >= 0
      ? searchValue.regex.Replace(value, replaceValue)
      : searchValue.regex.Replace(value, replaceValue, 1 as int);
  }

  const index = asDotnetString(value).IndexOf(
    searchValue,
    StringComparison.Ordinal
  );
  if (index < 0) {
    return value;
  }
  return asDotnetString(
    asDotnetString(value).Remove(index, getLength(searchValue))
  ).Insert(index, replaceValue);
}

const evaluateReplacement = (
  replaceValue: StringReplaceCallback,
  match: Match
): string => {
  const captures = new List<string>();
  const groups = asinterface<IReadOnlyList<Group>>(match.Groups);
  for (let index = 1 as int; index < match.Groups.Count; index += 1) {
    captures.Add(groups[index]!.Value);
  }
  return replaceValue(match.Value, ...captures.ToArray());
};

export function replace_callback(
  value: string,
  searchValue: string | JsRegExp,
  replaceValue: StringReplaceCallback
): string {
  if (searchValue instanceof JsRegExp) {
    return searchValue.flags.indexOf("g") >= 0
      ? searchValue.regex.Replace(
          value,
          (match) => evaluateReplacement(replaceValue, match)
        )
      : searchValue.regex.Replace(
          value,
          (match) => evaluateReplacement(replaceValue, match),
          1 as int
        );
  }

  const index = asDotnetString(value).IndexOf(
    searchValue,
    StringComparison.Ordinal
  );
  if (index < 0) {
    return value;
  }
  return asDotnetString(
    asDotnetString(value).Remove(index, getLength(searchValue))
  ).Insert(index, replaceValue(searchValue));
}

export const replaceAll = (
  value: string,
  searchValue: string,
  replaceValue: string
): string => asDotnetString(value).Replace(searchValue, replaceValue);

export const search = (value: string, pattern: string | JsRegExp): int => {
  const regex = toRegex(pattern);
  const matched = regex.Match(value);
  return matched.Success ? matched.Index : (-1 as int);
};

export const slice = (
  value: string,
  start: number = 0,
  end?: number
): string => {
  const lengthValue = getLength(value);
  const actualStart = normalizeRelativeIndex(start, lengthValue, 0 as int);
  const actualEnd = normalizeRelativeIndex(end, lengthValue, lengthValue);
  const sliceLength: int =
    actualEnd > actualStart ? actualEnd - actualStart : (0 as int);

  return asDotnetString(value).Substring(actualStart, sliceLength);
};

export const split = (
  value: string,
  separator: string | JsRegExp,
  limit?: number
): string[] => {
  if (separator instanceof JsRegExp) {
    return limit === undefined
      ? separator.regex.Split(value)
      : separator.regex.Split(
          value,
          clampJsInteger(limit, 0 as int, 2147483647 as int)
        );
  }

  if (separator === "") {
    const chars = new List<string>();
    const actualLimit =
      limit === undefined
        ? getLength(value)
        : clampJsInteger(limit, 0 as int, getLength(value));
    const maxLength: int =
      actualLimit < getLength(value) ? actualLimit : getLength(value);
    for (let i = 0 as int; i < maxLength; i += 1) {
      chars.Add(charAt(value, i));
    }
    return chars.ToArray();
  }

  return limit === undefined
    ? asDotnetString(value).Split(separator, StringSplitOptions.None)
    : asDotnetString(value).Split(
        separator,
        clampJsInteger(limit, 0 as int, 2147483647 as int),
        StringSplitOptions.None
      );
};

export const startsWith = (value: string, searchString: string): boolean =>
  asDotnetString(value).StartsWith(searchString);

export const substr = (
  value: string,
  start: number,
  substringLength?: number
): string => {
  const lengthValue = getLength(value);
  const normalizedStart = toJsInteger(start);
  const actualStart =
    normalizedStart < 0
      ? clampJsInteger(lengthValue + normalizedStart, 0 as int, lengthValue)
      : clampJsInteger(normalizedStart, 0 as int, lengthValue);
  const actualLength =
    substringLength === undefined
      ? lengthValue - actualStart
      : clampJsInteger(substringLength, 0 as int, lengthValue - actualStart);

  return asDotnetString(value).Substring(actualStart, actualLength);
};

export const substring = (
  value: string,
  start: number,
  end?: number
): string => {
  const lengthValue = getLength(value);
  const actualStart = clampJsInteger(start, 0 as int, lengthValue);
  const actualEnd =
    end === undefined
      ? lengthValue
      : clampJsInteger(end, 0 as int, lengthValue);
  const lower = actualStart < actualEnd ? actualStart : actualEnd;
  const upper = actualStart > actualEnd ? actualStart : actualEnd;
  return asDotnetString(value).Substring(lower, upper - lower);
};

export const toLocaleLowerCase = (value: string): string =>
  asDotnetString(value).ToLower(CultureInfo.CurrentCulture);

export const toLocaleUpperCase = (value: string): string =>
  asDotnetString(value).ToUpper(CultureInfo.CurrentCulture);

export const toLowerCase = (value: string): string =>
  asDotnetString(value).ToLower();

export const toString = (value: string): string => value;

export const trim = (value: string): string => asDotnetString(value).Trim();

export const trimLeft = (value: string): string => trimStart(value);

export const trimRight = (value: string): string => trimEnd(value);

export const trimStart = (value: string): string =>
  asDotnetString(value).TrimStart();

export const trimEnd = (value: string): string =>
  asDotnetString(value).TrimEnd();

export const toUpperCase = (value: string): string =>
  asDotnetString(value).ToUpper();

export const toWellFormed = (value: string): string => {
  const builder = new StringBuilder();
  const chars = toChars(value);

  for (let i = 0 as int; i < chars.length; i += 1) {
    const current = chars[i]!;
    if (Char.IsSurrogate(current)) {
      if (Char.IsHighSurrogate(current)) {
        if (i + 1 < chars.length && Char.IsLowSurrogate(chars[i + 1]!)) {
          builder.Append(current);
          builder.Append(chars[i + 1]!);
          i += 1;
        } else {
          builder.Append("\uFFFD");
        }
      } else {
        builder.Append("\uFFFD");
      }
    } else {
      builder.Append(current);
    }
  }

  return builder.ToString();
};

export const valueOf = (value: string): string => value;

export const fromCharCode = (...codes: number[]): string => {
  let result = "";
  for (let i = 0; i < codes.length; i += 1) {
    result += fromUtf16CodeUnit(toUint16CodeUnit(codes[i]!));
  }
  return result;
};

export const fromCodePoint = (...codePoints: number[]): string => {
  let result = "";
  for (let i = 0; i < codePoints.length; i += 1) {
    result += Char.ConvertFromUtf32(toValidCodePoint(codePoints[i]!));
  }
  return result;
};

export class StringConstructor {
  static fromCharCode(...codes: number[]): string {
    return fromCharCode(...codes);
  }

  static fromCodePoint(...codePoints: number[]): string {
    return fromCodePoint(...codePoints);
  }
}

O(replace_string).family(replace);
O(replace_callback).family(replace);

export const raw = (
  template: List<string>,
  ...substitutions: (string | number | boolean | object | null)[]
): string => {
  const builder = new StringBuilder();

  for (let i = 0 as int; i < template.Count; i += 1) {
    builder.Append(template[i]!);
    if (i < substitutions.length) {
      builder.Append(`${substitutions[i]!}`);
    }
  }

  return builder.ToString();
};
