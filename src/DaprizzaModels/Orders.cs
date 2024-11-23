namespace DaprizzaModels;

public record Order(
    Guid OrderId, 
    DateTime CreatedTimestampUtc, 
    List<Pizza> Pizzas, 
    Address Address, 
    decimal TotalPrice,
    OrderStatus Status,
    List<string>? Errors = null)
    : OrderRequest(Pizzas, Address);

public record OrderResponse(
    Guid OrderId, 
    decimal TotalPrice);

public record OrderRequest(
    List<Pizza> Pizzas, 
    Address Address);

public record Address(
    string HouseNumberOrName, 
    string Postcode);

public enum OrderStatus
{
    Created = 10,
    PassedToKitchen = 20,
    CookingInProgress = 30,
    ReadyForDelivery = 40,
    InTransit = 50,
    Delivered = 60,
    InError = 999
}
