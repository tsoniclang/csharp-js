import type { int } from "@tsonic/core/types.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { sameValueZero } from "./same-value-zero.js";

class WeakMapEntry<K extends object, V> {
  key: K;
  value: V;

  constructor(key: K, value: V) {
    this.key = key;
    this.value = value;
  }
}

export class WeakMap<K extends object, V> {
  entriesStore: List<WeakMapEntry<K, V>> = new List<
    WeakMapEntry<K, V>
  >();

  constructor(entries?: readonly (readonly [K, V])[] | null) {
    if (!entries) {
      return;
    }

    for (const [key, value] of entries) {
      this.set(key, value);
    }
  }

  findIndex(key: K): int {
    for (let index = 0 as int; index < this.entriesStore.Count; index = (index + 1) as int) {
      if (sameValueZero(this.entriesStore[index]!.key, key)) {
        return index;
      }
    }

    return -1 as int;
  }

  delete(key: K): boolean {
    const index = this.findIndex(key);
    if (index < 0) {
      return false;
    }

    this.entriesStore.RemoveAt(index);
    return true;
  }

  get(key: K): V | undefined {
    const index = this.findIndex(key);
    return index < 0 ? undefined : this.entriesStore[index]!.value;
  }

  has(key: K): boolean {
    return this.findIndex(key) >= 0;
  }

  set(key: K, value: V): this {
    const index = this.findIndex(key);
    if (index >= 0) {
      this.entriesStore[index]!.value = value;
      return this;
    }

    this.entriesStore.Add(new WeakMapEntry(key, value));
    return this;
  }
}
