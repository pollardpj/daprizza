namespace DaprizzaModels;

public record Pizza(PizzaSize Size, IEnumerable<string> Toppings);

public enum PizzaSize
{
    Small = 10,
    Medium = 20,
    Large = 30
}

