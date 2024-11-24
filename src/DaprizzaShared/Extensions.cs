using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaprizzaShared;

public static class Extensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize<TData>(this TData value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return JsonSerializer.Serialize(value, _options);
    }
}

