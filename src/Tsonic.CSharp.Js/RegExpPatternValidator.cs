using System;
using System.Text;

namespace Tsonic.CSharp.Js;

internal static class RegExpPatternValidator
{
    public static string TransformSupportedSubset(string pattern, bool dotAll)
    {
        var builder = new StringBuilder(pattern.Length);
        for (var index = 0; index < pattern.Length; index += 1)
        {
            var current = pattern[index];
            if (current == '\\')
            {
                var next = ValidateEscape(pattern, index);
                builder.Append(pattern, index, next - index + 1);
                index = next;
                continue;
            }

            if (current == '[')
            {
                var next = ValidateCharacterClass(pattern, index);
                builder.Append(pattern, index, next - index + 1);
                index = next;
                continue;
            }

            if (current == '(')
            {
                ValidateGroupPrefix(pattern, index);
            }

            if (current == '.' && dotAll)
            {
                builder.Append("[\\s\\S]");
            }
            else
            {
                builder.Append(current);
            }
        }
        return builder.ToString();
    }

    private static int ValidateEscape(string pattern, int slashIndex)
    {
        if (slashIndex + 1 >= pattern.Length)
        {
            throw new JsRegExpSyntaxException("RegExp pattern ends with an incomplete escape.");
        }

        var escaped = pattern[slashIndex + 1];
        switch (escaped)
        {
            case 'p':
            case 'P':
                if (slashIndex + 2 < pattern.Length && pattern[slashIndex + 2] == '{')
                {
                    throw new JsRegExpUnsupportedException("Unicode property escapes require ECMAScript Unicode semantics.");
                }
                return slashIndex + 1;
            case 'k':
                if (slashIndex + 2 < pattern.Length && pattern[slashIndex + 2] == '<')
                {
                    throw new JsRegExpUnsupportedException("Named backreferences are not in the proven RegExp subset.");
                }
                return slashIndex + 1;
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                throw new JsRegExpUnsupportedException("Numeric backreferences and legacy numeric escapes are not in the proven RegExp subset.");
            default:
                return slashIndex + 1;
        }
    }

    private static int ValidateCharacterClass(string pattern, int classStart)
    {
        var escaped = false;
        for (var index = classStart + 1; index < pattern.Length; index += 1)
        {
            var current = pattern[index];
            if (escaped)
            {
                if ((current == 'p' || current == 'P') && index + 1 < pattern.Length && pattern[index + 1] == '{')
                {
                    throw new JsRegExpUnsupportedException("Unicode property escapes inside character classes require ECMAScript Unicode semantics.");
                }
                escaped = false;
                continue;
            }
            if (current == '\\')
            {
                escaped = true;
                continue;
            }
            if (current == ']')
            {
                return index;
            }
        }
        throw new JsRegExpSyntaxException("Unterminated RegExp character class.");
    }

    private static void ValidateGroupPrefix(string pattern, int groupStart)
    {
        if (groupStart + 1 >= pattern.Length || pattern[groupStart + 1] != '?')
        {
            return;
        }

        if (groupStart + 2 >= pattern.Length)
        {
            throw new JsRegExpSyntaxException("Incomplete RegExp group prefix.");
        }

        var marker = pattern[groupStart + 2];
        switch (marker)
        {
            case ':':
            case '=':
            case '!':
                return;
            case '<':
                if (groupStart + 3 < pattern.Length && (pattern[groupStart + 3] == '=' || pattern[groupStart + 3] == '!'))
                {
                    throw new JsRegExpUnsupportedException("Lookbehind assertions are not in the proven RegExp subset.");
                }
                throw new JsRegExpUnsupportedException("Named capture groups are not in the proven RegExp subset.");
            case '>':
                throw new JsRegExpUnsupportedException(".NET atomic groups are not ECMAScript RegExp syntax.");
            case '#':
                throw new JsRegExpUnsupportedException(".NET comment groups are not ECMAScript RegExp syntax.");
            case '(':
                throw new JsRegExpUnsupportedException(".NET conditional groups are not ECMAScript RegExp syntax.");
            default:
                if (char.IsLetter(marker) || marker == '-')
                {
                    throw new JsRegExpUnsupportedException(".NET inline option groups are not ECMAScript RegExp syntax.");
                }
                throw new JsRegExpSyntaxException($"Unsupported RegExp group prefix '(?{marker}'.");
        }
    }
}
