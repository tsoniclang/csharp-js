import type { int } from "@tsonic/core/types.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { sameValueZero } from "./same-value-zero.js";

export interface ReadonlySet<T> {
  readonly size: int;
  entries(): Generator<[T, T], undefined, undefined>;
  forEach(callback: (value: T, key: T, set: ReadonlySet<T>) => void): void;
  has(value: T): boolean;
  keys(): Generator<T, undefined, undefined>;
  values(): Generator<T, undefined, undefined>;
  [Symbol.iterator](): Generator<T, undefined, undefined>;
}

export class Set<T> implements ReadonlySet<T> {
  valuesStore: List<T> = new List<T>();

  constructor(values?: Iterable<T> | null) {
    if (!values) {
      return;
    }

    for (const value of values) {
      this.add(value);
    }
  }

  get size(): int {
    return this.valuesStore.Count;
  }

  findIndex(value: T): int {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      if (sameValueZero(this.valuesStore[i], value)) {
        return i;
      }
    }
    return -1 as int;
  }

  add(value: T): this {
    if (!this.has(value)) {
      this.valuesStore.Add(value);
    }
    return this;
  }

  clear(): void {
    this.valuesStore.Clear();
  }

  delete(value: T): boolean {
    const index = this.findIndex(value);
    if (index < 0) {
      return false;
    }
    this.valuesStore.RemoveAt(index);
    return true;
  }

  *entries(): Generator<[T, T], undefined, undefined> {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      const value = this.valuesStore[i]!;
      yield [value, value];
    }
  }

  forEach(callback: (value: T, key: T, set: ReadonlySet<T>) => void): void {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      const value = this.valuesStore[i]!;
      callback(value, value, this);
    }
  }

  has(value: T): boolean {
    return this.findIndex(value) >= 0;
  }

  keys(): Generator<T, undefined, undefined> {
    return this.values();
  }

  *values(): Generator<T, undefined, undefined> {
    for (let i = 0 as int; i < this.valuesStore.Count; i = (i + 1) as int) {
      yield this.valuesStore[i]!;
    }
  }

  [Symbol.iterator](): Generator<T, undefined, undefined> {
    return this.values();
  }
}
