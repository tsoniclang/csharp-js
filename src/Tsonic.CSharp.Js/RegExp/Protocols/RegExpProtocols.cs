using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tsonic.CSharp.Js;

internal static class RegExpProtocols
{
    public static RegExpMatchArray? Match(string input, RegExp expression)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expression);
        if (!expression.global) return expression.exec(input);
        expression.lastIndex = 0;
        var values = new List<string?>();
        while (true)
        {
            var result = expression.exec(input);
            if (result is null) break;
            values.Add(result.value);
            if (result.value.Length == 0) expression.AdvanceAfterEmptyMatch(input);
        }
        return values.Count == 0
            ? null
            : new RegExpMatchArray(values.ToArray(), null, null, null, null);
    }

    public static RegExpStringIterator MatchAll(
        string input,
        RegExp expression,
        bool requireGlobal)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expression);
        if (requireGlobal && !expression.global)
        {
            throw new TypeError("String.prototype.matchAll requires a global RegExp.");
        }
        return new RegExpStringIterator(expression, input);
    }

    public static string Replace(string input, RegExp expression, string replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        return ReplaceCore(input, expression, match => GetSubstitution(input, match, replacement));
    }

    public static string Replace(string input, RegExp expression, ReplacementCallback replacer)
    {
        ArgumentNullException.ThrowIfNull(replacer);
        return ReplaceCore(input, expression, match =>
        {
            return replacer(
                ReplacementCallbackArguments.FromRegExpMatch(match, input));
        });
    }

    public static double Search(string input, RegExp expression)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expression);
        var previous = expression.lastIndex;
        expression.lastIndex = 0;
        try
        {
            return expression.exec(input)?.index ?? -1;
        }
        finally
        {
            expression.lastIndex = previous;
        }
    }

    public static JSArray<string> Split(string input, RegExp expression, double? limit)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expression);
        var maximum = ToUint32(limit ?? uint.MaxValue);
        var output = new JSArray<string>();
        if (maximum == 0) return output;
        if (input.Length == 0)
        {
            if (expression.ExecuteFrom(input, 0, true) is null) output.push(string.Empty);
            return output;
        }

        var segmentStart = 0;
        var cursor = 0;
        while (cursor < input.Length)
        {
            var match = expression.ExecuteFrom(input, cursor, true);
            if (match is null)
            {
                cursor = RegExp.AdvanceStringIndex(
                    input,
                    cursor,
                    expression.FullUnicode);
                continue;
            }
            var end = (int)match.index + match.value.Length;
            if (end == segmentStart)
            {
                cursor = RegExp.AdvanceStringIndex(
                    input,
                    cursor,
                    expression.FullUnicode);
                continue;
            }
            output.push(input[segmentStart..cursor]);
            if ((uint)output.length >= maximum) return output;
            for (var index = 1; index < match.length; index += 1)
            {
                output.push(match[index]!);
                if ((uint)output.length >= maximum) return output;
            }
            segmentStart = end;
            cursor = end;
        }
        output.push(input[segmentStart..]);
        return output;
    }

    private static string ReplaceCore(
        string input,
        RegExp expression,
        Func<RegExpExecArray, string> replacement)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expression);
        var matches = new List<RegExpExecArray>();
        if (expression.global) expression.lastIndex = 0;
        while (true)
        {
            var match = expression.exec(input);
            if (match is null) break;
            matches.Add(match);
            if (!expression.global) break;
            if (match.value.Length == 0) expression.AdvanceAfterEmptyMatch(input);
        }
        if (matches.Count == 0) return input;

        var output = new StringBuilder(input.Length);
        var nextSourcePosition = 0;
        foreach (var match in matches)
        {
            var position = System.Math.Clamp((int)match.index, 0, input.Length);
            if (position < nextSourcePosition) continue;
            output.Append(input, nextSourcePosition, position - nextSourcePosition);
            output.Append(replacement(match));
            nextSourcePosition = position + match.value.Length;
        }
        output.Append(input, nextSourcePosition, input.Length - nextSourcePosition);
        return output.ToString();
    }

    private static string GetSubstitution(string input, RegExpExecArray match, string replacement)
    {
        var output = new StringBuilder(replacement.Length);
        var position = (int)match.index;
        for (var index = 0; index < replacement.Length; index += 1)
        {
            var current = replacement[index];
            if (current != '$' || index + 1 >= replacement.Length)
            {
                output.Append(current);
                continue;
            }
            var next = replacement[index + 1];
            switch (next)
            {
                case '$': output.Append('$'); index += 1; continue;
                case '&': output.Append(match.value); index += 1; continue;
                case '`': output.Append(input, 0, position); index += 1; continue;
                case '\'':
                    var suffix = position + match.value.Length;
                    output.Append(input, suffix, input.Length - suffix);
                    index += 1;
                    continue;
                case '<' when match.groups is not null:
                    var close = replacement.IndexOf('>', index + 2);
                    if (close < 0)
                    {
                        output.Append('$');
                        continue;
                    }
                    var name = replacement[(index + 2)..close];
                    output.Append(match.groups[name] ?? string.Empty);
                    index = close;
                    continue;
            }
            if (next is >= '1' and <= '9')
            {
                var group = next - '0';
                var consumed = 1;
                if (index + 2 < replacement.Length && replacement[index + 2] is >= '0' and <= '9')
                {
                    var twoDigit = group * 10 + replacement[index + 2] - '0';
                    if (twoDigit < match.length)
                    {
                        group = twoDigit;
                        consumed = 2;
                    }
                }
                if (group < match.length)
                {
                    output.Append(match[group] ?? string.Empty);
                    index += consumed;
                    continue;
                }
            }
            output.Append('$');
        }
        return output.ToString();
    }

    private static uint ToUint32(double value)
    {
        if (!double.IsFinite(value) || value == 0) return 0;
        var integer = System.Math.Truncate(value);
        var modulo = integer % 4_294_967_296d;
        if (modulo < 0) modulo += 4_294_967_296d;
        return (uint)modulo;
    }
}
