import { Exception } from "@tsonic/dotnet/System.js";

export class Error extends Exception {
  name: string = "Error";
  message: string;
  stack?: string;

  constructor(message?: string) {
    super(message ?? "");
    this.message = message ?? "";
  }

  toString(): string {
    if (this.message === "") {
      return this.name;
    }

    return `${this.name}: ${this.message}`;
  }
}
