import { EqualityComparer } from "@tsonic/dotnet/System.Collections.Generic.js";

export const sameValueZero = <T>(left: T, right: T): boolean =>
  EqualityComparer.Default<T>().Equals(left, right);
