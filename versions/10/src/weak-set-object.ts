import type { int } from "@tsonic/core/types.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { sameValueZero } from "./same-value-zero.js";

export class WeakSet<T extends object> {
  valuesStore: List<T> = new List<T>();

  constructor(values?: readonly T[] | null) {
    if (!values) {
      return;
    }

    for (const value of values) {
      this.add(value);
    }
  }

  findIndex(value: T): int {
    for (let index = 0 as int; index < this.valuesStore.Count; index = (index + 1) as int) {
      if (sameValueZero(this.valuesStore[index], value)) {
        return index;
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

  delete(value: T): boolean {
    const index = this.findIndex(value);
    if (index < 0) {
      return false;
    }

    this.valuesStore.RemoveAt(index);
    return true;
  }

  has(value: T): boolean {
    return this.findIndex(value) >= 0;
  }
}
