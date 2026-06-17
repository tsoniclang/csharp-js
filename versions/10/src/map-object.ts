import type { int } from "@tsonic/core/types.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { sameValueZero } from "./same-value-zero.js";

class MapEntry<K, V> {
  key: K;
  value: V;

  constructor(key: K, value: V) {
    this.key = key;
    this.value = value;
  }
}

export interface ReadonlyMap<K, V> {
  readonly size: int;
  entries(): Generator<[K, V], undefined, undefined>;
  forEach(callback: (value: V, key: K, map: ReadonlyMap<K, V>) => void): void;
  get(key: K): V | undefined;
  has(key: K): boolean;
  keys(): Generator<K, undefined, undefined>;
  values(): Generator<V, undefined, undefined>;
  [Symbol.iterator](): Generator<[K, V], undefined, undefined>;
}

function castReadonlyMapValue<T>(value: unknown): T {
  return value as T;
}

export class Map<K, V> implements ReadonlyMap<K, V> {
  entriesStore: List<MapEntry<K, V>> = new List<
    MapEntry<K, V>
  >();

  constructor(entries?: Iterable<readonly [K, V]> | null) {
    if (!entries) {
      return;
    }

    for (const [key, value] of entries) {
      this.set(key, value);
    }
  }

  get size(): int {
    return this.entriesStore.Count;
  }

  findIndex(key: K): int {
    for (let i = 0 as int; i < this.entriesStore.Count; i = (i + 1) as int) {
      if (sameValueZero(this.entriesStore[i]!.key, key)) {
        return i;
      }
    }
    return -1 as int;
  }

  clear(): void {
    this.entriesStore.Clear();
  }

  delete(key: K): boolean {
    const index = this.findIndex(key);
    if (index < 0) {
      return false;
    }
    this.entriesStore.RemoveAt(index);
    return true;
  }

  *entries(): Generator<[K, V], undefined, undefined> {
    for (let i = 0 as int; i < this.entriesStore.Count; i = (i + 1) as int) {
      const entry = this.entriesStore[i]!;
      yield [entry.key, entry.value];
    }
  }

  forEach(callback: (value: V, key: K, map: ReadonlyMap<K, V>) => void): void {
    for (let i = 0 as int; i < this.entriesStore.Count; i = (i + 1) as int) {
      const entry = this.entriesStore[i]!;
      callback(entry.value, entry.key, this);
    }
  }

  get(key: K): V | undefined {
    const index = this.findIndex(key);
    return index < 0 ? undefined : this.entriesStore[index]!.value;
  }

  has(key: K): boolean {
    return this.findIndex(key) >= 0;
  }

  *keys(): Generator<K, undefined, undefined> {
    for (let i = 0 as int; i < this.entriesStore.Count; i = (i + 1) as int) {
      yield this.entriesStore[i]!.key;
    }
  }

  set(key: K, value: V): this {
    const index = this.findIndex(key);
    if (index >= 0) {
      this.entriesStore[index]!.value = value;
      return this;
    }

    this.entriesStore.Add(new MapEntry(key, value));
    return this;
  }

  *values(): Generator<V, undefined, undefined> {
    for (let i = 0 as int; i < this.entriesStore.Count; i = (i + 1) as int) {
      yield this.entriesStore[i]!.value;
    }
  }

  [Symbol.iterator](): Generator<[K, V], undefined, undefined> {
    return this.entries();
  }
}

export class ReadonlyMapView<K, VSource, V> implements ReadonlyMap<K, V> {
  private readonly source: ReadonlyMap<K, VSource>;

  constructor(source: ReadonlyMap<K, VSource>) {
    this.source = source;
  }

  get size(): int {
    return this.source.size;
  }

  *entries(): Generator<[K, V], undefined, undefined> {
    for (const [key, value] of this.source.entries()) {
      yield [key, castReadonlyMapValue<V>(value)];
    }
  }

  forEach(callback: (value: V, key: K, map: ReadonlyMap<K, V>) => void): void {
    this.source.forEach((value, key) => {
      callback(castReadonlyMapValue<V>(value), key, this);
    });
  }

  get(key: K): V | undefined {
    const value = this.source.get(key);
    return value === undefined ? undefined : castReadonlyMapValue<V>(value);
  }

  has(key: K): boolean {
    return this.source.has(key);
  }

  keys(): Generator<K, undefined, undefined> {
    return this.source.keys();
  }

  *values(): Generator<V, undefined, undefined> {
    for (const value of this.source.values()) {
      yield castReadonlyMapValue<V>(value);
    }
  }

  [Symbol.iterator](): Generator<[K, V], undefined, undefined> {
    return this.entries();
  }
}
