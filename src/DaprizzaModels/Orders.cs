namespace DaprizzaModels;

public record Order(
    Guid OrderId, 
    DateTime CreatedTimestampUtc, 
    List<Pizza> Pizzas, 
    Address Address, 
    decimal TotalPrice,
    OrderStatus Status = OrderStatus.Created)
    : OrderRequest(Pizzas, Address);

public record OrderResponse(Guid OrderId, decimal TotalPrice);

public record OrderRequest(List<Pizza> Pizzas, Address Address);

public record Address(string HouseNumberOrName, string Postcode);

public enum OrderStatus
{
    Created = 10,
    PassedToKitchen = 20,
    CookingInProgress = 40,
    ReadyForDelivery = 50,
    InTransit = 60,
    Delivered = 70,
    InError = 999
}
