using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class RegExpTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void ConstructionIdentityFlagsAndSourceAreExact()
    {
        var expression = new RegExp("/", "ymisgd");

        Assert.Equal("\\/", expression.source);
        Assert.Equal("dgimsy", expression.flags);
        Assert.True(expression.hasIndices);
        Assert.True(expression.global);
        Assert.True(expression.ignoreCase);
        Assert.True(expression.multiline);
        Assert.True(expression.dotAll);
        Assert.True(expression.sticky);
        Assert.False(expression.unicode);
        Assert.False(expression.unicodeSets);
        Assert.Equal("/\\//dgimsy", expression.toString());
        Assert.Equal("(?:)", new RegExp().source);

        var alias = RegExp.create(expression);
        var clone = new RegExp(expression);
        Assert.Same(expression, alias);
        Assert.NotSame(expression, clone);
        expression.lastIndex = 7;
        Assert.Equal(7, alias.lastIndex);
        Assert.Equal(0, clone.lastIndex);
    }

    [Fact]
    public void InvalidFlagsPatternsAndResourceLimitsFailPrecisely()
    {
        foreach (var flags in new[] { "gg", "uv", "q" })
        {
            Assert.Throws<JsRegExpSyntaxException>(() => new RegExp("a", flags));
        }
        foreach (var pattern in new[] { "[", "(", "\\", "a{2,1}" })
        {
            Assert.Throws<JsRegExpSyntaxException>(() => new RegExp(pattern, "u"));
        }
        Assert.Throws<RangeError>(() => new RegExp(new string('a', 1_048_577)));
    }

    [Fact]
    public void ModernGrammarMatchesTheNodeOracle()
    {
        var cases = new[]
        {
            OracleCase("lazy", "a+?", "", Exec("aaaa")),
            OracleCase("backreference", "(a)\\1", "", Exec("zaaz")),
            OracleCase("lookahead", "a(?=b)", "", Exec("zab")),
            OracleCase("lookbehind", "(?<=a)b", "", Exec("zab")),
            OracleCase("named", "(?<word>[a-z]+)(?<digits>\\d+)?", "d", Exec("abc")),
            OracleCase("unicode-property", "\\p{Script=Greek}+", "u", Exec("aαβz")),
            OracleCase("unicode-set", "[\\p{ASCII}&&\\p{Letter}]+", "v", Exec("éAb9")),
            OracleCase("modifiers", "(?i:a)b", "", Exec("Ab")),
            OracleCase("sticky", "b", "y", Exec("😀b", 2)),
            OracleCase("legacy-astral-quantifier", "💚+", "", Exec("a💚💚b")),
            OracleCase("nullable-mid-surrogate", "a*", "g", Exec("😀a", 1)),
        };

        foreach (var testCase in cases)
        {
            var oracle = RunNodeOracle(testCase);
            Assert.Equal("ok", oracle.GetProperty("kind").GetString());
            var expression = new RegExp(testCase.Pattern, testCase.Flags);
            Assert.Equal(oracle.GetProperty("source").GetString(), expression.source);
            Assert.Equal(oracle.GetProperty("flags").GetString(), expression.flags);
            Assert.Equal(oracle.GetProperty("hasIndices").GetBoolean(), expression.hasIndices);
            Assert.Equal(oracle.GetProperty("unicode").GetBoolean(), expression.unicode);
            Assert.Equal(oracle.GetProperty("unicodeSets").GetBoolean(), expression.unicodeSets);
            Assert.Equal(oracle.GetProperty("sticky").GetBoolean(), expression.sticky);

            var operation = testCase.Operations[0];
            if (operation.LastIndex is not null) expression.lastIndex = operation.LastIndex.Value;
            var expected = oracle.GetProperty("observations")[0];
            AssertMatchEqual(expected.GetProperty("result"), expression.exec(operation.Input));
            Assert.Equal(expected.GetProperty("lastIndex").GetDouble(), expression.lastIndex);
        }
    }

    [Fact]
    public void Utf16ModesAndIndicesUseCodeUnitOffsets()
    {
        const string astral = "😀";
        var legacy = new RegExp(".", "d").exec(astral)!;
        Assert.Equal("\uD83D", legacy.value);
        Assert.Equal((0d, 1d), legacy.indices![0]);

        var unicode = new RegExp(".", "du").exec(astral)!;
        Assert.Equal(astral, unicode.value);
        Assert.Equal((0d, 2d), unicode.indices![0]);

        var sticky = new RegExp("b", "y") { lastIndex = 2 };
        Assert.Equal("b", sticky.exec("😀b")!.value);
        Assert.Equal(3, sticky.lastIndex);
    }

    [Fact]
    public void NamedGroupsOptionalCapturesAndIndicesArePreserved()
    {
        var result = new RegExp("(?<letter>[a-z]+)(?<digits>\\d+)?", "d").exec("abc")!;

        Assert.Equal(3, result.length);
        Assert.Equal("abc", result[1]);
        Assert.Null(result[2]);
        Assert.Equal("abc", result.groups!["letter"]);
        Assert.Null(result.groups["digits"]);
        Assert.True(result.groups.ContainsKey("digits"));
        Assert.Equal((0d, 3d), result.indices![0]);
        Assert.Equal((0d, 3d), result.indices[1]);
        Assert.Null(result.indices[2]);
        Assert.Equal((0d, 3d), result.indices.groups!["letter"]);
        Assert.Null(result.indices.groups["digits"]);
    }

    [Fact]
    public void GlobalMatchAndLazyMatchAllUseIndependentState()
    {
        var expression = new RegExp("\\d+", "g") { lastIndex = 2 };
        var matched = "a1b22c333".match(expression)!;
        Assert.Equal(new[] { "1", "22", "333" }, matched.ToArray());
        Assert.Equal(0, expression.lastIndex);

        expression.lastIndex = 3;
        var iterator = "a1b22c333".matchAll(expression);
        Assert.Equal(3, expression.lastIndex);
        using var enumerator = iterator.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal("22", enumerator.Current.value);
        Assert.Equal(3, expression.lastIndex);
        Assert.True(enumerator.MoveNext());
        Assert.Equal("333", enumerator.Current.value);
        Assert.False(enumerator.MoveNext());

        Assert.Throws<TypeError>(() => "1".matchAll(new RegExp("\\d")));
        Assert.Single(new RegExp("\\d").matchAll("1"));
    }

    [Fact]
    public void ReplacementTokensAndCallbackArgumentsAreExact()
    {
        var expression = new RegExp("(?<digit>\\d)(x)?", "g");
        Assert.Equal("a<1>b<2>", expression.replace("a1b2x", "<$<digit>>"));

        var calls = new List<ReplacementObservation>();
        var output = expression.replace("a1b2x", arguments =>
        {
            var broadCapture = arguments.Get<Tsonic.CSharp.Runtime.TsValue>(1);
            Assert.True(Tsonic.CSharp.Runtime.TsValue.ApplyDynamicBinaryBoolean(
                broadCapture,
                "===",
                arguments.Get<string>(1)));
            var broadTail = arguments.Rest<Tsonic.CSharp.Runtime.TsValue>(1);
            Assert.Equal(arguments.length - 1, broadTail.length);
            Assert.True(Tsonic.CSharp.Runtime.TsValue.ApplyDynamicBinaryBoolean(
                broadTail[0],
                "===",
                arguments.Get<string>(1)));
            calls.Add(new ReplacementObservation(
                arguments.Get<string>(0),
                arguments.Get<string>(1),
                arguments.Get<object>(2),
                arguments.Get<double>(3),
                arguments.Get<string>(4),
                arguments.Get<RegExpNamedGroups>(5)));
            return "#";
        });
        Assert.Equal("a#b#", output);
        Assert.Equal(2, calls.Count);
        Assert.Equal(new ReplacementObservation(
            "1", "1", Undefined.value, 1, "a1b2x", calls[0].Groups), calls[0]);
        Assert.Equal("1", calls[0].Groups["digit"]);
        Assert.Same(Undefined.value, calls[0].OptionalCapture);
        Assert.Equal("x", calls[1].OptionalCapture);
    }

    [Fact]
    public void SplitSearchAndReplaceAllFollowStringProtocolRules()
    {
        Assert.Equal(
            new[] { "a", "1", "b", "22", "c" },
            "a1b22c".split(new RegExp("(\\d+)"), null).ToArray());
        Assert.Equal(
            new[] { "a", "1", "b" },
            "a1b22c".split(new RegExp("(\\d+)"), 3).ToArray());

        var expression = new RegExp("b", "g") { lastIndex = 7 };
        Assert.Equal(1, "ab".search(expression));
        Assert.Equal(7, expression.lastIndex);

        Assert.Equal("a#b#", "a1b2".replaceAll(
            new RegExp("\\d", "g"),
            _ => "#"));
        Assert.Throws<TypeError>(() => "a1".replaceAll(new RegExp("\\d"), "#"));
    }

    [Fact]
    public void EscapeUsesTheNormativeLiteralSafeForm()
    {
        Assert.Equal("\\x66oo\\x2dbar", RegExp.escape("foo-bar"));
        Assert.Equal("\\x61\\+b\\/c", RegExp.escape("a+b/c"));
        Assert.Equal("\\ud800", RegExp.escape("\uD800"));
    }

    [Fact]
    public void CompiledProgramsNeverShareMutableRegExpState()
    {
        var first = new RegExp("same", "g") { lastIndex = 3 };
        var second = new RegExp("same", "g");
        Assert.Equal(0, second.lastIndex);
        Assert.NotSame(first, second);

        for (var index = 0; index < 300; index += 1)
        {
            Assert.True(new RegExp($"cache{index}").test($"cache{index}"));
        }
        Assert.True(new RegExp("same", "g").test("same"));
    }

    private static OracleTestCase OracleCase(
        string id,
        string pattern,
        string flags,
        params OracleOperation[] operations) =>
        new(id, pattern, flags, operations);

    private static OracleOperation Exec(string input, double? lastIndex = null) =>
        new("exec", input, lastIndex);

    private static JsonElement RunNodeOracle(OracleTestCase testCase)
    {
        var root = FindRepositoryRoot();
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
        process.StartInfo.ArgumentList.Add(Path.Combine(root, "tools", "node-regexp-oracle.mjs"));
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
            throw new InvalidOperationException(
                $"Node oracle failed with exit code {process.ExitCode}: {stderr}");
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
        throw new DirectoryNotFoundException(
            "Could not find the repository root containing the RegExp oracle.");
    }

    private static void AssertMatchEqual(JsonElement expected, RegExpMatchArray? actual)
    {
        if (expected.ValueKind == JsonValueKind.Null)
        {
            Assert.Null(actual);
            return;
        }
        Assert.NotNull(actual);
        Assert.Equal(expected.GetProperty("value").GetString(), actual!.value);
        Assert.Equal(expected.GetProperty("index").GetDouble(), actual.index);
        Assert.Equal(expected.GetProperty("input").GetString(), actual.input);
        var groupIndex = 0;
        foreach (var expectedGroup in expected.GetProperty("groups").EnumerateArray())
        {
            Assert.Equal(
                expectedGroup.ValueKind == JsonValueKind.Null ? null : expectedGroup.GetString(),
                actual[groupIndex]);
            groupIndex += 1;
        }
        Assert.Equal(groupIndex, actual.length);
    }

    private sealed record OracleTestCase(
        string Id,
        string Pattern,
        string Flags,
        IReadOnlyList<OracleOperation> Operations);

    private sealed record OracleOperation(
        string Kind,
        string Input,
        double? LastIndex);

    private sealed record ReplacementObservation(
        string Whole,
        string Capture,
        object OptionalCapture,
        double Offset,
        string Input,
        RegExpNamedGroups Groups);
}
