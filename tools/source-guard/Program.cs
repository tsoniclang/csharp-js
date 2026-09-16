using System;
using System.IO;
using System.Text.Json;
using Tsonic.CSharp.TestTools;

var files = JsonSerializer.Deserialize<string[]>(Console.In.ReadToEnd());
if (files is null || files.Length == 0)
    throw new ArgumentException("Source guard requires explicit source paths.");
var failed = false;
foreach (var file in files)
{
    var violations = ForbiddenSyntax.FindForbiddenSyntax(File.ReadAllText(file));
    if (violations.Count == 0) continue;
    Console.Error.WriteLine($"{file}: forbidden runtime syntax: {string.Join(", ", violations)}");
    failed = true;
}
return failed ? 1 : 0;
