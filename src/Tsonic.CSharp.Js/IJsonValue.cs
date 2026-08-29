using System.Text.Json;

namespace Tsonic.CSharp.Js
{
    public interface IJsonValue
    {
        object? __tsonicJsonValue(string key);

        void __tsonicWriteJson(Utf8JsonWriter writer, JsonWriteContext context, string key);
    }
}
