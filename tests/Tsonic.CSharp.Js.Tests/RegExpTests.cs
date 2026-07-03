using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class RegExpTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void RegExp_Constructor_ExposesCanonicalFlagsAndSource()
    {
        var regexp = new RegExp("/", "migsy");

        Assert.Equal("\\/", regexp.source);
        Assert.Equal("gimsy", regexp.flags);
        Assert.True(regexp.global);
        Assert.True(regexp.ignoreCase);
        Assert.True(regexp.multiline);
        Assert.True(regexp.dotAll);
        Assert.True(regexp.sticky);
        Assert.False(regexp.hasIndices);
        Assert.False(regexp.unicode);
        Assert.False(regexp.unicodeSets);
        Assert.Equal("/\\//gimsy", regexp.toString());
        Assert.Equal("(?:)", new RegExp("").source);
    }

    [Fact]
    public void RegExp_InvalidAndUnsupportedFlags_FailDeterministically()
    {
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("x", "gg"));
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("x", "q"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("x", "d"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("x", "u"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("x", "v"));
    }

    [Fact]
    public void RegExp_UnsupportedSyntax_FailsBeforeDotNetRegexExecution()
    {
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("(?<name>a)"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("(?<=a)b"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("\\p{Letter}"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("(a)\\1"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("(?>a)"));
        Assert.Throws<JsRegExpUnsupportedException>(() => new RegExp("(?i:a)"));
    }

    [Fact]
    public void RegExp_InvalidPatterns_ThrowSyntaxErrors()
    {
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("["));
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("\\"));
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("("));
    }

    [Fact]
    public void RegExp_SupportedCases_MatchNodeOracle()
    {
        var cases = new[]
        {
            OracleCase("literal-ignore-case", "abc", "i", Test("ABC")),
            OracleCase("global-test-last-index", "a", "g", Test("a"), Test("a")),
            OracleCase("global-exec-sequence-reset", "a", "g", Exec("aba"), Exec("aba"), Exec("aba")),
            OracleCase("sticky-exec", "abc", "y", Exec("xyzabcdef", 3)),
            OracleCase("sticky-fail-reset", "a", "y", Exec("ba", 1), Exec("ba", 1)),
            OracleCase("dot-all", "a.b", "s", Test("a\nb")),
            OracleCase("dot-without-dot-all", "a.b", "", Test("a\nb")),
            OracleCase("multiline", "^abc", "m", Test("xyz\nabc")),
            OracleCase("captures", "([a-z]+)-(\\d+)", "", Exec("id abc-123 done")),
            OracleCase("optional-capture", "(a)(b)?", "", Exec("a")),
            OracleCase("noncapturing-group", "(?:ab)+", "", Exec("xxabab")),
            OracleCase("class-and-alternation", "[A-Z]+|\\d+", "", Exec("abc 123")),
            OracleCase("lookahead", "a(?=b)", "", Exec("ab")),
            OracleCase("escaped-slash-source", "\\/", "g", Test("/")),
            OracleCase("empty-source", "", "", Test("abc")),
        };

        foreach (var testCase in cases)
        {
            var oracle = RunNodeOracle(testCase);
            Assert.Equal("ok", oracle.GetProperty("kind").GetString());

            var regexp = new RegExp(testCase.Pattern, testCase.Flags);
            Assert.Equal(oracle.GetProperty("source").GetString(), regexp.source);
            Assert.Equal(oracle.GetProperty("flags").GetString(), regexp.flags);
            Assert.Equal(oracle.GetProperty("global").GetBoolean(), regexp.global);
            Assert.Equal(oracle.GetProperty("ignoreCase").GetBoolean(), regexp.ignoreCase);
            Assert.Equal(oracle.GetProperty("multiline").GetBoolean(), regexp.multiline);
            Assert.Equal(oracle.GetProperty("dotAll").GetBoolean(), regexp.dotAll);
            Assert.Equal(oracle.GetProperty("sticky").GetBoolean(), regexp.sticky);

            var observations = oracle.GetProperty("observations").EnumerateArray();
            var index = 0;
            foreach (var observation in observations)
            {
                var operation = testCase.Operations[index];
                if (operation.LastIndex is not null)
                {
                    regexp.lastIndex = operation.LastIndex.Value;
                }

                if (operation.Kind == "test")
                {
                    Assert.Equal(observation.GetProperty("result").GetBoolean(), regexp.test(operation.Input));
                    Assert.Equal(observation.GetProperty("lastIndex").GetInt32(), regexp.lastIndex);
                }
                else
                {
                    AssertMatchEqual(observation.GetProperty("result"), regexp.exec(operation.Input));
                    Assert.Equal(observation.GetProperty("lastIndex").GetInt32(), regexp.lastIndex);
                }
                index += 1;
            }
        }
    }

    [Fact]
    public void RegExp_NullInput_FailsDeterministically()
    {
        var regexp = new RegExp("abc");

        Assert.Throws<ArgumentNullException>(() => regexp.test(null!));
        Assert.Throws<ArgumentNullException>(() => regexp.exec(null!));
    }

    [Fact]
    public void RegExpMatchResult_Groups_AreReadOnlyAndCopied()
    {
        var result = new RegExp("(a)(b)?").exec("a");

        Assert.NotNull(result);
        Assert.Equal(3, result!.length);
        Assert.Equal("a", result[0]);
        Assert.Equal("a", result[1]);
        Assert.Null(result[2]);
        var copy = result.ToArray();
        copy[0] = "mutated";
        Assert.Equal("a", result[0]);
    }

    private static OracleTestCase OracleCase(string id, string pattern, string flags, params OracleOperation[] operations) =>
        new(id, pattern, flags, operations);

    private static OracleOperation Test(string input, int? lastIndex = null) =>
        new("test", input, lastIndex);

    private static OracleOperation Exec(string input, int? lastIndex = null) =>
        new("exec", input, lastIndex);

    private static JsonElement RunNodeOracle(OracleTestCase testCase)
    {
        var root = FindRepositoryRoot();
        var script = Path.Combine(root, "tools", "node-regexp-oracle.mjs");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };
        process.StartInfo.ArgumentList.Add(script);
        process.Start();
        process.StandardInput.Write(JsonSerializer.Serialize(new
        {
            pattern = testCase.Pattern,
            flags = testCase.Flags,
            operations = testCase.Operations,
        }, JsonOptions));
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Node oracle failed with exit code {process.ExitCode}: {stderr}");
        }
        using var document = JsonDocument.Parse(stdout);
        return document.RootElement.Clone();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "tools", "node-regexp-oracle.mjs")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not find repository root containing tools/node-regexp-oracle.mjs.");
    }

    private static void AssertMatchEqual(JsonElement expected, RegExpMatchResult? actual)
    {
        if (expected.ValueKind == JsonValueKind.Null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        Assert.Equal(expected.GetProperty("value").GetString(), actual!.value);
        Assert.Equal(expected.GetProperty("index").GetInt32(), actual.index);
        Assert.Equal(expected.GetProperty("input").GetString(), actual.input);
        Assert.Equal(expected.GetProperty("length").GetInt32(), actual.length);
        var groupIndex = 0;
        foreach (var expectedGroup in expected.GetProperty("groups").EnumerateArray())
        {
            var expectedValue = expectedGroup.ValueKind == JsonValueKind.Null ? null : expectedGroup.GetString();
            Assert.Equal(expectedValue, actual[groupIndex]);
            groupIndex += 1;
        }
    }

    private sealed record OracleTestCase(string Id, string Pattern, string Flags, IReadOnlyList<OracleOperation> Operations);

    private sealed record OracleOperation(string Kind, string Input, int? LastIndex);
}
