import { debug, error, info, log, warn } from "./console.js";

type ConsoleValue = string | number | boolean | object | null | undefined;

class ConsoleModule {
  static debug(...data: ConsoleValue[]): void {
    debug(...data);
  }

  static error(...data: ConsoleValue[]): void {
    error(...data);
  }

  static info(...data: ConsoleValue[]): void {
    info(...data);
  }

  static log(...data: ConsoleValue[]): void {
    log(...data);
  }

  static warn(...data: ConsoleValue[]): void {
    warn(...data);
  }
}

export { ConsoleModule as console };
