import type { int } from "@tsonic/core/types.js";

type IteratorError = string | number | boolean | object | null;

export class IndexIterator<T> implements IterableIterator<T> {
  index: int = 0 as int;

  constructor(
    getLength: () => int,
    getValue: (index: int) => T
  ) {
  }

  next(): IteratorResult<T, undefined> {
    if (this.index >= this.getLength()) {
      return {
        done: true,
        value: undefined,
      };
    }

    const current = this.index;
    this.index = (this.index + 1) as int;
    return {
      done: false,
      value: this.getValue(current),
    };
  }

  return(value?: undefined): IteratorResult<T, undefined> {
    this.index = this.getLength();
    return {
      done: true,
      value,
    };
  }

  throw(error?: IteratorError): IteratorResult<T, undefined> {
    throw error;
  }

  [Symbol.iterator](): IterableIterator<T> {
    return this;
  }
}
