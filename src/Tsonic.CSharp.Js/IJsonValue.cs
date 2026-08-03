using System.Text.Json;

namespace Tsonic.CSharp.Js
{
    public interface IJsonValue
    {
        void __tsonicWriteJson(Utf8JsonWriter writer, JsonWriteContext context);
    }
}
