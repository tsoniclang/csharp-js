import { overloads as O } from "@tsonic/core/lang.js";
import type { int } from "@tsonic/core/types.js";
import { Convert, Double, Uri } from "@tsonic/dotnet/System.js";
import {
  CultureInfo,
  NumberStyles,
} from "@tsonic/dotnet/System.Globalization.js";
import { BigInteger } from "@tsonic/dotnet/System.Numerics.js";
import { RangeError } from "./range-error.js";

const nan = (): number => Double.NaN;
export const NaN: number = Double.NaN;
export const Infinity: number = Double.PositiveInfinity;
const MAX_SAFE_INTEGER: number = 9_007_199_254_740_991;
const MIN_SAFE_INTEGER: number = MAX_SAFE_INTEGER * -1;
type TsonicJsNumberInput =
  | string
  | number
  | boolean
  | null
  | undefined;
type TsonicJsStringInput = string | number | bigint | boolean | object | null;
type TsonicJsBooleanInput =
  | string
  | number
  | boolean
  | object
  | null
  | undefined;

const digitValue = (ch: string): number => {
  if (ch >= "0" && ch <= "9") {
    return ch.charCodeAt(0) - "0".charCodeAt(0);
  }
  if (ch >= "a" && ch <= "z") {
    return ch.charCodeAt(0) - "a".charCodeAt(0) + 10;
  }
  if (ch >= "A" && ch <= "Z") {
    return ch.charCodeAt(0) - "A".charCodeAt(0) + 10;
  }
  return -1;
};

const asJsBigInt = (value: BigInteger): bigint => value as unknown as bigint;

const parseBigIntDigits = (
  digits: string,
  radix: number,
  sign: number
): bigint => {
  let result = BigInteger.Zero;
  const radixBig = new BigInteger(radix);

  for (let index = 0; index < digits.length; index += 1) {
    const digit = digitValue(digits[index]!);
    if (digit < 0 || digit >= radix) {
      throw new Error("Cannot convert value to a BigInt");
    }
    result = BigInteger.Add(
      BigInteger.Multiply(result, radixBig),
      new BigInteger(digit)
    );
  }

  return asJsBigInt(sign < 0 ? BigInteger.Negate(result) : result);
};

const parseBigIntString = (value: string): bigint => {
  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return asJsBigInt(BigInteger.Zero);
  }

  let sign = 1;
  let index = 0;
  if (trimmed[index] === "+" || trimmed[index] === "-") {
    sign = trimmed[index] === "-" ? -1 : 1;
    index += 1;
  }

  let radix = 10;
  if (trimmed.slice(index, index + 2).toLowerCase() === "0x") {
    radix = 16;
    index += 2;
  } else if (trimmed.slice(index, index + 2).toLowerCase() === "0o") {
    radix = 8;
    index += 2;
  } else if (trimmed.slice(index, index + 2).toLowerCase() === "0b") {
    radix = 2;
    index += 2;
  }

  const digits = trimmed.slice(index);
  if (digits.length === 0) {
    throw new Error("Cannot convert value to a BigInt");
  }

  return parseBigIntDigits(digits, radix, sign);
};

export const parseInt = (value: string, radix?: number): number => {
  const trimmed = value.trimStart();
  if (trimmed.length === 0) {
    return nan();
  }

  let index = 0;
  let sign: number = 1;
  if (trimmed[index] === "+" || trimmed[index] === "-") {
    sign = trimmed[index] === "-" ? -1 : 1;
    index += 1;
  }

  let actualRadix = radix ?? 0;
  if (actualRadix !== 0 && (actualRadix < 2 || actualRadix > 36)) {
    return nan();
  }

  if (
    actualRadix === 0 &&
    trimmed.slice(index, index + 2).toLowerCase() === "0x"
  ) {
    actualRadix = 16;
    index += 2;
  } else if (actualRadix === 0) {
    actualRadix = 10;
  } else if (
    actualRadix === 16 &&
    trimmed.slice(index, index + 2).toLowerCase() === "0x"
  ) {
    index += 2;
  }

  let parsed: number = 0;
  let sawDigit = false;

  while (index < trimmed.length) {
    const digit = digitValue(trimmed[index]!);
    if (digit < 0 || digit >= actualRadix) {
      break;
    }
    parsed = parsed * actualRadix + digit;
    sawDigit = true;
    index += 1;
  }

  return sawDigit ? sign * parsed : nan();
};

export const parseFloat = (value: string): number => {
  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return nan();
  }

  try {
    return Double.Parse(
      trimmed,
      NumberStyles.Float,
      CultureInfo.InvariantCulture,
    );
  } catch {
    return nan();
  }
};

export const isFinite = (value: number): boolean => Double.IsFinite(value);

export const isInteger = (value: number): boolean =>
  Double.IsFinite(value) && value % 1 === 0;

export const isNaN = (value: number): boolean => Double.IsNaN(value);

export const isSafeInteger = (value: number): boolean =>
  isInteger(value) && value <= MAX_SAFE_INTEGER && value >= MIN_SAFE_INTEGER;

export function Number(value?: TsonicJsNumberInput): number;
export function Number(value: bigint): number;
export function Number(_value?: any): any {
  throw new Error("Unreachable overload stub");
}

export function Number_default(value?: TsonicJsNumberInput): number {
  if (value === undefined || value === null) {
    return 0;
  }

  if (typeof value === "number") {
    return value;
  }

  if (typeof value === "boolean") {
    return value ? 1 : 0;
  }

  const trimmed = value.trim();
  if (trimmed.length === 0) {
    return 0;
  }

  try {
    return Double.Parse(
      trimmed,
      NumberStyles.Float,
      CultureInfo.InvariantCulture,
    );
  } catch {
    return nan();
  }
}

export function Number_bigint(value: bigint): number {
  return value as unknown as number;
}

export const String = (value?: TsonicJsStringInput): string => {
  if (value === undefined || value === null) {
    return "";
  }

  if (typeof value === "string") {
    return value;
  }

  if (typeof value === "boolean") {
    return value ? "true" : "false";
  }

  if (typeof value === "number") {
    return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
  }

  if (typeof value === "bigint") {
    return Convert.ToString(value as unknown as object) ?? "";
  }

  return Convert.ToString(value) ?? "";
};

export function BigInt(value: string): bigint;
export function BigInt(value: int): bigint;
export function BigInt(value: number): bigint;
export function BigInt(value: bigint): bigint;
export function BigInt(value: boolean): bigint;
export function BigInt(_value: any): any {
  throw new Error("Unreachable overload stub");
}

export function BigInt_string(value: string): bigint {
  return parseBigIntString(value);
}

export function BigInt_int(value: int): bigint {
  return asJsBigInt(new BigInteger(value));
}

export function BigInt_number(value: number): bigint {
  if (!isInteger(value)) {
    throw new RangeError("The number is not an integer");
  }
  return asJsBigInt(new BigInteger(value));
}

export function BigInt_bigint(value: bigint): bigint {
  return value;
}

export function BigInt_boolean(value: boolean): bigint {
  return asJsBigInt(value ? BigInteger.One : BigInteger.Zero);
}

export function Boolean(value?: TsonicJsBooleanInput): boolean;
export function Boolean(value: bigint): boolean;
export function Boolean(_value?: any): any {
  throw new Error("Unreachable overload stub");
}

export function Boolean_default(value?: TsonicJsBooleanInput): boolean {
  if (value === undefined || value === null) {
    return false;
  }

  if (typeof value === "boolean") {
    return value;
  }

  if (typeof value === "number") {
    return value !== 0 && !Double.IsNaN(value);
  }

  if (typeof value === "string") {
    const text = value as string;
    return text.length > 0;
  }

  return true;
}

export function Boolean_bigint(value: bigint): boolean {
  return value !== asJsBigInt(BigInteger.Zero);
}

export const encodeURIComponent = (component: string): string =>
  Uri.EscapeDataString(component);

export const decodeURIComponent = (component: string): string =>
  Uri.UnescapeDataString(component);

export const encodeURI = (uri: string): string => {
  let encoded = Uri.EscapeDataString(uri);
  encoded = encoded.replaceAll("%3A", ":");
  encoded = encoded.replaceAll("%2F", "/");
  encoded = encoded.replaceAll("%3F", "?");
  encoded = encoded.replaceAll("%23", "#");
  encoded = encoded.replaceAll("%5B", "[");
  encoded = encoded.replaceAll("%5D", "]");
  encoded = encoded.replaceAll("%40", "@");
  encoded = encoded.replaceAll("%21", "!");
  encoded = encoded.replaceAll("%24", "$");
  encoded = encoded.replaceAll("%26", "&");
  encoded = encoded.replaceAll("%27", "'");
  encoded = encoded.replaceAll("%28", "(");
  encoded = encoded.replaceAll("%29", ")");
  encoded = encoded.replaceAll("%2A", "*");
  encoded = encoded.replaceAll("%2B", "+");
  encoded = encoded.replaceAll("%2C", ",");
  encoded = encoded.replaceAll("%3B", ";");
  encoded = encoded.replaceAll("%3D", "=");
  return encoded;
};

export const decodeURI = (uri: string): string => Uri.UnescapeDataString(uri);

O(Number_default).family(Number);
O(Number_bigint).family(Number);
O(Boolean_default).family(Boolean);
O(Boolean_bigint).family(Boolean);
O(BigInt_string).family(BigInt);
O(BigInt_int).family(BigInt);
O(BigInt_number).family(BigInt);
O(BigInt_bigint).family(BigInt);
O(BigInt_boolean).family(BigInt);
