using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraDotnet;

public class TerraJsonSerializerOptions
{
    public static JsonSerializerOptions GetThem()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = {new JsonNumberToStringConverter()},
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };
    }
}