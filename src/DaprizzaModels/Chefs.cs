namespace DaprizzaModels;

public record Chef
{
    public string? Name { get; init; }
}

public record RegisterChefsRequest(IEnumerable<Chef> Chefs);