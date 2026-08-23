using System;

namespace Tsonic.CSharp.Js.RegExpRuntime.Engine.QuickJs;

/// <summary>
/// Thrown when a regex pattern has a syntax error during custom engine compilation.
/// </summary>
internal sealed class RegExpSyntaxException : Exception
{
    public RegExpSyntaxException(string message) : base(message)
    {
    }
}
