import type { float, int } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeFloat32,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";

function wrapFloat32Array(values: float[]): Float32Array {
  const result = new Float32Array(0 as int);
  result.replaceData(values);
  return result;
}

export class Float32Array extends TypedArrayBase<float, Float32Array> {
  static BYTES_PER_ELEMENT: int = 4 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<float>
  ) {
    super(
      lengthOrValues,
      Float32Array.BYTES_PER_ELEMENT,
      0 as float,
      normalizeFloat32,
      numericIdentity,
      wrapFloat32Array
    );
  }
}
