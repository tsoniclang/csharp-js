# QuickJS-Derived RegExp Engine

This directory is adapted from Jint's self-contained QuickJS `libregexp` port
at commit `9817530a8a6a138835eee0be623ccfbee680be6b`.

The underlying engine and Unicode data are MIT-licensed. The original notices
are retained in the adapted source files. Tsonic replaces Jint's wall-clock
deadline with a deterministic checked interpreter-step budget and exposes the
engine only through the closed `Tsonic.CSharp.Js.RegExp` runtime contract.
