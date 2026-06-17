import type { byte, int } from "@tsonic/core/types.js";
import { RangeError } from "./range-error.js";
import { ArrayBuffer } from "./array-buffer-object.js";
import { Convert } from "@tsonic/dotnet/System.js";

const UINT8_MAX = 0xff;
const UINT16_MAX = 0xffff;
const UINT32_FACTOR = 4294967296;

const normalizeByteOffset = (
  buffer: ArrayBuffer,
  byteOffset: int,
  width: int
): int => {
  if (byteOffset < (0 as int) || byteOffset + width > buffer.byteLength) {
    throw new RangeError("Offset is outside the bounds of the DataView");
  }
  return byteOffset;
};

const normalizeUint32 = (value: number): number => {
  const truncated = Math.trunc(value);
  const wrapped = truncated % UINT32_FACTOR;
  return wrapped < 0 ? wrapped + UINT32_FACTOR : wrapped;
};

const normalizeInt32 = (value: number): number => {
  const unsigned = normalizeUint32(value);
  return unsigned >= 2147483648 ? unsigned - UINT32_FACTOR : unsigned;
};

const readUnsigned = (
  bytes: byte[],
  offset: int,
  width: int,
  littleEndian: boolean
): number => {
  let result: number = 0;
  for (let index = 0 as int; index < width; index = (index + 1) as int) {
    const sourceIndex = littleEndian ? offset + index : offset + width - 1 - index;
    result += (bytes[sourceIndex] ?? 0) * Math.pow(256, index);
  }
  return result;
};

const writeUnsigned = (
  bytes: byte[],
  offset: int,
  width: int,
  value: number,
  littleEndian: boolean
): void => {
  let remaining = normalizeUint32(value);
  for (let index = 0 as int; index < width; index = (index + 1) as int) {
    const targetIndex = littleEndian ? offset + index : offset + width - 1 - index;
    bytes[targetIndex] = Convert.ToByte(remaining & UINT8_MAX);
    remaining = Math.floor(remaining / 256);
  }
};

export class DataView {
  readonly buffer: ArrayBuffer;
  readonly byteOffset: int;
  readonly byteLength: int;

  constructor(buffer: ArrayBuffer, byteOffset: int = 0 as int, byteLength?: int) {
    const viewLength = byteLength ?? (buffer.byteLength - byteOffset) as int;
    normalizeByteOffset(buffer, byteOffset, viewLength);
    this.buffer = buffer;
    this.byteOffset = byteOffset;
    this.byteLength = viewLength;
  }

  getUint8(byteOffset: int): number {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 1 as int);
    return this.buffer.bytes[offset] ?? 0;
  }

  getInt8(byteOffset: int): number {
    const value = this.getUint8(byteOffset);
    return value > 127 ? value - 256 : value;
  }

  getUint16(byteOffset: int, littleEndian: boolean = false): number {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 2 as int);
    return readUnsigned(this.buffer.bytes, offset, 2 as int, littleEndian) & UINT16_MAX;
  }

  getInt16(byteOffset: int, littleEndian: boolean = false): number {
    const value = this.getUint16(byteOffset, littleEndian);
    return value > 32767 ? value - 65536 : value;
  }

  getUint32(byteOffset: int, littleEndian: boolean = false): number {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 4 as int);
    return readUnsigned(this.buffer.bytes, offset, 4 as int, littleEndian);
  }

  getInt32(byteOffset: int, littleEndian: boolean = false): number {
    return normalizeInt32(this.getUint32(byteOffset, littleEndian));
  }

  setUint8(byteOffset: int, value: number): void {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 1 as int);
    this.buffer.bytes[offset] = Convert.ToByte(value & UINT8_MAX);
  }

  setInt8(byteOffset: int, value: number): void {
    this.setUint8(byteOffset, value);
  }

  setUint16(byteOffset: int, value: number, littleEndian: boolean = false): void {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 2 as int);
    writeUnsigned(this.buffer.bytes, offset, 2 as int, value, littleEndian);
  }

  setInt16(byteOffset: int, value: number, littleEndian: boolean = false): void {
    this.setUint16(byteOffset, value, littleEndian);
  }

  setUint32(byteOffset: int, value: number, littleEndian: boolean = false): void {
    const offset = normalizeByteOffset(this.buffer, (this.byteOffset + byteOffset) as int, 4 as int);
    writeUnsigned(this.buffer.bytes, offset, 4 as int, value, littleEndian);
  }

  setInt32(byteOffset: int, value: number, littleEndian: boolean = false): void {
    this.setUint32(byteOffset, value, littleEndian);
  }
}
