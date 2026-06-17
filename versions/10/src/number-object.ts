import {
  isFinite,
  isInteger,
  isNaN,
  isSafeInteger,
  parseFloat,
  parseInt,
} from "./Globals.js";
import { Convert, Double } from "@tsonic/dotnet/System.js";
import { CultureInfo } from "@tsonic/dotnet/System.Globalization.js";

const MAX_SAFE_INTEGER = 9_007_199_254_740_991;
const MIN_SAFE_INTEGER = -9_007_199_254_740_991;
const EPSILON = 2.220446049250313e-16;
const MAX_VALUE = Double.MaxValue;
const MIN_VALUE = Double.Epsilon;
const POSITIVE_INFINITY = Double.PositiveInfinity;
const NEGATIVE_INFINITY = Double.NegativeInfinity;
const NaN = Double.NaN;

export class Number {
  static EPSILON = EPSILON;
  static MAX_SAFE_INTEGER = MAX_SAFE_INTEGER;
  static MAX_VALUE = MAX_VALUE;
  static MIN_SAFE_INTEGER = MIN_SAFE_INTEGER;
  static MIN_VALUE = MIN_VALUE;
  static NEGATIVE_INFINITY = NEGATIVE_INFINITY;
  static POSITIVE_INFINITY = POSITIVE_INFINITY;
  static NaN = NaN;

  static isFinite(value: number): boolean {
    return isFinite(value);
  }

  static isInteger(value: number): boolean {
    return isInteger(value);
  }

  static isNaN(value: number): boolean {
    return isNaN(value);
  }

  static isSafeInteger(value: number): boolean {
    return isSafeInteger(value);
  }

  static parseFloat(value: string): number {
    return parseFloat(value);
  }

  static parseInt(value: string, radix?: number): number {
    return parseInt(value, radix);
  }

  static toString(value: number): string {
    return (value as Double).ToString(CultureInfo.InvariantCulture) ?? "";
  }

  static valueOf(value: number): number {
    return value;
  }
}
