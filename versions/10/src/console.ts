import { Console as DotnetConsole } from "@tsonic/dotnet/System.js";
import { String as coerceString } from "./Globals.js";

type ConsoleValue = string | number | boolean | object | null | undefined;

export function formatArgs(values: readonly ConsoleValue[]): string {
  let result = "";
  for (let i = 0; i < values.length; i += 1) {
    if (i > 0) {
      result += " ";
    }
    result += coerceString(values[i]);
  }
  return result;
}

export function log(...data: ConsoleValue[]): void {
  DotnetConsole.WriteLine(formatArgs(data));
}

export function error(...data: ConsoleValue[]): void {
  DotnetConsole.Error.WriteLine(formatArgs(data));
}

export function warn(...data: ConsoleValue[]): void {
  DotnetConsole.WriteLine(`WARN: ${formatArgs(data)}`);
}

export function info(...data: ConsoleValue[]): void {
  log(...data);
}

export function debug(...data: ConsoleValue[]): void {
  log(...data);
}
