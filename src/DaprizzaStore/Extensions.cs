using System.Text.Json;
using System.Text.Json.Serialization;
using DaprizzaModels;

namespace DaprizzaStore;

public static class Extensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static decimal GetTotalPrice(this IEnumerable<Pizza> pizzas)
    {
        return pizzas.Sum(p => 5M * p.Size switch
        {
            PizzaSize.Large => 3,
            PizzaSize.Medium => 2,
            PizzaSize.Small => 1,
            _ => throw new InvalidOperationException($"Pizza with size = {p.Size} doesn't have a price")
        } + p.Toppings.Count());
    }

    public static string Serialize<TData>(this TData value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return JsonSerializer.Serialize(value, _options);
    }
}

