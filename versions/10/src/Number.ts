import { Double } from "@tsonic/dotnet/System.js";
import { CultureInfo } from "@tsonic/dotnet/System.Globalization.js";

export const toString = (value: number): string =>
  (value as Double).ToString(CultureInfo.InvariantCulture) ?? "";

export const valueOf = (value: number): number => value;
