import type { byte, int } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeUint8Clamped,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";

function wrapUint8ClampedArray(values: byte[]): Uint8ClampedArray {
  const result = new Uint8ClampedArray(0 as int);
  result.replaceData(values);
  return result;
}

export class Uint8ClampedArray extends TypedArrayBase<byte, Uint8ClampedArray> {
  static BYTES_PER_ELEMENT: int = 1 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<byte>
  ) {
    super(
      lengthOrValues,
      Uint8ClampedArray.BYTES_PER_ELEMENT,
      0 as byte,
      normalizeUint8Clamped,
      numericIdentity,
      wrapUint8ClampedArray
    );
  }
}
