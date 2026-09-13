namespace Tsonic.CSharp.Js;

public sealed class EmptyObject
{
    private bool _frozen;

    public static EmptyObject Freeze(EmptyObject value)
    {
        value._frozen = true;
        return value;
    }

    public static bool IsFrozen(EmptyObject value) => value._frozen;
}
