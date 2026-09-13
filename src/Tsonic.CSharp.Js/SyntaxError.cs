namespace Tsonic.CSharp.Js;

public sealed class SyntaxError : Error
{
    public SyntaxError(string message) : base(message) { }

    public override string name { get; set; } = nameof(SyntaxError);
}
