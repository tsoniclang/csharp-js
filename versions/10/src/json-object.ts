import type { int } from "@tsonic/core/types.js";

export class JSON {
  static parse<T>(_text: string): T {
    throw new Error("JSON.parse<T> must be lowered through compiler-generated serialization metadata.");
  }

  static stringify<T>(
    _value: T,
    _replacer?: undefined,
    _space?: string | number | int
  ): string {
    throw new Error("JSON.stringify<T> must be lowered through compiler-generated serialization metadata.");
  }
}
