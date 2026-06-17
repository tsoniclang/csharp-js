# @tsonic/csharp-js

C# implementation package for the portable `@tsonic/js` source surface.

This package owns .NET-backed JavaScript surface behavior for the `csharp`
target only. It may import `@tsonic/dotnet` and C# runtime packages. The
portable `@tsonic/js` package must not.

User source imports remain target-neutral:

```ts
import { console } from "@tsonic/js/console.js";

console.log("hello");
```

The C# target maps that source operation to this package when building for
`--target csharp`.
