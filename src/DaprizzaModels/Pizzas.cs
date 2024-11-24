namespace DaprizzaModels;

public class Pizza
{
    public PizzaSize Size { get; init; }
    public IEnumerable<string> Toppings { get; init; }
}

public enum PizzaSize
{
    Small = 10,
    Medium = 20,
    Large = 30
}

