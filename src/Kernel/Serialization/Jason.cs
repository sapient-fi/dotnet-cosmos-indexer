using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapientFi.Kernel.Serialization;

/// <summary>
/// A JSON (de)serializer, which is named like this to avoid
/// clashing with all of the other JsonSerializer classes in the world.
/// </summary>
public class Jason
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        IncludeFields = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
    
    public virtual string ToJson(object thing)
    {
        return JsonSerializer.Serialize(thing, _options);
    }

    public virtual T? FromJson<T>(string encoded)
    {
        return JsonSerializer.Deserialize<T>(encoded, _options);
    }
}