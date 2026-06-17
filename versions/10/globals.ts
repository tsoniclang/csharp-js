import type { byte, double, int, JsValue, long } from "@tsonic/core/types.js";
import { Array as SourceArray } from "./src/array-object.js";
import { ArrayBuffer as SourceArrayBuffer } from "./src/array-buffer-object.js";
import {
  BigInt as SourceBigInt,
  Boolean as SourceBoolean,
  Infinity as SourceInfinity,
  NaN as SourceNaN,
  Number as SourceNumberFunction,
  String as SourceString,
  decodeURI as SourceDecodeURI,
  decodeURIComponent as SourceDecodeURIComponent,
  encodeURI as SourceEncodeURI,
  encodeURIComponent as SourceEncodeURIComponent,
  isFinite as SourceIsFinite,
  isNaN as SourceIsNaN,
  parseFloat as SourceParseFloat,
  parseInt as SourceParseInt,
} from "./src/Globals.js";
import {
  StringConstructor as SourceStringStatics,
} from "./src/String.js";
import { console as SourceConsole } from "./src/console-object.js";
import { Date as SourceDate } from "./src/date-object.js";
import { DataView as SourceDataView } from "./src/data-view.js";
import { Error as SourceError } from "./src/error-object.js";
import { Float32Array as SourceFloat32Array } from "./src/float32-array.js";
import { Float64Array as SourceFloat64Array } from "./src/float64-array.js";
import {
  Map as SourceMap,
  type ReadonlyMap as SourceReadonlyMap,
} from "./src/map-object.js";
import { Math as SourceMath } from "./src/math-object.js";
import { Number as SourceNumberStatics } from "./src/number-object.js";
import { Object as SourceObject } from "./src/object-object.js";
import { Int16Array as SourceInt16Array } from "./src/int16-array.js";
import { Int32Array as SourceInt32Array } from "./src/int32-array.js";
import { Int8Array as SourceInt8Array } from "./src/int8-array.js";
import { JSON as SourceJSON } from "./src/json-object.js";
import { RangeError as SourceRangeError } from "./src/range-error.js";
import { RegExp as SourceRegExp } from "./src/regexp-object.js";
import {
  Set as SourceSet,
  type ReadonlySet as SourceReadonlySet,
} from "./src/set-object.js";
import { TextEncoder as SourceTextEncoder } from "./src/text-encoder.js";
import { Uint16Array as SourceUint16Array } from "./src/uint16-array.js";
import { Uint32Array as SourceUint32Array } from "./src/uint32-array.js";
import { Uint8Array as SourceUint8Array } from "./src/uint8-array.js";
import { Uint8ClampedArray as SourceUint8ClampedArray } from "./src/uint8-clamped-array.js";
import { WeakMap as SourceWeakMap } from "./src/weak-map-object.js";
import { WeakSet as SourceWeakSet } from "./src/weak-set-object.js";

type RuntimePrimitiveValue = string | number | bigint | boolean | null;
type RuntimeValue = RuntimePrimitiveValue | object;

declare global {
  interface Error extends SourceError {
    name: string;
    message: string;
    stack?: string;
  }

  interface ErrorConstructor {
    readonly prototype: Error;
    new (message?: string): Error;
  }

  interface RangeErrorConstructor extends ErrorConstructor {
    readonly prototype: RangeError;
    new (message?: string): RangeError;
  }

  interface RangeError extends Error, SourceRangeError {}

  interface Function {
    readonly length: int;
    prototype: object;
  }

  interface CallableFunction extends Function {}

  interface NewableFunction extends Function {}

  interface IArguments {
    readonly length: int;
    readonly [index: number]: RuntimeValue;
  }

  interface Object {
    constructor: Function;
  }

  interface SymbolConstructor {
    (description?: string | number): symbol;
    readonly iterator: unique symbol;
    readonly asyncIterator: unique symbol;
    readonly hasInstance: unique symbol;
    readonly isConcatSpreadable: unique symbol;
    readonly species: unique symbol;
    readonly toPrimitive: unique symbol;
    readonly toStringTag: unique symbol;
  }

  type PropertyKey = string | number | symbol;

  interface Promise<T> {
    then(): Promise<T>;
    then<TResult1>(
      onfulfilled:
        | ((value: T) => TResult1 | PromiseLike<TResult1>)
        | undefined
        | null,
    ): Promise<TResult1>;
    then<TResult1, TResult2>(
      onfulfilled:
        | ((value: T) => TResult1 | PromiseLike<TResult1>)
        | undefined
        | null,
      onrejected:
        | ((reason: RuntimeValue) => TResult2 | PromiseLike<TResult2>)
        | undefined
        | null,
    ): Promise<TResult1 | TResult2>;
    catch(): Promise<T>;
    catch<TResult>(
      onrejected:
        | ((reason: RuntimeValue) => TResult | PromiseLike<TResult>)
        | undefined
        | null,
    ): Promise<T | TResult>;
    finally(onfinally?: (() => void) | undefined | null): Promise<T>;
  }

  interface PromiseLike<T> {
    then(): PromiseLike<T>;
    then<TResult1>(
      onfulfilled:
        | ((value: T) => TResult1 | PromiseLike<TResult1>)
        | undefined
        | null,
    ): PromiseLike<TResult1>;
    then<TResult1, TResult2>(
      onfulfilled:
        | ((value: T) => TResult1 | PromiseLike<TResult1>)
        | undefined
        | null,
      onrejected:
        | ((reason: RuntimeValue) => TResult2 | PromiseLike<TResult2>)
        | undefined
        | null,
    ): PromiseLike<TResult1 | TResult2>;
  }

  interface PromiseConstructor {
    new <T>(
      executor: (
        resolve: (value: T | PromiseLike<T>) => void,
        reject: (reason?: RuntimeValue) => void,
      ) => void,
    ): Promise<T>;
    resolve(): Promise<void>;
    resolve<T>(value: T | PromiseLike<T>): Promise<T>;
    reject<T>(reason?: RuntimeValue): Promise<T>;
    reject(reason?: RuntimeValue): Promise<RuntimeValue>;
    all<T>(values: readonly (T | PromiseLike<T>)[]): Promise<T[]>;
    race<T>(values: readonly (T | PromiseLike<T>)[]): Promise<T>;
  }

  interface IteratorYieldResult<T> {
    done?: false;
    value: T;
  }

  interface IteratorReturnResult<TReturn> {
    done: true;
    value: TReturn;
  }

  interface IteratorResult<T, TReturn = undefined> {
    done: boolean;
    value: T | TReturn;
  }

  interface Iterator<T, TReturn = undefined, TNext = undefined> {
    next(...[value]: [] | [TNext]): IteratorResult<T, TReturn>;
    return?(value?: TReturn): IteratorResult<T, TReturn>;
    throw?(e?: RuntimeValue): IteratorResult<T, TReturn>;
  }

  interface Iterable<T, TReturn = undefined, TNext = undefined> {
    [Symbol.iterator](): Iterator<T, TReturn, TNext>;
  }

  interface IterableIterator<T, TReturn = undefined, TNext = undefined>
    extends Generator<T, TReturn, TNext> {
    [Symbol.iterator](): IterableIterator<T, TReturn, TNext>;
  }

  interface IteratorObject<T, TReturn = undefined, TNext = undefined>
    extends Iterator<T, TReturn, TNext> {
    [Symbol.iterator](): IteratorObject<T, TReturn, TNext>;
  }

  interface AsyncIterator<T, TReturn = undefined, TNext = undefined> {
    next(...[value]: [] | [TNext]): Promise<IteratorResult<T, TReturn>>;
    return?(
      value?: TReturn | PromiseLike<TReturn>,
    ): Promise<IteratorResult<T, TReturn>>;
    throw?(e?: RuntimeValue): Promise<IteratorResult<T, TReturn>>;
  }

  interface AsyncIterable<T, TReturn = undefined, TNext = undefined> {
    [Symbol.asyncIterator](): AsyncIterator<T, TReturn, TNext>;
  }

  interface AsyncIterableIterator<T, TReturn = undefined, TNext = undefined>
    extends AsyncIterator<T, TReturn, TNext> {
    [Symbol.asyncIterator](): AsyncIterableIterator<T, TReturn, TNext>;
  }

  interface Generator<T = RuntimeValue, TReturn = undefined, TNext = undefined>
    extends IteratorObject<T, TReturn, TNext> {
    next(...[value]: [] | [TNext]): IteratorResult<T, TReturn>;
    return(value: TReturn): IteratorResult<T, TReturn>;
    throw(e: RuntimeValue): IteratorResult<T, TReturn>;
    [Symbol.iterator](): Generator<T, TReturn, TNext>;
  }

  interface AsyncGenerator<
    T = RuntimeValue,
    TReturn = undefined,
    TNext = undefined,
  > extends AsyncIterator<T, TReturn, TNext> {
    next(...[value]: [] | [TNext]): Promise<IteratorResult<T, TReturn>>;
    return(
      value: TReturn | PromiseLike<TReturn>,
    ): Promise<IteratorResult<T, TReturn>>;
    throw(e: RuntimeValue): Promise<IteratorResult<T, TReturn>>;
    [Symbol.asyncIterator](): AsyncGenerator<T, TReturn, TNext>;
  }

  interface TemplateStringsArray extends ReadonlyArray<string> {
    readonly raw: readonly string[];
  }

  type Partial<T> = { [P in keyof T]?: T[P] };
  type Required<T> = { [P in keyof T]-?: T[P] };
  type Readonly<T> = { readonly [P in keyof T]: T[P] };
  type Pick<T, K extends keyof T> = { [P in K]: T[P] };
  type Record<K extends PropertyKey, T> = { [P in K]: T };
  type Exclude<T, U> = T extends U ? never : T;
  type Extract<T, U> = T extends U ? T : never;
  type Omit<T, K extends PropertyKey> = Pick<T, Exclude<keyof T, K>>;
  type NonNullable<T> = T extends null | undefined ? never : T;
  type Parameters<T> = T extends (...args: infer P) => infer _R ? P : never;
  type ConstructorParameters<T> = T extends new (...args: infer P) => infer _R
    ? P
    : never;
  type ReturnType<T> = T extends (...args: infer _P) => infer R ? R : never;
  type InstanceType<T> = T extends new (...args: infer _P) => infer R
    ? R
    : never;
  type Awaited<T> = T extends PromiseLike<infer U> ? Awaited<U> : T;

  interface ArrayLike<T> {
    readonly length: int;
    readonly [n: number]: T;
  }

  interface String {
    readonly [n: number]: string;
    readonly length: int;
    at(index: number): string;
    charAt(index: number): string;
    charCodeAt(index: number): int;
    codePointAt(index: number): int;
    concat(...strings: string[]): string;
    endsWith(searchString: string): boolean;
    includes(searchString: string): boolean;
    indexOf(searchString: string, position?: number): int;
    isWellFormed(): boolean;
    lastIndexOf(searchString: string, position?: number): int;
    localeCompare(compareString: string): int;
    match(pattern: string | RegExp): string[] | undefined;
    matchAll(pattern: string | RegExp): string[][];
    normalize(form?: string): string;
    padEnd(targetLength: number, padString?: string): string;
    padStart(targetLength: number, padString?: string): string;
    repeat(count: number): string;
    replace(searchValue: string | RegExp, replaceValue: string): string;
    replace(
      searchValue: string | RegExp,
      replaceValue: (match: string, ...captures: string[]) => string,
    ): string;
    replaceAll(searchValue: string, replaceValue: string): string;
    search(pattern: string | RegExp): int;
    slice(start?: number, end?: number): string;
    split(separator: string | RegExp, limit?: number): string[];
    startsWith(searchString: string): boolean;
    substr(start: number, length?: number): string;
    substring(start: number, end?: number): string;
    toLocaleLowerCase(): string;
    toLocaleUpperCase(): string;
    toLowerCase(): string;
    toString(): string;
    trim(): string;
    trimLeft(): string;
    trimRight(): string;
    trimStart(): string;
    trimEnd(): string;
    toUpperCase(): string;
    toWellFormed(): string;
    valueOf(): string;
    [Symbol.iterator](): IterableIterator<string, undefined, undefined>;
  }

  interface StringConstructor {
    (value?: RuntimeValue): string;
    fromCharCode(...codes: number[]): string;
    fromCodePoint(...codePoints: number[]): string;
  }

  interface Number {
    toString(): string;
    valueOf(): number;
  }

  interface Boolean {
    toString(): string;
    valueOf(): boolean;
  }

  interface BooleanConstructor {
    (value?: RuntimeValue): boolean;
  }

  interface NumberConstructor {
    (value?: RuntimePrimitiveValue): number;
    readonly EPSILON: number;
    readonly MAX_SAFE_INTEGER: number;
    readonly MAX_VALUE: number;
    readonly MIN_SAFE_INTEGER: number;
    readonly MIN_VALUE: number;
    readonly NEGATIVE_INFINITY: number;
    readonly POSITIVE_INFINITY: number;
    readonly NaN: number;
    isFinite(value: number): boolean;
    isInteger(value: number): boolean;
    isNaN(value: number): boolean;
    isSafeInteger(value: number): boolean;
    parseFloat(str: string): number;
    parseInt(str: string, radix?: number): number;
  }

  interface BigInt {
    toString(radix?: int): string;
    valueOf(): bigint;
  }

  interface BigIntConstructor {
    (value: bigint | boolean | number | string): bigint;
    readonly prototype: BigInt;
  }

  interface Array<T> {
    length: int;
    [n: number]: T;
    at(index: int): T;
    concat(...items: T[]): T[];
    copyWithin(target: int, start?: int, end?: int): T[];
    entries(): IterableIterator<[int, T], undefined, undefined>;
    every(callback: (value: T) => boolean): boolean;
    every(callback: (value: T, index: number, array: T[]) => boolean): boolean;
    fill(value: T, start?: int, end?: int): T[];
    filter(callback: (value: T) => boolean): T[];
    filter(callback: (value: T, index: number) => boolean): T[];
    filter(callback: (value: T, index: number, array: T[]) => boolean): T[];
    find(callback: (value: T) => boolean): T | undefined;
    find(callback: (value: T, index: number) => boolean): T | undefined;
    find(
      callback: (value: T, index: number, array: T[]) => boolean,
    ): T | undefined;
    findIndex(callback: (value: T) => boolean): int;
    findIndex(callback: (value: T, index: number) => boolean): int;
    findIndex(callback: (value: T, index: number, array: T[]) => boolean): int;
    findLast(callback: (value: T) => boolean): T | undefined;
    findLast(callback: (value: T, index: number) => boolean): T | undefined;
    findLast(
      callback: (value: T, index: number, array: T[]) => boolean,
    ): T | undefined;
    findLastIndex(callback: (value: T) => boolean): int;
    findLastIndex(callback: (value: T, index: number) => boolean): int;
    findLastIndex(callback: (value: T, index: number, array: T[]) => boolean): int;
    forEach(callback: (value: T) => void): void;
    forEach(callback: (value: T, index: number) => void): void;
    forEach(callback: (value: T, index: number, array: T[]) => void): void;
    includes(searchElement: T): boolean;
    indexOf(searchElement: T, fromIndex?: int): int;
    join(separator?: string): string;
    keys(): IterableIterator<int, undefined, undefined>;
    lastIndexOf(searchElement: T, fromIndex?: int): int;
    flatMap<TResult>(callback: (value: T) => readonly TResult[]): TResult[];
    flatMap<TResult>(
      callback: (value: T, index: number) => readonly TResult[],
    ): TResult[];
    flatMap<TResult>(
      callback: (value: T, index: number, array: T[]) => readonly TResult[],
    ): TResult[];
    map<TResult>(callback: (value: T) => TResult): TResult[];
    map<TResult>(callback: (value: T, index: number) => TResult): TResult[];
    map<TResult>(
      callback: (value: T, index: number, array: T[]) => TResult,
    ): TResult[];
    pop(): T;
    push(...items: T[]): int;
    reduce(callback: (previousValue: T, currentValue: T) => T): T;
    reduce<TResult>(
      callback: (previousValue: TResult, currentValue: T) => TResult,
      initialValue: TResult,
    ): TResult;
    reduce<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduce<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
        array: T[],
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (previousValue: TResult, currentValue: T) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
        array: T[],
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reverse(): T[];
    shift(): T;
    slice(start?: int, end?: int): T[];
    some(callback: (value: T) => boolean): boolean;
    some(callback: (value: T, index: number, array: T[]) => boolean): boolean;
    sort(compareFunc?: (left: T, right: T) => double): T[];
    splice(start: int, deleteCount?: int, ...items: T[]): T[];
    toLocaleString(): string;
    toReversed(): T[];
    toSorted(compareFunc?: (left: T, right: T) => double): T[];
    toSpliced(start: int, deleteCount?: int, ...items: T[]): T[];
    toString(): string;
    unshift(...items: T[]): int;
    values(): IterableIterator<T, undefined, undefined>;
    with(index: int, value: T): T[];
    [Symbol.iterator](): IterableIterator<T, undefined, undefined>;
  }

  interface ReadonlyArray<T> {
    readonly length: int;
    readonly [n: number]: T;
    at(index: int): T;
    concat(...items: T[]): T[];
    entries(): IterableIterator<[int, T], undefined, undefined>;
    every(callback: (value: T) => boolean): boolean;
    every(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): boolean;
    filter(callback: (value: T) => boolean): T[];
    filter(callback: (value: T, index: number) => boolean): T[];
    filter(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): T[];
    find(callback: (value: T) => boolean): T | undefined;
    find(callback: (value: T, index: number) => boolean): T | undefined;
    find(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): T | undefined;
    findIndex(callback: (value: T) => boolean): int;
    findIndex(callback: (value: T, index: number) => boolean): int;
    findIndex(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): int;
    findLast(callback: (value: T) => boolean): T | undefined;
    findLast(callback: (value: T, index: number) => boolean): T | undefined;
    findLast(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): T | undefined;
    findLastIndex(callback: (value: T) => boolean): int;
    findLastIndex(callback: (value: T, index: number) => boolean): int;
    findLastIndex(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): int;
    forEach(callback: (value: T) => void): void;
    forEach(callback: (value: T, index: number) => void): void;
    forEach(
      callback: (value: T, index: number, array: readonly T[]) => void,
    ): void;
    includes(searchElement: T): boolean;
    indexOf(searchElement: T, fromIndex?: int): int;
    join(separator?: string): string;
    keys(): IterableIterator<int, undefined, undefined>;
    lastIndexOf(searchElement: T, fromIndex?: int): int;
    flatMap<TResult>(callback: (value: T) => readonly TResult[]): TResult[];
    flatMap<TResult>(
      callback: (value: T, index: number) => readonly TResult[],
    ): TResult[];
    flatMap<TResult>(
      callback: (
        value: T,
        index: number,
        array: readonly T[],
      ) => readonly TResult[],
    ): TResult[];
    map<TResult>(callback: (value: T) => TResult): TResult[];
    map<TResult>(callback: (value: T, index: number) => TResult): TResult[];
    map<TResult>(
      callback: (value: T, index: number, array: readonly T[]) => TResult,
    ): TResult[];
    reduce(callback: (previousValue: T, currentValue: T) => T): T;
    reduce<TResult>(
      callback: (previousValue: TResult, currentValue: T) => TResult,
      initialValue: TResult,
    ): TResult;
    reduce<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduce<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
        array: readonly T[],
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (previousValue: TResult, currentValue: T) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    reduceRight<TResult>(
      callback: (
        previousValue: TResult,
        currentValue: T,
        index: number,
        array: readonly T[],
      ) => TResult,
      initialValue: TResult,
    ): TResult;
    slice(start?: int, end?: int): T[];
    some(callback: (value: T) => boolean): boolean;
    some(
      callback: (value: T, index: number, array: readonly T[]) => boolean,
    ): boolean;
    toLocaleString(): string;
    toReversed(): T[];
    toSorted(compareFunc?: (left: T, right: T) => double): T[];
    toString(): string;
    values(): IterableIterator<T, undefined, undefined>;
    with(index: int, value: T): T[];
    [Symbol.iterator](): IterableIterator<T, undefined, undefined>;
  }

  interface Console {
    log(...data: (RuntimeValue | undefined)[]): void;
    error(...data: (RuntimeValue | undefined)[]): void;
    warn(...data: (RuntimeValue | undefined)[]): void;
    info(...data: (RuntimeValue | undefined)[]): void;
    debug(...data: (RuntimeValue | undefined)[]): void;
  }

  interface ArrayConstructor {
    readonly prototype: RuntimeValue[];
    new (arrayLength: int): RuntimeValue[];
    new <T>(arrayLength: int): T[];
    new (...items: RuntimeValue[]): RuntimeValue[];
    new <T>(...items: T[]): T[];
    from(source: string): string[];
    from<TResult>(
      source: string,
      mapfn: (value: string, index: number) => TResult,
    ): TResult[];
    from<T>(source: Iterable<T>): T[];
    from<T, TResult>(
      source: Iterable<T>,
      mapfn: (value: T, index: number) => TResult,
    ): TResult[];
    isArray(value: unknown): value is readonly unknown[];
    of<T>(...items: T[]): T[];
  }

  interface Date extends SourceDate {
    getDate(): int;
    getDay(): int;
    getFullYear(): int;
    getHours(): int;
    getMilliseconds(): int;
    getMinutes(): int;
    getMonth(): int;
    getSeconds(): int;
    getTime(): long;
    getTimezoneOffset(): int;
    getUTCDate(): int;
    getUTCDay(): int;
    getUTCFullYear(): int;
    getUTCHours(): int;
    getUTCMilliseconds(): int;
    getUTCMinutes(): int;
    getUTCMonth(): int;
    getUTCSeconds(): int;
    setDate(day: int): number;
    setFullYear(year: int, month?: int, day?: int): number;
    setHours(hour: int, min?: int, sec?: int, ms?: int): number;
    setMilliseconds(ms: int): number;
    setMinutes(min: int, sec?: int, ms?: int): number;
    setMonth(month: int, day?: int): number;
    setSeconds(sec: int, ms?: int): number;
    setTime(milliseconds: number): number;
    setUTCDate(day: int): number;
    setUTCFullYear(year: int, month?: int, day?: int): number;
    setUTCHours(hour: int, min?: int, sec?: int, ms?: int): number;
    setUTCMilliseconds(ms: int): number;
    setUTCMinutes(min: int, sec?: int, ms?: int): number;
    setUTCMonth(month: int, day?: int): number;
    setUTCSeconds(sec: int, ms?: int): number;
    toDateString(): string;
    toISOString(): string;
    toJSON(): string;
    toLocaleDateString(): string;
    toLocaleString(): string;
    toLocaleTimeString(): string;
    toString(): string;
    toTimeString(): string;
    toUTCString(): string;
    valueOf(): long;
  }

  interface ArrayBuffer extends SourceArrayBuffer {
    readonly byteLength: int;
    slice(begin?: int, end?: int): ArrayBuffer;
  }

  interface DataView extends SourceDataView {
    readonly buffer: ArrayBuffer;
    readonly byteOffset: int;
    readonly byteLength: int;
    getInt8(byteOffset: int): number;
    getUint8(byteOffset: int): number;
    getInt16(byteOffset: int, littleEndian?: boolean): number;
    getUint16(byteOffset: int, littleEndian?: boolean): number;
    getInt32(byteOffset: int, littleEndian?: boolean): number;
    getUint32(byteOffset: int, littleEndian?: boolean): number;
    setInt8(byteOffset: int, value: number): void;
    setUint8(byteOffset: int, value: number): void;
    setInt16(byteOffset: int, value: number, littleEndian?: boolean): void;
    setUint16(byteOffset: int, value: number, littleEndian?: boolean): void;
    setInt32(byteOffset: int, value: number, littleEndian?: boolean): void;
    setUint32(byteOffset: int, value: number, littleEndian?: boolean): void;
  }

  interface TypedArrayLike<T, TSelf>
    extends ArrayLike<T>,
      Iterable<T, undefined, undefined> {
    readonly byteLength: int;
    readonly length: int;
    at(index: int): T | undefined;
    entries(): IterableIterator<[int, T], undefined, undefined>;
    fill(value: T, start?: int, end?: int): TSelf;
    includes(value: T, fromIndex?: int): boolean;
    indexOf(value: T, fromIndex?: int): int;
    join(separator?: string): string;
    keys(): IterableIterator<int, undefined, undefined>;
    reverse(): TSelf;
    set(index: int, value: number): void;
    set(source: readonly T[] | Iterable<number>, offset?: int): void;
    slice(begin?: int, end?: int): TSelf;
    sort(compareFn?: (left: T, right: T) => double): TSelf;
    subarray(begin?: int, end?: int): TSelf;
    values(): IterableIterator<T, undefined, undefined>;
    [index: number]: T;
    [Symbol.iterator](): IterableIterator<T, undefined, undefined>;
  }

  interface Int8Array extends TypedArrayLike<number, Int8Array> {}

  interface Uint8Array extends TypedArrayLike<byte, Uint8Array> {
    readonly buffer: ArrayBuffer;
  }

  interface Uint8ClampedArray
    extends TypedArrayLike<number, Uint8ClampedArray> {}

  interface Int16Array extends TypedArrayLike<number, Int16Array> {}

  interface Uint16Array extends TypedArrayLike<number, Uint16Array> {}

  interface Int32Array extends TypedArrayLike<number, Int32Array> {}

  interface Uint32Array extends TypedArrayLike<number, Uint32Array> {}

  interface Float32Array extends TypedArrayLike<number, Float32Array> {}

  interface Float64Array extends TypedArrayLike<number, Float64Array> {}

  interface DateConstructor {
    readonly prototype: Date;
    new (): Date;
    new (value: number | long): Date;
    now(): long;
    parse(s: string): number;
  }

  interface Uint8ArrayConstructor {
    readonly prototype: Uint8Array;
    new (lengthOrValues: int | readonly byte[] | Iterable<number>): Uint8Array;
  }

  interface ArrayBufferConstructor {
    readonly prototype: ArrayBuffer;
    new (byteLength: int): ArrayBuffer;
  }

  interface DataViewConstructor {
    readonly prototype: DataView;
    new (buffer: ArrayBuffer, byteOffset?: int, byteLength?: int): DataView;
  }

  interface Int8ArrayConstructor {
    readonly prototype: Int8Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Int8Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Uint8ClampedArrayConstructor {
    readonly prototype: Uint8ClampedArray;
    new (
      lengthOrValues: int | readonly number[] | Iterable<number>
    ): Uint8ClampedArray;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Int16ArrayConstructor {
    readonly prototype: Int16Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Int16Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Uint16ArrayConstructor {
    readonly prototype: Uint16Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Uint16Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Int32ArrayConstructor {
    readonly prototype: Int32Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Int32Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Uint32ArrayConstructor {
    readonly prototype: Uint32Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Uint32Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Float32ArrayConstructor {
    readonly prototype: Float32Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Float32Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface Float64ArrayConstructor {
    readonly prototype: Float64Array;
    new (lengthOrValues: int | readonly number[] | Iterable<number>): Float64Array;
    readonly BYTES_PER_ELEMENT: number;
  }

  interface JSON {
    parse(text: string): JsValue;
    parse<T>(text: string): T;
    stringify<T>(
      value: T,
      replacer?: undefined,
      space?: string | number | int,
    ): string;
  }

  interface Math {
    abs(x: number): number;
    acos(x: number): number;
    acosh(x: number): number;
    asin(x: number): number;
    asinh(x: number): number;
    atan(x: number): number;
    atan2(y: number, x: number): number;
    atanh(x: number): number;
    cbrt(x: number): number;
    ceil(x: number): number;
    clz32(x: number): number;
    cos(x: number): number;
    cosh(x: number): number;
    exp(x: number): number;
    expm1(x: number): number;
    f16round(x: number): number;
    floor(x: number): number;
    fround(x: number): number;
    hypot(...values: number[]): number;
    imul(a: number, b: number): number;
    log(x: number): number;
    log10(x: number): number;
    log1p(x: number): number;
    log2(x: number): number;
    pow(x: number, y: number): number;
    round(x: number): number;
    sign(x: number): number;
    sin(x: number): number;
    sinh(x: number): number;
    sqrt(x: number): number;
    tan(x: number): number;
    tanh(x: number): number;
    trunc(x: number): number;
    max(...values: number[]): number;
    min(...values: number[]): number;
    random(): number;
  }

  interface RegExpMatchArray extends Array<string> {
    index?: int;
    input?: string;
  }

  interface RegExp extends SourceRegExp {
    readonly source: string;
    readonly flags: string;
    exec(string: string): RegExpMatchArray | null;
    test(string: string): boolean;
    toString(): string;
  }

  interface RegExpConstructor {
    readonly prototype: RegExp;
    new (pattern: string | RegExp, flags?: string): RegExp;
    (pattern: string | RegExp, flags?: string): RegExp;
  }

  interface ReadonlyMap<K, V> extends SourceReadonlyMap<K, V> {
    readonly size: int;
    entries(): IterableIterator<[K, V], undefined, undefined>;
    forEach(
      callback: (value: V, key: K, map: ReadonlyMap<K, V>) => void,
    ): void;
    get(key: K): V | undefined;
    has(key: K): boolean;
    keys(): IterableIterator<K, undefined, undefined>;
    values(): IterableIterator<V, undefined, undefined>;
    [Symbol.iterator](): IterableIterator<[K, V], undefined, undefined>;
  }

  interface Map<K, V> extends ReadonlyMap<K, V>, SourceMap<K, V> {
    readonly size: int;
    clear(): void;
    delete(key: K): boolean;
    entries(): IterableIterator<[K, V], undefined, undefined>;
    forEach(callback: (value: V, key: K, map: ReadonlyMap<K, V>) => void): void;
    get(key: K): V | undefined;
    has(key: K): boolean;
    keys(): IterableIterator<K, undefined, undefined>;
    set(key: K, value: V): this;
    values(): IterableIterator<V, undefined, undefined>;
    [Symbol.iterator](): IterableIterator<[K, V], undefined, undefined>;
  }

  interface MapConstructor {
    readonly prototype: Map<RuntimeValue, RuntimeValue>;
    new <K, V>(entries?: Iterable<readonly [K, V]> | null): Map<K, V>;
  }

  interface ReadonlySet<T> extends SourceReadonlySet<T> {
    readonly size: int;
    entries(): IterableIterator<[T, T], undefined, undefined>;
    forEach(callback: (value: T, key: T, set: ReadonlySet<T>) => void): void;
    has(value: T): boolean;
    keys(): IterableIterator<T, undefined, undefined>;
    values(): IterableIterator<T, undefined, undefined>;
    [Symbol.iterator](): IterableIterator<T, undefined, undefined>;
  }

  interface Set<T> extends ReadonlySet<T>, SourceSet<T> {
    readonly size: int;
    add(value: T): this;
    clear(): void;
    delete(value: T): boolean;
    entries(): IterableIterator<[T, T], undefined, undefined>;
    forEach(callback: (value: T, key: T, set: ReadonlySet<T>) => void): void;
    has(value: T): boolean;
    keys(): IterableIterator<T, undefined, undefined>;
    values(): IterableIterator<T, undefined, undefined>;
    [Symbol.iterator](): IterableIterator<T, undefined, undefined>;
  }

  interface SetConstructor {
    readonly prototype: Set<RuntimeValue>;
    new <T>(values?: Iterable<T> | null): Set<T>;
  }

  interface WeakMap<K extends object, V> extends SourceWeakMap<K, V> {
    delete(key: K): boolean;
    get(key: K): V | undefined;
    has(key: K): boolean;
    set(key: K, value: V): this;
  }

  interface WeakMapConstructor {
    readonly prototype: WeakMap<object, RuntimeValue>;
    new <K extends object, V>(
      entries?: readonly (readonly [K, V])[] | null,
    ): WeakMap<K, V>;
  }

  interface WeakSet<T extends object> extends SourceWeakSet<T> {
    add(value: T): this;
    delete(value: T): boolean;
    has(value: T): boolean;
  }

  interface WeakSetConstructor {
    readonly prototype: WeakSet<object>;
    new <T extends object = object>(values?: readonly T[] | null): WeakSet<T>;
  }

  interface TextEncoder {
    readonly encoding: string;
    encode(input?: string): Uint8Array;
  }

  interface TextEncoderConstructor {
    readonly prototype: TextEncoder;
    new(): TextEncoder;
  }

  interface ObjectConstructor {
    readonly prototype: object;
    entries(obj: Record<string, RuntimeValue>): [string, RuntimeValue][];
    entries<T>(obj: Record<string, T>): [string, T][];
    entries(obj: object): [string, RuntimeValue][];
    is(value1: RuntimeValue | undefined, value2: RuntimeValue | undefined): boolean;
    keys<T>(obj: Record<string, T>): string[];
    values<T>(obj: Record<string, T>): T[];
  }

  const Error: ErrorConstructor;

  const String: typeof SourceString & typeof SourceStringStatics;

  const Number: typeof SourceNumberFunction & typeof SourceNumberStatics;

  const BigInt: typeof SourceBigInt;

  const NaN: typeof SourceNaN;

  const Infinity: typeof SourceInfinity;

  const Boolean: typeof SourceBoolean;

  const console: typeof SourceConsole;

  const Date: DateConstructor;

  const Array: ArrayConstructor & typeof SourceArray;

  const ArrayBuffer: ArrayBufferConstructor;

  const DataView: DataViewConstructor;

  const Int8Array: Int8ArrayConstructor;

  const Uint8Array: Uint8ArrayConstructor;

  const Uint8ClampedArray: Uint8ClampedArrayConstructor;

  const Int16Array: Int16ArrayConstructor;

  const Uint16Array: Uint16ArrayConstructor;

  const Int32Array: Int32ArrayConstructor;

  const Uint32Array: Uint32ArrayConstructor;

  const Float32Array: Float32ArrayConstructor;

  const Float64Array: Float64ArrayConstructor;

  const JSON: JSON & typeof SourceJSON;

  const Math: typeof SourceMath;

  const RegExp: RegExpConstructor;

  const Map: MapConstructor;

  const Set: SetConstructor;

  const WeakMap: WeakMapConstructor;

  const WeakSet: WeakSetConstructor;

  const TextEncoder: TextEncoderConstructor;

  const Object: ObjectConstructor & typeof SourceObject;

  const RangeError: typeof SourceRangeError;

  const Symbol: SymbolConstructor;

  const Promise: PromiseConstructor;

  const parseInt: typeof SourceParseInt;

  const parseFloat: typeof SourceParseFloat;

  function setTimeout(
    handler: (...args: RuntimeValue[]) => void,
    timeout?: int,
    ...args: RuntimeValue[]
  ): int;

  function clearTimeout(id: int): void;

  function setInterval(
    handler: (...args: RuntimeValue[]) => void,
    timeout?: int,
    ...args: RuntimeValue[]
  ): int;

  function clearInterval(id: int): void;

  const decodeURI: typeof SourceDecodeURI;

  const decodeURIComponent: typeof SourceDecodeURIComponent;

  const encodeURI: typeof SourceEncodeURI;

  const encodeURIComponent: typeof SourceEncodeURIComponent;

  const isFinite: typeof SourceIsFinite;

  const isNaN: typeof SourceIsNaN;
}

export {};
