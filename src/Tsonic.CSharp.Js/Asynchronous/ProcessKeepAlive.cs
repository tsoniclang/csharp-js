namespace Tsonic.CSharp.Js;

public static class ProcessKeepAlive
{
    public static bool HasReferences => JsEventLoop.HasReferences;
    public static void Acquire() => JsEventLoop.Capture().Acquire();
    public static void Release() => JsEventLoop.Capture().Release();
}
