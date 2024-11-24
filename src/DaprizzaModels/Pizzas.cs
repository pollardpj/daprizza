using DaprizzaShared;

namespace DaprizzaModels;

public class Pizza
{
    public PizzaSize Size { get; init; }
    public IEnumerable<string> Toppings { get; init; }
}

