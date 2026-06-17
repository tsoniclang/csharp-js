import type { int } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeInt32,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";

function wrapInt32Array(values: int[]): Int32Array {
  const result = new Int32Array(0 as int);
  result.replaceData(values);
  return result;
}

export class Int32Array extends TypedArrayBase<int, Int32Array> {
  static BYTES_PER_ELEMENT: int = 4 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<int>
  ) {
    super(
      lengthOrValues,
      Int32Array.BYTES_PER_ELEMENT,
      0 as int,
      normalizeInt32,
      numericIdentity,
      wrapInt32Array
    );
  }
}
