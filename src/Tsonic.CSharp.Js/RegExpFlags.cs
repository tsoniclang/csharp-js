using System;

namespace Tsonic.CSharp.Js;

internal readonly record struct RegExpFlags(
    bool HasIndices,
    bool Global,
    bool IgnoreCase,
    bool Multiline,
    bool DotAll,
    bool Unicode,
    bool UnicodeSets,
    bool Sticky)
{
    public bool FullUnicode => Unicode || UnicodeSets;

    public string CanonicalFlags
    {
        get
        {
            Span<char> buffer = stackalloc char[8];
            var length = 0;
            if (HasIndices) buffer[length++] = 'd';
            if (Global) buffer[length++] = 'g';
            if (IgnoreCase) buffer[length++] = 'i';
            if (Multiline) buffer[length++] = 'm';
            if (DotAll) buffer[length++] = 's';
            if (Unicode) buffer[length++] = 'u';
            if (UnicodeSets) buffer[length++] = 'v';
            if (Sticky) buffer[length++] = 'y';
            return new string(buffer[..length]);
        }
    }

    public static RegExpFlags Parse(string? flags)
    {
        Span<bool> seen = stackalloc bool[128];
        var hasIndices = false;
        var global = false;
        var ignoreCase = false;
        var multiline = false;
        var dotAll = false;
        var unicode = false;
        var unicodeSets = false;
        var sticky = false;

        foreach (var flag in flags ?? string.Empty)
        {
            if (flag >= seen.Length || seen[flag])
            {
                throw new JsRegExpSyntaxException($"Invalid or duplicate RegExp flag '{flag}'.");
            }
            seen[flag] = true;
            switch (flag)
            {
                case 'd': hasIndices = true; break;
                case 'g': global = true; break;
                case 'i': ignoreCase = true; break;
                case 'm': multiline = true; break;
                case 's': dotAll = true; break;
                case 'u': unicode = true; break;
                case 'v': unicodeSets = true; break;
                case 'y': sticky = true; break;
                default: throw new JsRegExpSyntaxException($"Invalid RegExp flag '{flag}'.");
            }
        }

        if (unicode && unicodeSets)
        {
            throw new JsRegExpSyntaxException("RegExp flags 'u' and 'v' cannot be used together.");
        }

        return new RegExpFlags(
            hasIndices,
            global,
            ignoreCase,
            multiline,
            dotAll,
            unicode,
            unicodeSets,
            sticky);
    }
}
