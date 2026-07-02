using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Tsonic.CSharp.Js;

public sealed class RegExp
{
    private readonly Regex _regex;
    private readonly string _source;
    private readonly RegExpFlags _flags;
    private int _lastIndex;

    public RegExp(string pattern)
        : this(pattern, string.Empty)
    {
    }

    public RegExp(string pattern, string? flags)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        _flags = RegExpFlags.Parse(flags);
        var runtimePattern = RegExpPatternValidator.TransformSupportedSubset(pattern, _flags.DotAll);
        _source = EscapeSource(pattern);
        _lastIndex = 0;

        try
        {
            _regex = new Regex(runtimePattern, RegexOptionsFor(_flags));
        }
        catch (ArgumentException exception)
        {
            throw new JsRegExpSyntaxException($"Invalid ECMAScript RegExp pattern for supported subset: {exception.Message}");
        }
    }

    public string source => _source;

    public string flags => _flags.CanonicalFlags;

    public bool global => _flags.Global;

    public bool hasIndices => false;

    public bool ignoreCase => _flags.IgnoreCase;

    public bool multiline => _flags.Multiline;

    public bool dotAll => _flags.DotAll;

    public bool unicode => false;

    public bool unicodeSets => false;

    public bool sticky => _flags.Sticky;

    public int lastIndex
    {
        get => _lastIndex;
        set => _lastIndex = value < 0 ? 0 : value;
    }

    public RegExpMatchResult? exec(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var startIndex = global || sticky ? _lastIndex : 0;
        if (startIndex < 0 || startIndex > input.Length)
        {
            ResetStatefulLastIndex();
            return null;
        }

        var match = _regex.Match(input, startIndex);
        if (sticky && match.Success && match.Index != startIndex)
        {
            ResetStatefulLastIndex();
            return null;
        }

        if (!match.Success)
        {
            ResetStatefulLastIndex();
            return null;
        }

        if (global || sticky)
        {
            _lastIndex = match.Index + match.Length;
        }

        var groups = new string?[match.Groups.Count];
        for (var index = 0; index < match.Groups.Count; index += 1)
        {
            groups[index] = match.Groups[index].Success ? match.Groups[index].Value : null;
        }

        return new RegExpMatchResult(match.Value, match.Index, input, groups);
    }

    public bool test(string input) => exec(input) is not null;

    public override string ToString() => toString();

    public string toString() => "/" + source + "/" + flags;

    internal Regex GetInternalRegexForSupportedStringIntegration() => _regex;

    private void ResetStatefulLastIndex()
    {
        if (global || sticky)
        {
            _lastIndex = 0;
        }
    }

    private static RegexOptions RegexOptionsFor(RegExpFlags flags)
    {
        var options = RegexOptions.CultureInvariant | RegexOptions.ECMAScript;
        if (flags.IgnoreCase)
        {
            options |= RegexOptions.IgnoreCase;
        }
        if (flags.Multiline)
        {
            options |= RegexOptions.Multiline;
        }
        return options;
    }

    private static string EscapeSource(string pattern)
    {
        if (pattern.Length == 0)
        {
            return "(?:)";
        }

        var builder = new StringBuilder(pattern.Length);
        for (var index = 0; index < pattern.Length; index += 1)
        {
            var current = pattern[index];
            if (current == '\\' && index + 1 < pattern.Length && pattern[index + 1] == '/')
            {
                builder.Append("\\/");
                index += 1;
                continue;
            }

            switch (current)
            {
                case '/':
                    builder.Append("\\/");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\u2028':
                    builder.Append("\\u2028");
                    break;
                case '\u2029':
                    builder.Append("\\u2029");
                    break;
                default:
                    builder.Append(current);
                    break;
            }
        }
        return builder.ToString();
    }
}
