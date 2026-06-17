import { Encoding } from "@tsonic/dotnet/System.Text.js";
import { Uint8Array } from "./uint8-array.js";

export class TextEncoder {
  get encoding(): string {
    return "utf-8";
  }

  encode(input: string = ""): Uint8Array {
    return Uint8Array.fromByteArrayRaw(Encoding.UTF8.GetBytes(input));
  }
}
