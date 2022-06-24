using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TerraDotnet.Extensions;

public static class SerializerExtensions
{
    private static readonly JsonSerializerOptions Options  = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonNumberToStringConverter()
        }
    };

    
    public static T? ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        if (json.IsBase64String())
        {
            json = Encoding.UTF8.GetString(Convert.FromBase64String(json));
        }
            
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T? ToObject<T>(this JsonDocument document)
    {
        var json = document.RootElement.GetRawText();
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T? ToObject<T>(this Dictionary<string, JsonElement>? dict, string key)
    {
        if (dict == null)
        {
            return default;
        }

        if (!dict.TryGetValue(key, out var val))
        {
            return default;
        }

        return val.ToObject<T>();
    }

    public static T? ToObject<T>(this string json)
    {
        if (json.IsBase64String())
        {
            return ToObjectFromBase64<T>(json);
        }
        
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T? ToObjectFromBase64<T>(this string base64StringMessage)
    {
        return JsonSerializer.Deserialize<T>(Convert.FromBase64String(base64StringMessage), Options);
    }
        
    public static bool IsBase64String(this string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

    }
}