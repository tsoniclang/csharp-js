import type { double, int } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeFloat64,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";

function wrapFloat64Array(values: double[]): Float64Array {
  const result = new Float64Array(0 as int);
  result.replaceData(values);
  return result;
}

export class Float64Array extends TypedArrayBase<double, Float64Array> {
  static BYTES_PER_ELEMENT: int = 8 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<double>
  ) {
    super(
      lengthOrValues,
      Float64Array.BYTES_PER_ELEMENT,
      0 as double,
      normalizeFloat64,
      numericIdentity,
      wrapFloat64Array
    );
  }
}
