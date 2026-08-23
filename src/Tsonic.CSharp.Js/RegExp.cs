using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Tsonic.CSharp.Js.RegExpRuntime.Engine;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

public class RegExp
{
    private const int MaximumPatternCodeUnits = 1_048_576;
    private readonly string _pattern;
    private readonly RegExpFlags _flags;
    private readonly EcmaRegExpProgram _program;
    private double _lastIndex;

    public RegExp()
        : this(string.Empty, (string?)null)
    {
    }

    public RegExp(string pattern, string? flags = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        _pattern = pattern;
        if (_pattern.Length > MaximumPatternCodeUnits)
        {
            throw new RangeError(
                "Regular-expression pattern exceeds the deterministic compilation budget.");
        }
        _flags = RegExpFlags.Parse(flags);
        _program = EcmaRegExpProgramCache.GetOrCompile(_pattern, _flags);
        source = EscapeSource(_pattern);
    }

    public RegExp(string pattern, Undefined flags)
        : this(pattern, (string?)null)
    {
        ArgumentNullException.ThrowIfNull(flags);
    }

    public RegExp(RegExp pattern, string? flags = null)
        : this(
            pattern?.Pattern ?? throw new ArgumentNullException(nameof(pattern)),
            flags ?? pattern.flags)
    {
    }

    public RegExp(RegExp pattern, Undefined flags)
        : this(pattern, (string?)null)
    {
        ArgumentNullException.ThrowIfNull(flags);
    }

    public RegExp(Undefined pattern)
        : this(string.Empty, (string?)null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
    }

    public RegExp(Undefined pattern, string flags)
        : this(string.Empty, flags)
    {
        ArgumentNullException.ThrowIfNull(pattern);
    }

    public RegExp(Undefined pattern, Undefined flags)
        : this(string.Empty, (string?)null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(flags);
    }

    public string source { get; }
    public string flags => _flags.CanonicalFlags;
    public bool global => _flags.Global;
    public bool hasIndices => _flags.HasIndices;
    public bool ignoreCase => _flags.IgnoreCase;
    public bool multiline => _flags.Multiline;
    public bool dotAll => _flags.DotAll;
    public bool unicode => _flags.Unicode;
    public bool unicodeSets => _flags.UnicodeSets;
    public bool sticky => _flags.Sticky;

    public double lastIndex
    {
        get => _lastIndex;
        set => _lastIndex = value;
    }

    public static RegExp create() => new();
    public static RegExp create(string pattern, string? flags = null) =>
        new(pattern, flags);
    public static RegExp create(string pattern, Undefined flags) =>
        new(pattern, flags);
    public static RegExp create(RegExp pattern) =>
        pattern ?? throw new ArgumentNullException(nameof(pattern));
    public static RegExp create(RegExp pattern, string flags) =>
        new(pattern, flags);
    public static RegExp create(RegExp pattern, Undefined flags)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(flags);
        return pattern;
    }
    public static RegExp create(Undefined pattern) => new(pattern);
    public static RegExp create(Undefined pattern, string flags) =>
        new(pattern, flags);
    public static RegExp create(Undefined pattern, Undefined flags) =>
        new(pattern, flags);

    public static string escape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length == 0) return string.Empty;
        var output = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index += 1)
        {
            var current = value[index];
            if (index == 0 && IsAsciiAlphaNumeric(current))
            {
                AppendHexByte(output, current);
                continue;
            }
            if (current is '^' or '$' or '\\' or '.' or '*' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}' or '|' or '/')
            {
                output.Append('\\').Append(current);
                continue;
            }
            if (current is ',' or '-' or '=' or '<' or '>' or '#' or '&' or '!' or '%' or ':' or ';' or '@' or '~' or '\'' or '`' or '"')
            {
                AppendHexByte(output, current);
                continue;
            }
            switch (current)
            {
                case '\f': output.Append("\\f"); continue;
                case '\n': output.Append("\\n"); continue;
                case '\r': output.Append("\\r"); continue;
                case '\t': output.Append("\\t"); continue;
                case '\v': output.Append("\\v"); continue;
                case ' ': output.Append("\\x20"); continue;
            }
            if (char.IsHighSurrogate(current) &&
                index + 1 < value.Length &&
                char.IsLowSurrogate(value[index + 1]))
            {
                output.Append(current).Append(value[index + 1]);
                index += 1;
                continue;
            }
            if (IsRegExpEscapeWhiteSpace(current) ||
                char.IsSurrogate(current))
            {
                output.Append("\\u").Append(((int)current).ToString("x4", CultureInfo.InvariantCulture));
                continue;
            }
            output.Append(current);
        }
        return output.ToString();
    }

    public virtual RegExpExecArray? exec(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var stateful = global || sticky;
        var start = stateful ? ToLength(_lastIndex) : 0;
        if (start > input.Length)
        {
            if (stateful) _lastIndex = 0;
            return null;
        }
        var match = _program.Find(input, start, sticky);
        if (match is null)
        {
            if (stateful) _lastIndex = 0;
            return null;
        }
        if (stateful) _lastIndex = match.End;
        return BuildResult(input, match);
    }

    public bool test(string input) => exec(input) is not null;

    public RegExpMatchArray? match(string input) => RegExpProtocols.Match(input, this);
    public RegExpStringIterator matchAll(string input) =>
        RegExpProtocols.MatchAll(input, this, requireGlobal: false);
    public string replace(string input, string replacement) => RegExpProtocols.Replace(input, this, replacement);
    public string replace(string input, ReplacementCallback replacer) => RegExpProtocols.Replace(input, this, replacer);
    public double search(string input) => RegExpProtocols.Search(input, this);
    public JSArray<string> split(string input, double? limit = null) => RegExpProtocols.Split(input, this, limit);

    public override string ToString() => toString();
    public string toString() => "/" + source + "/" + flags;

    internal RegExp CloneForIteration()
    {
        var clone = new RegExp(_pattern, flags) { lastIndex = lastIndex };
        return clone;
    }

    internal void AdvanceAfterEmptyMatch(string input)
    {
        var current = ToLength(_lastIndex);
        _lastIndex = AdvanceStringIndex(input, current, _flags.FullUnicode);
    }

    internal RegExpExecArray? ExecuteFrom(string input, int start, bool stickySearch)
    {
        var match = _program.Find(input, start, stickySearch);
        return match is null ? null : BuildResult(input, match);
    }

    internal string Pattern => _pattern;
    internal bool FullUnicode => _flags.FullUnicode;

    internal static int AdvanceStringIndex(
        string input,
        int index,
        bool unicode)
    {
        if (!unicode || index + 1 >= input.Length)
        {
            return checked(index + 1);
        }
        return char.IsHighSurrogate(input[index]) &&
            char.IsLowSurrogate(input[index + 1])
            ? index + 2
            : index + 1;
    }

    private RegExpExecArray BuildResult(
        string input,
        EcmaRegExpMatch match)
    {
        var values = new string?[_program.CaptureCount];
        var indexPairs = hasIndices
            ? new (double Start, double End)?[values.Length]
            : null;
        for (var index = 0; index < values.Length; index += 1)
        {
            var start = match.CaptureStarts[index];
            var end = match.CaptureEnds[index];
            if (start < 0 || end < start) continue;
            values[index] = input[start..end];
            if (indexPairs is not null)
            {
                indexPairs[index] = (start, end);
            }
        }

        Dictionary<string, string?>? namedValues = null;
        Dictionary<string, (double Start, double End)?>? namedIndices = null;
        foreach (var entry in _program.NamedCaptures)
        {
            namedValues ??= new Dictionary<string, string?>(StringComparer.Ordinal);
            var selectedValue = values[entry.Value];
            if (!namedValues.ContainsKey(entry.Key) ||
                selectedValue is not null)
            {
                namedValues[entry.Key] = selectedValue;
            }
            if (indexPairs is not null)
            {
                namedIndices ??= new Dictionary<
                    string,
                    (double Start, double End)?
                >(StringComparer.Ordinal);
                var selectedIndices = indexPairs[entry.Value];
                if (!namedIndices.ContainsKey(entry.Key) ||
                    selectedIndices is not null)
                {
                    namedIndices[entry.Key] = selectedIndices;
                }
            }
        }
        var groups = namedValues is null ? null : new RegExpNamedGroups(namedValues);
        var indices = indexPairs is null
            ? null
            : new RegExpIndicesArray(
                indexPairs,
                namedIndices is null ? null : new RegExpNamedIndices(namedIndices));
        return new RegExpExecArray(values, match.Start, input, groups, indices);
    }

    private static int ToLength(double value)
    {
        if (double.IsNaN(value) || value <= 0) return 0;
        if (double.IsPositiveInfinity(value) || value >= int.MaxValue) return int.MaxValue;
        return (int)System.Math.Floor(value);
    }

    private static string EscapeSource(string pattern)
    {
        if (pattern.Length == 0) return "(?:)";
        var builder = new StringBuilder(pattern.Length);
        for (var index = 0; index < pattern.Length; index += 1)
        {
            var current = pattern[index];
            switch (current)
            {
                case '/': builder.Append("\\/"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\u2028': builder.Append("\\u2028"); break;
                case '\u2029': builder.Append("\\u2029"); break;
                default: builder.Append(current); break;
            }
        }
        return builder.ToString();
    }

    private static bool IsAsciiAlphaNumeric(char value) => value is
        >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static void AppendHexByte(StringBuilder output, char value) =>
        output.Append("\\x").Append(((int)value).ToString("x2", CultureInfo.InvariantCulture));

    private static bool IsRegExpEscapeWhiteSpace(char value) =>
        value is >= '\u0009' and <= '\u000D' or
            '\u0020' or
            '\u00A0' or
            '\u1680' or
            >= '\u2000' and <= '\u200A' or
            '\u2028' or
            '\u2029' or
            '\u202F' or
            '\u205F' or
            '\u3000' or
            '\uFEFF';
}
