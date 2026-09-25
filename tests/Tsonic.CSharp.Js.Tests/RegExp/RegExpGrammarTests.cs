using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class RegExpGrammarTests
{
    public static IEnumerable<object[]> ValidPatterns()
    {
        foreach (var (pattern, flags) in ValidPatternValues)
        {
            yield return [pattern, flags];
        }
    }

    public static IEnumerable<object[]> InvalidPatterns()
    {
        foreach (var (pattern, flags) in InvalidPatternValues)
        {
            yield return [pattern, flags];
        }
    }

    [Theory]
    [MemberData(nameof(ValidPatterns))]
    public void NormativeGrammarIsAccepted(string pattern, string flags)
    {
        _ = new RegExp(pattern, flags);
    }

    [Theory]
    [MemberData(nameof(InvalidPatterns))]
    public void InvalidGrammarThrowsSyntaxError(string pattern, string flags)
    {
        Assert.Throws<JsRegExpSyntaxException>(() => new RegExp(pattern, flags));
    }

    private static readonly (string Pattern, string Flags)[] ValidPatternValues =
    [
        ("", ""),
        ("abc", ""),
        ("a|b", ""),
        ("(?:a|b)+", ""),
        ("(?:(a)(b))+", ""),
        ("a*?", ""),
        ("a+?", ""),
        ("a??", ""),
        ("a{1,2}?", ""),
        ("(?=a)", ""),
        ("(?!a)", ""),
        ("(?<=a)b", ""),
        ("(?<!a)b", ""),
        ("(?<name>a)", ""),
        ("(?<name>a)\\k<name>", ""),
        ("(a)\\1", ""),
        ("\\1(a)", ""),
        ("\\p{Letter}+", "u"),
        ("\\P{Script=Latin}+", "u"),
        ("\\u{1F600}", "u"),
        (".", "s"),
        (".", "u"),
        ("[\\p{ASCII}&&\\p{Letter}]+", "v"),
        ("[[a-z]--[aeiou]]+", "v"),
        ("[\\q{ab|cd}]", "v"),
        ("(?i:a)", ""),
        ("(?im-s:^a.$)", ""),
        ("(?<x>a)|(?<x>b)", ""),
        ("[^]", ""),
        ("[]", ""),
        ("\\8", ""),
        ("{", ""),
        ("}", ""),
        ("a{", ""),
        ("\\cA", ""),
        ("[\\cA]", ""),
        ("\\0", ""),
        ("\\01", ""),
        ("[a-]", ""),
        ("[-a]", ""),
        ("a", "d"),
        ("a", "g"),
        ("a", "i"),
        ("a", "m"),
        ("a", "s"),
        ("a", "u"),
        ("a", "v"),
        ("a", "y"),
        ("a", "dgimsy"),
        ("a", "dgimvy"),
    ];

    private static readonly (string Pattern, string Flags)[] InvalidPatternValues =
    [
        ("+a", ""),
        ("^*", ""),
        ("a{2,1}", ""),
        ("[z-a]", ""),
        ("[", ""),
        ("(", ""),
        ("\\", ""),
        ("(?<a>a)(?<a>b)", ""),
        ("(?<1>a)", ""),
        ("(?q:a)", ""),
        ("\\p{NoSuchProperty}", "u"),
        ("\\u{110000}", "u"),
        ("[a--b]", "u"),
        ("[\\q{ab}]", "u"),
        ("a", "uv"),
        ("a", "gg"),
        ("a", "q"),
    ];
}
