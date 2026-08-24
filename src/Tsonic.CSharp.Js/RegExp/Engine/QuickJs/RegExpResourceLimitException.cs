using System;

namespace Tsonic.CSharp.Js.RegExpRuntime.Engine.QuickJs;

internal sealed class RegExpResourceLimitException : Exception
{
    public RegExpResourceLimitException(string message)
        : base(message)
    {
    }
}
