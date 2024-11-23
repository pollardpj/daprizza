namespace DaprizzaModels;

public class Pizza
{
    public PizzaSize Size { get; set; }
    public IEnumerable<string> Toppings { get; set; }
}

public enum PizzaSize
{
    Small = 10,
    Medium = 20,
    Large = 30
}

