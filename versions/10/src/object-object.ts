import { overloads as O } from "@tsonic/core/lang.js";
import type { JsValue } from "@tsonic/core/types.js";
import type { Dictionary } from "@tsonic/dotnet/System.Collections.Generic.js";
import { List } from "@tsonic/dotnet/System.Collections.Generic.js";
import { Double } from "@tsonic/dotnet/System.js";

type ObjectIsValue =
  | string
  | number
  | bigint
  | boolean
  | object
  | null
  | undefined;

export class Object {
  static entries(obj: Record<string, JsValue>): [string, JsValue][];
  static entries<T>(obj: Record<string, T>): [string, T][];
  static entries(obj: object): [string, JsValue][];
  static entries(_obj: object): [string, JsValue][] {
    throw new Error("Unreachable overload stub");
  }

  static is(value1: ObjectIsValue, value2: ObjectIsValue): boolean {
    if (typeof value1 === "number" && typeof value2 === "number") {
      if (Double.IsNaN(value1) && Double.IsNaN(value2)) {
        return true;
      }
      if (value1 === 0 && value2 === 0) {
        return Double.IsNegativeInfinity(1.0 / value1) ===
          Double.IsNegativeInfinity(1.0 / value2);
      }
    }
    return value1 === value2;
  }

  private static entries_from_record<T>(
    source: Record<string, T>,
    dictionary: Dictionary<string, T>,
  ): [string, T][] {
    const result = new List<[string, T]>();
    const keys = dictionary.Keys.GetEnumerator();
    while (keys.MoveNext()) {
      const key = keys.Current;
      result.Add([key, source[key]!]);
    }
    return result.ToArray();
  }

  static entries_jsvalue_record(
    obj: Record<string, JsValue>,
  ): [string, JsValue][] {
    const dictionary = obj as object as Dictionary<string, JsValue>;
    return Object.entries_from_record(obj, dictionary);
  }

  static entries_record<T>(obj: Record<string, T>): [string, T][] {
    const dictionary = obj as object as Dictionary<string, T>;
    return Object.entries_from_record(obj, dictionary);
  }

  static entries_object(obj: object): [string, JsValue][] {
    const dictionary = obj as Dictionary<string, JsValue>;
    const source = obj as Record<string, JsValue>;
    return Object.entries_from_record(source, dictionary);
  }

  static keys<T>(obj: Record<string, T>): string[] {
    const dictionary = obj as object as Dictionary<string, T>;
    const result = new List<string>();
    const keys = dictionary.Keys.GetEnumerator();
    while (keys.MoveNext()) {
      result.Add(keys.Current);
    }
    return result.ToArray();
  }

  static values<T>(obj: Record<string, T>): T[] {
    const dictionary = obj as object as Dictionary<string, T>;
    const result = new List<T>();
    const keys = dictionary.Keys.GetEnumerator();
    while (keys.MoveNext()) {
      const key = keys.Current;
      result.Add(obj[key]!);
    }
    return result.ToArray();
  }
}

O<typeof Object>()
  .method((obj) => obj.entries_jsvalue_record)
  .family((obj) => obj.entries);
O<typeof Object>()
  .method((obj) => obj.entries_record)
  .family((obj) => obj.entries);
O<typeof Object>()
  .method((obj) => obj.entries_object)
  .family((obj) => obj.entries);
