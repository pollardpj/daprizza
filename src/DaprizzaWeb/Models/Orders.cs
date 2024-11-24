namespace DaprizzaWeb.Models;

public class Order
{
    public Pizza[] Pizzas { get; set; } = [];
    public Address Address { get; init; } = new();
}

public class Address
{
    public string HouseNumberOrName { get; set; }
    public string Postcode { get; set; }
}
