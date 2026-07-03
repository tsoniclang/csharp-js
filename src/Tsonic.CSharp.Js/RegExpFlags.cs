using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

internal readonly record struct RegExpFlags(
    bool Global,
    bool IgnoreCase,
    bool Multiline,
    bool DotAll,
    bool Sticky)
{
    public string CanonicalFlags
    {
        get
        {
            Span<char> buffer = stackalloc char[5];
            var length = 0;
            if (Global)
            {
                buffer[length++] = 'g';
            }
            if (IgnoreCase)
            {
                buffer[length++] = 'i';
            }
            if (Multiline)
            {
                buffer[length++] = 'm';
            }
            if (DotAll)
            {
                buffer[length++] = 's';
            }
            if (Sticky)
            {
                buffer[length++] = 'y';
            }
            return new string(buffer[..length]);
        }
    }

    public static RegExpFlags Parse(string? flags)
    {
        var seen = new HashSet<char>();
        var global = false;
        var ignoreCase = false;
        var multiline = false;
        var dotAll = false;
        var sticky = false;

        foreach (var flag in flags ?? string.Empty)
        {
            if (!seen.Add(flag))
            {
                throw new JsRegExpSyntaxException($"Duplicate RegExp flag '{flag}'.");
            }

            switch (flag)
            {
                case 'g':
                    global = true;
                    break;
                case 'i':
                    ignoreCase = true;
                    break;
                case 'm':
                    multiline = true;
                    break;
                case 's':
                    dotAll = true;
                    break;
                case 'y':
                    sticky = true;
                    break;
                case 'd':
                    throw new JsRegExpUnsupportedException("RegExp flag 'd' requires hasIndices match result semantics.");
                case 'u':
                    throw new JsRegExpUnsupportedException("RegExp flag 'u' requires ECMAScript Unicode-mode pattern semantics.");
                case 'v':
                    throw new JsRegExpUnsupportedException("RegExp flag 'v' requires ECMAScript Unicode-sets semantics.");
                default:
                    throw new JsRegExpSyntaxException($"Invalid RegExp flag '{flag}'.");
            }
        }

        return new RegExpFlags(global, ignoreCase, multiline, dotAll, sticky);
    }
}
