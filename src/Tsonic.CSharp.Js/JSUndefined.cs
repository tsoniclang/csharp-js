namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Closed singleton carrier for JavaScript undefined when broad runtime values need to distinguish it from null.
    /// </summary>
    public sealed class JSUndefined
    {
        public static readonly JSUndefined value = new();

        private JSUndefined()
        {
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}
