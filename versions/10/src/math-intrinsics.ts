import { Math as DotnetMath, Random } from "@tsonic/dotnet/System.js";

const randomSource = new Random();
const POSITIVE_INFINITY = DotnetMath.Exp(1000);
const NEGATIVE_INFINITY = -POSITIVE_INFINITY;

export const E = 2.718281828459045;
export const PI = 3.141592653589793;
export const LN2 = 0.6931471805599453;
export const LN10 = 2.302585092994046;
export const LOG2E = 1.4426950408889634;
export const LOG10E = 0.4342944819032518;
export const SQRT1_2 = 0.7071067811865476;
export const SQRT2 = 1.4142135623730951;

export const abs = (value: number): number => DotnetMath.Abs(value);
export const ceil = (value: number): number => DotnetMath.Ceiling(value);
export const floor = (value: number): number => DotnetMath.Floor(value);
export const round = (value: number): number => DotnetMath.Round(value);
export const sqrt = (value: number): number => DotnetMath.Sqrt(value);
export const pow = (left: number, right: number): number =>
  DotnetMath.Pow(left, right);
export const max = (...values: number[]): number => {
  if (values.length === 0) {
    return NEGATIVE_INFINITY;
  }

  let current = values[0]!;
  for (let i = 1; i < values.length; i += 1) {
    current = current > values[i]! ? current : values[i]!;
  }
  return current;
};
export const min = (...values: number[]): number => {
  if (values.length === 0) {
    return POSITIVE_INFINITY;
  }

  let current = values[0]!;
  for (let i = 1; i < values.length; i += 1) {
    current = current < values[i]! ? current : values[i]!;
  }
  return current;
};
export const sin = (value: number): number => DotnetMath.Sin(value);
export const cos = (value: number): number => DotnetMath.Cos(value);
export const tan = (value: number): number => DotnetMath.Tan(value);
export const asin = (value: number): number => DotnetMath.Asin(value);
export const acos = (value: number): number => DotnetMath.Acos(value);
export const atan = (value: number): number => DotnetMath.Atan(value);
export const atan2 = (left: number, right: number): number =>
  DotnetMath.Atan2(left, right);
export const exp = (value: number): number => DotnetMath.Exp(value);
export const log = (value: number): number => DotnetMath.Log(value);
export const log10 = (value: number): number => DotnetMath.Log10(value);
export const log2 = (value: number): number => DotnetMath.Log2(value);
export const random = (): number => randomSource.NextDouble();
export const sign = (value: number): number => DotnetMath.Sign(value);
export const trunc = (value: number): number => DotnetMath.Truncate(value);
