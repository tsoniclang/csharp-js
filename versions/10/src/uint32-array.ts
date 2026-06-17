import type { int, uint } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeUint32,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";

function wrapUint32Array(values: uint[]): Uint32Array {
  const result = new Uint32Array(0 as int);
  result.replaceData(values);
  return result;
}

export class Uint32Array extends TypedArrayBase<uint, Uint32Array> {
  static BYTES_PER_ELEMENT: int = 4 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<uint>
  ) {
    super(
      lengthOrValues,
      Uint32Array.BYTES_PER_ELEMENT,
      0 as uint,
      normalizeUint32,
      numericIdentity,
      wrapUint32Array
    );
  }
}
