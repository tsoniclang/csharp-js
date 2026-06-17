import type { double, int } from "@tsonic/core/types.js";
import { overloads as O } from "@tsonic/core/lang.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { String as coerceString } from "./Globals.js";
import { sameValueZero } from "./same-value-zero.js";

type ArrayRuntimeValue = string | number | boolean | object | null | undefined;

const mapString = (source: string): string[] => {
  const values = new List<string>();
  for (let i = 0 as int; i < source.length; i = (i + 1) as int) {
    values.Add(source[i]!);
  }
  return values.ToArray();
};

const mapStringMapped = <TResult>(
  source: string,
  mapfn: (value: string, index: number) => TResult,
): TResult[] => {
  const mapped = new List<TResult>();
  for (let i = 0 as int; i < source.length; i = (i + 1) as int) {
    mapped.Add(mapfn(source[i]!, i));
  }
  return mapped.ToArray();
};

const mapIterable = <T>(source: Iterable<T>): T[] => {
  const values = new List<T>();
  for (const value of source) {
    values.Add(value);
  }
  return values.ToArray();
};

const mapIterableMapped = <T, TResult>(
  source: Iterable<T>,
  mapfn: (value: T, index: number) => TResult,
): TResult[] => {
  const mapped = new List<TResult>();
  let index = 0 as int;
  for (const value of source) {
    mapped.Add(mapfn(value, index));
    index = (index + 1) as int;
  }
  return mapped.ToArray();
};

const wrapArray = <T>(values: readonly T[] | T[]): Array<T> => {
  const array = new Array<T>();
  for (const value of values) {
    array.push(value);
  }
  return array;
};

const cloneList = <T>(source: List<T>): List<T> => {
  const clone = new List<T>();
  for (let i = 0 as int; i < source.Count; i = (i + 1) as int) {
    clone.Add(source[i]!);
  }
  return clone;
};

const normalizeRelativeIndex = (index: int, length: int): int => {
  if (index < 0) {
    const normalized = (length + index) as int;
    return normalized < 0 ? (0 as int) : normalized;
  }

  return index > length ? length : index;
};

const normalizeElementIndex = (index: int, length: int): int => {
  if (index < 0) {
    const normalized = (length + index) as int;
    return normalized < 0 ? (-1 as int) : normalized;
  }

  return index >= length ? (-1 as int) : index;
};

const appendList = <T>(target: List<T>, source: List<T>): void => {
  for (let i = 0 as int; i < source.Count; i = (i + 1) as int) {
    target.Add(source[i]!);
  }
};

export class Array<T = unknown> {
  valuesStore: List<T> = new List<T>();

  constructor(...items: T[]) {
    for (const item of items) {
      this.valuesStore.Add(item);
    }
  }

  static from(source: string): string[];
  static from<TResult>(
    source: string,
    mapfn: (value: string, index: number) => TResult,
  ): TResult[];
  static from<T>(source: Iterable<T>): T[];
  static from<T, TResult>(
    source: Iterable<T>,
    mapfn: (value: T, index: number) => TResult,
  ): TResult[];
  static from<T, TResult>(
    source: Iterable<T>,
    mapfn?: (value: T, index: number) => TResult,
  ): T[] | TResult[] {
    return mapfn
      ? this.from_iterableMapped(source, mapfn)
      : this.from_iterable(source);
  }

  static from_string(source: string): string[] {
    return mapString(source);
  }

  static from_stringMapped<TResult>(
    source: string,
    mapfn: (value: string, index: number) => TResult,
  ): TResult[] {
    return mapStringMapped(source, mapfn);
  }

  static from_iterable<T>(source: Iterable<T>): T[] {
    return mapIterable(source);
  }

  static from_iterableMapped<T, TResult>(
    source: Iterable<T>,
    mapfn: (value: T, index: number) => TResult,
  ): TResult[] {
    return mapIterableMapped(source, mapfn);
  }

  static of<T>(...items: T[]): T[] {
    return items;
  }

  get length(): int {
    return this.valuesStore.Count;
  }

  toArray(): T[] {
    return this.valuesStore.ToArray();
  }

  at(index: int): T {
    const normalized = normalizeElementIndex(index, this.valuesStore.Count);
    return normalized < 0 ? (undefined as T) : this.valuesStore[normalized]!;
  }

  concat(...items: T[]): Array<T> {
    const merged = cloneList(this.valuesStore);
    for (const item of items) {
      merged.Add(item);
    }
    return wrapArray<T>(merged.ToArray());
  }

  copyWithin(target: int, start?: int, end?: int): this {
    const values = this.toArray();
    const length = values.length as int;
    const to = normalizeRelativeIndex(target, length);
    const from = normalizeRelativeIndex(start ?? (0 as int), length);
    const final = normalizeRelativeIndex(end ?? length, length);
    const copied = values.slice(from, final);
    for (let i = 0 as int; i < copied.length; i = (i + 1) as int) {
      const destination = (to + i) as int;
      if (destination >= length) {
        break;
      }
      values[destination] = copied[i]!;
    }
    this.valuesStore.Clear();
    for (const value of values) {
      this.valuesStore.Add(value);
    }
    return this;
  }

  entries(): IterableIterator<[int, T], undefined, undefined> {
    return (function* (
      self: Array<T>,
    ): Generator<[int, T], undefined, undefined> {
      for (let i = 0 as int; i < self.length; i = (i + 1) as int) {
        yield [i, self.valuesStore[i]!];
      }
    })(this);
  }

  every(callback: (value: T) => boolean): boolean;
  every(callback: (value: T, index: number, array: T[]) => boolean): boolean;
  every(callback: any): any {
    return this.every_full(callback);
  }

  every_value(callback: (value: T) => boolean): boolean {
    return this.every_full((value, _index, _array) => callback(value));
  }

  every_full(callback: (value: T, index: number, array: T[]) => boolean): boolean {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (!callback(this.valuesStore[i]!, i, this.toArray())) {
        return false;
      }
    }
    return true;
  }

  fill(value: T, start?: int, end?: int): this {
    const length = this.valuesStore.Count;
    const from = normalizeRelativeIndex(start ?? (0 as int), length);
    const to = normalizeRelativeIndex(end ?? length, length);
    for (let i = from; i < to; i = (i + 1) as int) {
      this.valuesStore[i] = value;
    }
    return this;
  }

  filter(callback: (value: T) => boolean): Array<T>;
  filter(callback: (value: T, index: number) => boolean): Array<T>;
  filter(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): Array<T>;
  filter(callback: any): any {
    return this.filter_full(callback);
  }

  filter_value(callback: (value: T) => boolean): Array<T> {
    return this.filter_full((value, _index, _array) => callback(value));
  }

  filter_valueIndex(callback: (value: T, index: number) => boolean): Array<T> {
    return this.filter_full((value, index, _array) => callback(value, index));
  }

  filter_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): Array<T> {
    const filtered = new List<T>();
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      const value = this.valuesStore[i]!;
      if (callback(value, i, this.toArray())) {
        filtered.Add(value);
      }
    }
    return wrapArray<T>(filtered.ToArray());
  }

  find(callback: (value: T) => boolean): T | undefined;
  find(callback: (value: T, index: number) => boolean): T | undefined;
  find(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): T | undefined;
  find(callback: any): any {
    return this.find_full(callback);
  }

  find_value(callback: (value: T) => boolean): T | undefined {
    return this.find_full((value, _index, _array) => callback(value));
  }

  find_valueIndex(callback: (value: T, index: number) => boolean): T | undefined {
    return this.find_full((value, index, _array) => callback(value, index));
  }

  find_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): T | undefined {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      const value = this.valuesStore[i]!;
      if (callback(value, i, this.toArray())) {
        return value;
      }
    }
    return undefined;
  }

  findIndex(callback: (value: T) => boolean): int;
  findIndex(callback: (value: T, index: number) => boolean): int;
  findIndex(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): int;
  findIndex(callback: any): any {
    return this.findIndex_full(callback);
  }

  findIndex_value(callback: (value: T) => boolean): int {
    return this.findIndex_full((value, _index, _array) => callback(value));
  }

  findIndex_valueIndex(callback: (value: T, index: number) => boolean): int {
    return this.findIndex_full((value, index, _array) =>
      callback(value, index),
    );
  }

  findIndex_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): int {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (callback(this.valuesStore[i]!, i, this.toArray())) {
        return i;
      }
    }
    return -1 as int;
  }

  findLast(callback: (value: T) => boolean): T | undefined;
  findLast(callback: (value: T, index: number) => boolean): T | undefined;
  findLast(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): T | undefined;
  findLast(callback: any): any {
    return this.findLast_full(callback);
  }

  findLast_value(callback: (value: T) => boolean): T | undefined {
    return this.findLast_full((value, _index, _array) => callback(value));
  }

  findLast_valueIndex(
    callback: (value: T, index: number) => boolean,
  ): T | undefined {
    return this.findLast_full((value, index, _array) => callback(value, index));
  }

  findLast_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): T | undefined {
    for (
      let i = (this.valuesStore.Count - 1) as int;
      i >= 0;
      i = (i - 1) as int
    ) {
      const value = this.valuesStore[i]!;
      if (callback(value, i, this.toArray())) {
        return value;
      }
    }
    return undefined;
  }

  findLastIndex(callback: (value: T) => boolean): int;
  findLastIndex(callback: (value: T, index: number) => boolean): int;
  findLastIndex(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): int;
  findLastIndex(callback: any): any {
    return this.findLastIndex_full(callback);
  }

  findLastIndex_value(callback: (value: T) => boolean): int {
    return this.findLastIndex_full((value, _index, _array) => callback(value));
  }

  findLastIndex_valueIndex(callback: (value: T, index: number) => boolean): int {
    return this.findLastIndex_full((value, index, _array) =>
      callback(value, index),
    );
  }

  findLastIndex_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): int {
    for (
      let i = (this.valuesStore.Count - 1) as int;
      i >= 0;
      i = (i - 1) as int
    ) {
      if (callback(this.valuesStore[i]!, i, this.toArray())) {
        return i;
      }
    }
    return -1 as int;
  }

  forEach(callback: (value: T) => void): void;
  forEach(callback: (value: T, index: number) => void): void;
  forEach(callback: (value: T, index: number, array: T[]) => void): void;
  forEach(callback: any): any {
    return this.forEach_full(callback);
  }

  forEach_value(callback: (value: T) => void): void {
    this.forEach_full((value, _index, _array) => callback(value));
  }

  forEach_valueIndex(callback: (value: T, index: number) => void): void {
    this.forEach_full((value, index, _array) => callback(value, index));
  }

  forEach_full(callback: (value: T, index: number, array: T[]) => void): void {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      callback(this.valuesStore[i]!, i, this.toArray());
    }
  }

  includes(searchElement: T): boolean {
    return this.indexOf(searchElement) >= 0;
  }

  indexOf(searchElement: T, fromIndex: int = 0 as int): int {
    const start = normalizeRelativeIndex(fromIndex, this.valuesStore.Count);
    for (let i = start; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (sameValueZero(this.valuesStore[i], searchElement)) {
        return i;
      }
    }
    return -1 as int;
  }

  join(separator: string = ","): string {
    let text = "";
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (i > 0) {
        text += separator;
      }
      text += coerceString(this.valuesStore[i] as ArrayRuntimeValue);
    }
    return text;
  }

  keys(): IterableIterator<int, undefined, undefined> {
    return (function* (self: Array<T>): Generator<int, undefined, undefined> {
      for (let i = 0 as int; i < self.length; i = (i + 1) as int) {
        yield i;
      }
    })(this);
  }

  lastIndexOf(searchElement: T, fromIndex?: int): int {
    const start =
      fromIndex === undefined
        ? ((this.valuesStore.Count - 1) as int)
        : normalizeElementIndex(fromIndex, this.valuesStore.Count);
    for (let i = start; i >= 0; i = (i - 1) as int) {
      if (sameValueZero(this.valuesStore[i], searchElement)) {
        return i;
      }
    }
    return -1 as int;
  }

  map<TResult>(callback: (value: T) => TResult): Array<TResult>;
  map<TResult>(callback: (value: T, index: number) => TResult): Array<TResult>;
  map<TResult>(
    callback: (value: T, index: number, array: T[]) => TResult,
  ): Array<TResult>;
  map(callback: any): any {
    return this.map_full(callback);
  }

  map_value<TResult>(callback: (value: T) => TResult): Array<TResult> {
    return this.map_full((value, _index, _array) => callback(value));
  }

  map_valueIndex<TResult>(
    callback: (value: T, index: number) => TResult,
  ): Array<TResult> {
    return this.map_full((value, index, _array) => callback(value, index));
  }

  map_full<TResult>(
    callback: (value: T, index: number, array: T[]) => TResult,
  ): Array<TResult> {
    const mapped = new List<TResult>();
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      mapped.Add(callback(this.valuesStore[i]!, i, this.toArray()));
    }
    return wrapArray<TResult>(mapped.ToArray());
  }

  flatMap<TResult>(callback: (value: T) => readonly TResult[]): Array<TResult>;
  flatMap<TResult>(
    callback: (value: T, index: number) => readonly TResult[],
  ): Array<TResult>;
  flatMap<TResult>(
    callback: (value: T, index: number, array: T[]) => readonly TResult[],
  ): Array<TResult>;
  flatMap(callback: any): any {
    return this.flatMap_full(callback);
  }

  flatMap_value<TResult>(
    callback: (value: T) => readonly TResult[],
  ): Array<TResult> {
    return this.flatMap_full((value, _index, _array) => callback(value));
  }

  flatMap_valueIndex<TResult>(
    callback: (value: T, index: number) => readonly TResult[],
  ): Array<TResult> {
    return this.flatMap_full((value, index, _array) => callback(value, index));
  }

  flatMap_full<TResult>(
    callback: (value: T, index: number, array: T[]) => readonly TResult[],
  ): Array<TResult> {
    const flattened = new List<TResult>();
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      const mappedValues = callback(this.valuesStore[i]!, i, this.toArray());
      for (const mappedValue of mappedValues) {
        flattened.Add(mappedValue);
      }
    }
    return wrapArray<TResult>(flattened.ToArray());
  }

  pop(): T {
    if (this.valuesStore.Count === 0) {
      return undefined as T;
    }
    const index = (this.valuesStore.Count - 1) as int;
    const value = this.valuesStore[index]!;
    this.valuesStore.RemoveAt(index);
    return value;
  }

  push(...items: T[]): int {
    for (const item of items) {
      this.valuesStore.Add(item);
    }
    return this.valuesStore.Count;
  }

  reduce(callback: (previousValue: T, currentValue: T) => T): T;
  reduce(callback: (previousValue: T, currentValue: T, index: number) => T): T;
  reduce(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
  ): T;
  reduce(
    callback: (previousValue: T, currentValue: T) => T,
    initialValue: T,
  ): T;
  reduce(
    callback: (previousValue: T, currentValue: T, index: number) => T,
    initialValue: T,
  ): T;
  reduce(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
    initialValue: T,
  ): T;
  reduce<TResult>(
    callback: (previousValue: TResult, currentValue: T) => TResult,
    initialValue: TResult,
  ): TResult;
  reduce<TResult>(
    callback: (previousValue: TResult, currentValue: T, index: number) => TResult,
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
  reduce(callback: any, initialValue?: any): any {
    throw new Error("stub");
  }

  reduce_pair_noInitial(callback: (previousValue: T, currentValue: T) => T): T {
    return this.reduce_full_noInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
    );
  }

  reduce_pairIndex_noInitial(
    callback: (previousValue: T, currentValue: T, index: number) => T,
  ): T {
    return this.reduce_full_noInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
    );
  }

  reduce_full_noInitial(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
  ): T {
    if (this.valuesStore.Count === 0) {
      throw new Error("Reduce of empty array with no initial value.");
    }

    let accumulator: T = this.valuesStore[0]!;
    for (
      let index = 1 as int;
      index < this.valuesStore.Count;
      index = (index + 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reduce_pair_sameInitial(
    callback: (previousValue: T, currentValue: T) => T,
    initialValue: T,
  ): T {
    return this.reduce_full_sameInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
      initialValue,
    );
  }

  reduce_pairIndex_sameInitial(
    callback: (previousValue: T, currentValue: T, index: number) => T,
    initialValue: T,
  ): T {
    return this.reduce_full_sameInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
      initialValue,
    );
  }

  reduce_full_sameInitial(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
    initialValue: T,
  ): T {
    let accumulator = initialValue;
    for (
      let index = 0 as int;
      index < this.valuesStore.Count;
      index = (index + 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reduce_pair_withInitial<TResult>(
    callback: (previousValue: TResult, currentValue: T) => TResult,
    initialValue: TResult,
  ): TResult {
    return this.reduce_full_withInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
      initialValue,
    );
  }

  reduce_pairIndex_withInitial<TResult>(
    callback: (previousValue: TResult, currentValue: T, index: number) => TResult,
    initialValue: TResult,
  ): TResult {
    return this.reduce_full_withInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
      initialValue,
    );
  }

  reduce_full_withInitial<TResult>(
    callback: (
      previousValue: TResult,
      currentValue: T,
      index: number,
      array: T[],
    ) => TResult,
    initialValue: TResult,
  ): TResult {
    let accumulator = initialValue;
    for (
      let index = 0 as int;
      index < this.valuesStore.Count;
      index = (index + 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reduceRight(callback: (previousValue: T, currentValue: T) => T): T;
  reduceRight(
    callback: (previousValue: T, currentValue: T, index: number) => T,
  ): T;
  reduceRight(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
  ): T;
  reduceRight(
    callback: (previousValue: T, currentValue: T) => T,
    initialValue: T,
  ): T;
  reduceRight(
    callback: (previousValue: T, currentValue: T, index: number) => T,
    initialValue: T,
  ): T;
  reduceRight(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
    initialValue: T,
  ): T;
  reduceRight<TResult>(
    callback: (previousValue: TResult, currentValue: T) => TResult,
    initialValue: TResult,
  ): TResult;
  reduceRight<TResult>(
    callback: (previousValue: TResult, currentValue: T, index: number) => TResult,
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
  reduceRight(callback: any, initialValue?: any): any {
    throw new Error("stub");
  }

  reduceRight_pair_noInitial(
    callback: (previousValue: T, currentValue: T) => T,
  ): T {
    return this.reduceRight_full_noInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
    );
  }

  reduceRight_pairIndex_noInitial(
    callback: (previousValue: T, currentValue: T, index: number) => T,
  ): T {
    return this.reduceRight_full_noInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
    );
  }

  reduceRight_full_noInitial(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
  ): T {
    if (this.valuesStore.Count === 0) {
      throw new Error("Reduce of empty array with no initial value.");
    }

    let accumulator: T = this.valuesStore[(this.valuesStore.Count - 1) as int]!;
    for (
      let index = (this.valuesStore.Count - 2) as int;
      index >= 0;
      index = (index - 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reduceRight_pair_sameInitial(
    callback: (previousValue: T, currentValue: T) => T,
    initialValue: T,
  ): T {
    return this.reduceRight_full_sameInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
      initialValue,
    );
  }

  reduceRight_pairIndex_sameInitial(
    callback: (previousValue: T, currentValue: T, index: number) => T,
    initialValue: T,
  ): T {
    return this.reduceRight_full_sameInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
      initialValue,
    );
  }

  reduceRight_full_sameInitial(
    callback: (previousValue: T, currentValue: T, index: number, array: T[]) => T,
    initialValue: T,
  ): T {
    let accumulator = initialValue;
    for (
      let index = (this.valuesStore.Count - 1) as int;
      index >= 0;
      index = (index - 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reduceRight_pair_withInitial<TResult>(
    callback: (previousValue: TResult, currentValue: T) => TResult,
    initialValue: TResult,
  ): TResult {
    return this.reduceRight_full_withInitial(
      (previousValue, currentValue, _index, _array) =>
        callback(previousValue, currentValue),
      initialValue,
    );
  }

  reduceRight_pairIndex_withInitial<TResult>(
    callback: (previousValue: TResult, currentValue: T, index: number) => TResult,
    initialValue: TResult,
  ): TResult {
    return this.reduceRight_full_withInitial(
      (previousValue, currentValue, index, _array) =>
        callback(previousValue, currentValue, index),
      initialValue,
    );
  }

  reduceRight_full_withInitial<TResult>(
    callback: (
      previousValue: TResult,
      currentValue: T,
      index: number,
      array: T[],
    ) => TResult,
    initialValue: TResult,
  ): TResult {
    let accumulator = initialValue;
    for (
      let index = (this.valuesStore.Count - 1) as int;
      index >= 0;
      index = (index - 1) as int
    ) {
      accumulator = callback(
        accumulator,
        this.valuesStore[index]!,
        index,
        this.toArray(),
      );
    }

    return accumulator;
  }

  reverse(): this {
    const reversed = cloneList(this.valuesStore);
    reversed.Reverse();
    this.valuesStore.Clear();
    appendList(this.valuesStore, reversed);
    return this;
  }

  shift(): T {
    if (this.valuesStore.Count === 0) {
      return undefined as T;
    }
    const value = this.valuesStore[0]!;
    this.valuesStore.RemoveAt(0 as int);
    return value;
  }

  slice(start?: int, end?: int): Array<T> {
    const length = this.valuesStore.Count;
    const from = normalizeRelativeIndex(start ?? (0 as int), length);
    const to = normalizeRelativeIndex(end ?? length, length);
    const values = new List<T>();
    for (let i = from; i < to; i = (i + 1) as int) {
      values.Add(this.valuesStore[i]!);
    }
    return wrapArray<T>(values.ToArray());
  }

  some(callback: (value: T) => boolean): boolean;
  some(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): boolean;
  some(callback: any): any {
    return this.some_full(callback);
  }

  some_value(callback: (value: T) => boolean): boolean {
    return this.some_full((value, _index, _array) => callback(value));
  }

  some_full(
    callback: (value: T, index: number, array: readonly T[]) => boolean,
  ): boolean {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (callback(this.valuesStore[i]!, i, this.toArray())) {
        return true;
      }
    }
    return false;
  }

  sort(compareFunc?: (left: T, right: T) => double): this {
    const values = this.toArray();
    for (let left = 0 as int; left < values.length; left = (left + 1) as int) {
      for (
        let right = (left + 1) as int;
        right < values.length;
        right = (right + 1) as int
      ) {
        const leftValue = values[left]!;
        const rightValue = values[right]!;
        const comparison =
          compareFunc?.(leftValue, rightValue) ??
          (coerceString(leftValue as ArrayRuntimeValue) <
          coerceString(rightValue as ArrayRuntimeValue)
            ? (-1 as double)
            : coerceString(leftValue as ArrayRuntimeValue) >
                coerceString(rightValue as ArrayRuntimeValue)
              ? (1 as double)
              : (0 as double));
        if (comparison > 0) {
          values[left] = rightValue;
          values[right] = leftValue;
        }
      }
    }
    this.valuesStore.Clear();
    for (const value of values) {
      this.valuesStore.Add(value);
    }
    return this;
  }

  splice(
    start: int,
    deleteCount: int = (this.valuesStore.Count - start) as int,
    ...items: T[]
  ): Array<T> {
    const length = this.valuesStore.Count;
    const from = normalizeRelativeIndex(start, length);
    const removeCount = deleteCount < 0 ? (0 as int) : deleteCount;
    const removed = new List<T>();
    for (
      let i = 0 as int;
      i < removeCount && from < this.valuesStore.Count;
      i = (i + 1) as int
    ) {
      removed.Add(this.valuesStore[from]!);
      this.valuesStore.RemoveAt(from);
    }
    for (let i = (items.length - 1) as int; i >= 0; i = (i - 1) as int) {
      this.valuesStore.Insert(from, items[i]!);
    }
    return wrapArray<T>(removed.ToArray());
  }

  toLocaleString(): string {
    return this.join(",");
  }

  toReversed(): Array<T> {
    return wrapArray<T>(this.toArray()).reverse();
  }

  toSorted(compareFunc?: (left: T, right: T) => double): Array<T> {
    return wrapArray<T>(this.toArray()).sort(compareFunc);
  }

  toSpliced(start: int, deleteCount?: int, ...items: T[]): Array<T> {
    const clone = wrapArray<T>(this.toArray());
    clone.splice(start, deleteCount, ...items);
    return clone;
  }

  toString(): string {
    return this.join(",");
  }

  unshift(...items: T[]): int {
    for (let i = (items.length - 1) as int; i >= 0; i = (i - 1) as int) {
      this.valuesStore.Insert(0 as int, items[i]!);
    }
    return this.valuesStore.Count;
  }

  values(): IterableIterator<T, undefined, undefined> {
    return (function* (self: Array<T>): Generator<T, undefined, undefined> {
      for (let i = 0 as int; i < self.length; i = (i + 1) as int) {
        yield self.valuesStore[i]!;
      }
    })(this);
  }

  with(index: int, value: T): Array<T> {
    const clone = wrapArray<T>(this.toArray());
    const normalized = normalizeElementIndex(index, clone.length);
    if (normalized >= 0) {
      clone.valuesStore[normalized] = value;
    }
    return clone;
  }

  [Symbol.iterator](): IterableIterator<T, undefined, undefined> {
    return this.values();
  }
}

O<Array>()
  .method((x) => x.map_value)
  .family((x) => x.map);
O<Array>()
  .method((x) => x.map_valueIndex)
  .family((x) => x.map);
O<Array>()
  .method((x) => x.map_full)
  .family((x) => x.map);
O<Array>()
  .method((x) => x.flatMap_value)
  .family((x) => x.flatMap);
O<Array>()
  .method((x) => x.flatMap_valueIndex)
  .family((x) => x.flatMap);
O<Array>()
  .method((x) => x.flatMap_full)
  .family((x) => x.flatMap);
O<Array>()
  .method((x) => x.every_value)
  .family((x) => x.every);
O<Array>()
  .method((x) => x.every_full)
  .family((x) => x.every);
O<Array>()
  .method((x) => x.filter_value)
  .family((x) => x.filter);
O<Array>()
  .method((x) => x.filter_valueIndex)
  .family((x) => x.filter);
O<Array>()
  .method((x) => x.filter_full)
  .family((x) => x.filter);
O<Array>()
  .method((x) => x.find_value)
  .family((x) => x.find);
O<Array>()
  .method((x) => x.find_valueIndex)
  .family((x) => x.find);
O<Array>()
  .method((x) => x.find_full)
  .family((x) => x.find);
O<Array>()
  .method((x) => x.findIndex_value)
  .family((x) => x.findIndex);
O<Array>()
  .method((x) => x.findIndex_valueIndex)
  .family((x) => x.findIndex);
O<Array>()
  .method((x) => x.findIndex_full)
  .family((x) => x.findIndex);
O<Array>()
  .method((x) => x.findLast_value)
  .family((x) => x.findLast);
O<Array>()
  .method((x) => x.findLast_valueIndex)
  .family((x) => x.findLast);
O<Array>()
  .method((x) => x.findLast_full)
  .family((x) => x.findLast);
O<Array>()
  .method((x) => x.findLastIndex_value)
  .family((x) => x.findLastIndex);
O<Array>()
  .method((x) => x.findLastIndex_valueIndex)
  .family((x) => x.findLastIndex);
O<Array>()
  .method((x) => x.findLastIndex_full)
  .family((x) => x.findLastIndex);
O<Array>()
  .method((x) => x.forEach_value)
  .family((x) => x.forEach);
O<Array>()
  .method((x) => x.forEach_valueIndex)
  .family((x) => x.forEach);
O<Array>()
  .method((x) => x.forEach_full)
  .family((x) => x.forEach);
O<Array>()
  .method((x) => x.some_value)
  .family((x) => x.some);
O<Array>()
  .method((x) => x.some_full)
  .family((x) => x.some);

O<typeof Array>()
  .method((x) => x.from_string)
  .family((x) => x.from);
O<typeof Array>()
  .method((x) => x.from_stringMapped)
  .family((x) => x.from);
O<typeof Array>()
  .method((x) => x.from_iterable)
  .family((x) => x.from);
O<typeof Array>()
  .method((x) => x.from_iterableMapped)
  .family((x) => x.from);

O<Array>()
  .method((x) => x.reduce_pair_noInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_pairIndex_noInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_full_noInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_pair_sameInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_pairIndex_sameInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_full_sameInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_pair_withInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_pairIndex_withInitial)
  .family((x) => x.reduce);
O<Array>()
  .method((x) => x.reduce_full_withInitial)
  .family((x) => x.reduce);

O<Array>()
  .method((x) => x.reduceRight_pair_noInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_pairIndex_noInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_full_noInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_pair_sameInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_pairIndex_sameInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_full_sameInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_pair_withInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_pairIndex_withInitial)
  .family((x) => x.reduceRight);
O<Array>()
  .method((x) => x.reduceRight_full_withInitial)
  .family((x) => x.reduceRight);
