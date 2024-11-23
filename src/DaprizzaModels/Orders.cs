namespace DaprizzaModels
{
    public record Order(Guid OrderId, DateTime CreatedTimestampUtc, List<Pizza> Pizzas, Address Address, decimal TotalPrice) 
        : OrderRequest(Pizzas, Address);

    public record OrderResponse(Guid OrderId, decimal TotalPrice);

    public record OrderRequest(List<Pizza> Pizzas, Address Address);

    public record Address(string HouseNumberOrName, string Postcode);
}
