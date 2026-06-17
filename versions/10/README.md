# @tsonic/js

JavaScript runtime bindings for **Tsonic**.

This package is part of Tsonic: https://tsonic.org.

`@tsonic/js` provides JS runtime APIs (`JSON`, `console`, `Map`, `Set`, `Date`, timers, etc.) for Tsonic projects.

## Target support

`@tsonic/js` supports the default C# target. The package declares this in
`tsonic.package.json` and `tsonic.surface.json` with
`"supportedTargets": ["csharp"]`. User code imports stay target-neutral:
applications use `@tsonic/js` and do not add a target suffix.

## Prerequisites

- Install the .NET 10 SDK: https://dotnet.microsoft.com/download
- Verify: `dotnet --version`

## Quick Start (surface-first, no `@tsonic/js` imports required)

```bash
mkdir my-app && cd my-app
npx --yes tsonic@latest init --surface @tsonic/js
```

```ts
export function main(): void {
  const value = JSON.parse<{ x: number }>('{"x": 1}');
  console.log(JSON.stringify(value));
}
```

Build/run:

```bash
npm run dev
```

## Existing project

```bash
npx --yes tsonic@latest add npm @tsonic/js
```

If the workspace is not already JS surface, set `surface` in
`tsonic.workspace.json` to `@tsonic/js`:

```json
{
  "surface": "@tsonic/js"
}
```

## Optional direct imports

Surface mode enables natural JS authoring, but explicit subpath imports remain supported:

```ts
import { Date } from "@tsonic/js/index.js";
import { Math } from "@tsonic/js/index.js";
```

## Core APIs

- `console`
- `JSON`
- `Map`, `Set`, `WeakMap`, `WeakSet`
- `Date`, `Math`, `RegExp`, `Number`, `String`
- `Timers`
- globals like `parseInt`, `parseFloat`, `encodeURI`

## Typed JSON and broad values

`JSON.parse<T>()` and `JSON.stringify<T>()` are compiler-lowered APIs. Use a
closed compile-time type for JSON payloads so Tsonic can root generated
serialization metadata for NativeAOT:

```ts
type Payload = { ok: boolean; count: number };

const payload = JSON.parse<Payload>('{"ok":true,"count":2}');
console.log(JSON.stringify(payload));
```

Use `unknown` for deliberately broad JS values and narrow before member access.
Do not use `JsValue` as an application-level JSON or object escape hatch.

## Relationship to `@tsonic/nodejs`

- `@tsonic/js` = JS runtime surface
- `@tsonic/nodejs` = Node-style modules (`node:fs`, `node:path`, `node:crypto`, ...)

## Versioning

- `10` → `versions/10/` → npm: `@tsonic/js@10.x`

Publish:

```bash
npm publish versions/10 --access public
```

## Development

Run validation from the repository root:

```bash
npm run selftest
```

The selftest builds the package fixture and runs the JS API matrix for arrays,
typed arrays, JSON, maps, sets, weak collections, numbers, strings, dates,
timers, errors, console, globals, and regular expressions.

## License

MIT
