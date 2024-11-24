namespace DaprizzaWeb.Models;

public class Order
{
    public string SomeValue { get; set; }
    public IEnumerable<Pizza> Pizzas { get; init; } = new List<Pizza>();
    public Address Address { get; init; } = new();
}

public class Address
{
    public string HouseNumberOrName { get; set; }
    public string Postcode { get; set; }
}
