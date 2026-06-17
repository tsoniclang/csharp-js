import type { byte, int } from "@tsonic/core/types.js";
import { trunc } from "./math-intrinsics.js";
import { toInt } from "./int32.js";

const clampIndex = (
  value: int | undefined,
  length: int,
  fallback: int
): int => {
  if (value === undefined) {
    return fallback;
  }

  const normalized = toInt(trunc(value));
  if (normalized < 0) {
    const fromEnd = toInt(length + normalized);
    return fromEnd < (0 as int) ? (0 as int) : fromEnd;
  }
  if (normalized > length) {
    return length;
  }
  return normalized;
};

const zeroBytes = (length: int): byte[] => {
  const result: byte[] = [];
  for (let index = 0 as int; index < length; index = (index + 1) as int) {
    result.push(0 as byte);
  }
  return result;
};

export class ArrayBuffer {
  bytes: byte[];

  constructor(byteLength: int) {
    this.bytes = zeroBytes(byteLength < (0 as int) ? (0 as int) : byteLength);
  }

  static fromBytesRaw(values: byte[]): ArrayBuffer {
    const buffer = new ArrayBuffer(0 as int);
    buffer.replaceBytesRaw(values);
    return buffer;
  }

  replaceBytesRaw(values: byte[]): void {
    this.bytes = values;
  }

  get byteLength(): int {
    return this.bytes.length as int;
  }

  slice(begin?: int, end?: int): ArrayBuffer {
    const start = clampIndex(begin, this.byteLength, 0 as int);
    const finish = clampIndex(end, this.byteLength, this.byteLength);
    const length = finish > start ? toInt(finish - start) : (0 as int);
    const result = new ArrayBuffer(length);
    for (let index = 0 as int; index < length; index = (index + 1) as int) {
      result.bytes[index] = this.bytes[start + index] ?? (0 as byte);
    }
    return result;
  }
}
