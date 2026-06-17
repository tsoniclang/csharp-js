import {
  E,
  LN2,
  LN10,
  LOG2E,
  LOG10E,
  PI,
  SQRT1_2,
  SQRT2,
  abs,
  acos,
  asin,
  atan,
  atan2,
  ceil,
  cos,
  exp,
  floor,
  log,
  log10,
  log2,
  max,
  min,
  pow,
  random,
  round,
  sign,
  sin,
  sqrt,
  tan,
  trunc,
} from "./math-intrinsics.js";

export class Math {
  static E = E;
  static PI = PI;
  static LN2 = LN2;
  static LN10 = LN10;
  static LOG2E = LOG2E;
  static LOG10E = LOG10E;
  static SQRT1_2 = SQRT1_2;
  static SQRT2 = SQRT2;

  static abs(value: number): number {
    return abs(value);
  }

  static acos(value: number): number {
    return acos(value);
  }

  static asin(value: number): number {
    return asin(value);
  }

  static atan(value: number): number {
    return atan(value);
  }

  static atan2(left: number, right: number): number {
    return atan2(left, right);
  }

  static ceil(value: number): number {
    return ceil(value);
  }

  static cos(value: number): number {
    return cos(value);
  }

  static exp(value: number): number {
    return exp(value);
  }

  static floor(value: number): number {
    return floor(value);
  }

  static log(value: number): number {
    return log(value);
  }

  static log10(value: number): number {
    return log10(value);
  }

  static log2(value: number): number {
    return log2(value);
  }

  static max(...values: number[]): number {
    return max(...values);
  }

  static min(...values: number[]): number {
    return min(...values);
  }

  static pow(left: number, right: number): number {
    return pow(left, right);
  }

  static random(): number {
    return random();
  }

  static round(value: number): number {
    return round(value);
  }

  static sign(value: number): number {
    return sign(value);
  }

  static sin(value: number): number {
    return sin(value);
  }

  static sqrt(value: number): number {
    return sqrt(value);
  }

  static tan(value: number): number {
    return tan(value);
  }

  static trunc(value: number): number {
    return trunc(value);
  }
}
