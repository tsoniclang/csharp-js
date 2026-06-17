import { Error } from "./error-object.js";

export class RangeError extends Error {
  override name: string = "RangeError";

  constructor(message?: string) {
    super(message);
  }
}
