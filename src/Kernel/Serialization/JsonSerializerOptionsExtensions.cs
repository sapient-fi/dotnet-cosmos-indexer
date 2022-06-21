using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapientFi.Kernel.Serialization;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddConverter(this JsonSerializerOptions options, JsonConverter converter)
    {
        options.Converters.Add(converter);
        return options;
    }
}