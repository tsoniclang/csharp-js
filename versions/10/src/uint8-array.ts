import type { byte, int } from "@tsonic/core/types.js";
import {
  numericIdentity,
  normalizeUint8,
  TypedArrayBase,
} from "./typed-array-core.js";
import type { TypedArrayConstructorInput } from "./typed-array-core.js";
import { ArrayBuffer } from "./array-buffer-object.js";

function wrapUint8Array(values: byte[]): Uint8Array {
  const result = new Uint8Array(0 as int);
  result.replaceData(values);
  return result;
}

export class Uint8Array extends TypedArrayBase<byte, Uint8Array> {
  static BYTES_PER_ELEMENT: int = 1 as int;

  constructor(
    lengthOrValues: TypedArrayConstructorInput<byte>
  ) {
    super(
      lengthOrValues,
      Uint8Array.BYTES_PER_ELEMENT,
      0 as byte,
      normalizeUint8,
      numericIdentity,
      wrapUint8Array
    );
  }

  toByteArrayRaw(): byte[] {
    return this.data;
  }

  get buffer(): ArrayBuffer {
    return ArrayBuffer.fromBytesRaw(this.data);
  }

  static fromByteArrayRaw(values: byte[]): Uint8Array {
    const result = new Uint8Array(0 as int);
    result.replaceData(values);
    return result;
  }
}
