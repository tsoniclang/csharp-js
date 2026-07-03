using System;

namespace Tsonic.CSharp.Js;

public class JsRegExpException : ArgumentException
{
    public JsRegExpException(string message)
        : base(message)
    {
    }
}

public sealed class JsRegExpSyntaxException : JsRegExpException
{
    public JsRegExpSyntaxException(string message)
        : base(message)
    {
    }
}

public sealed class JsRegExpUnsupportedException : JsRegExpException
{
    public JsRegExpUnsupportedException(string message)
        : base(message)
    {
    }
}
