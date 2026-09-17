# `@tsonic/csharp-js`

C# runtime implementation for Tsonic's explicitly selected JavaScript source
surface. It owns the closed C# implementations of supported JavaScript
globals and built-ins and depends on `@tsonic/csharp-runtime`.

Canonical product documentation:

- [JavaScript source profile](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/javascript-source-profile.md)
- [C# JavaScript surface](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/targets/csharp/javascript-surface.md)
- [C# support inventory](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/targets/csharp/support-inventory.md)

The npm package contains C# source and the native project exported as
`@tsonic/csharp-js/runtime.csproj`. MSBuild compiles it and its core-runtime
dependency for the application's selected framework.
Compiler-intrinsic carriers remain owned by `@tsonic/csharp-runtime`.
