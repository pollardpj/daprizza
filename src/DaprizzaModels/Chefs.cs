namespace DaprizzaModels;

public class Chef
{
    public string Name { get; init; }
}

public class RegisterChefsRequest
{
    public IEnumerable<Chef> Chefs { get; init; }
}