import type { int } from "@tsonic/core/types.js";
import { isInteger } from "./Globals.js";
import { RangeError } from "./range-error.js";

export function isInt32(value: number): value is int {
  return isInteger(value) && value >= -2147483648 && value <= 2147483647;
}

export const toInt = (value: number): int => {
  if (isInt32(value)) {
    return value as int;
  }

  throw new RangeError("Expected Int32-compatible numeric value");
};
