namespace DaprizzaModels;

public record OrderStatusUpdate(
    Guid OrderId,
    DateTime UpdatedTimestampUtc,
    OrderStatus Status,
    IEnumerable<string>? Errors = null);

public record Order(
    Guid OrderId,
    DateTime CreatedTimestampUtc,
    DateTime? UpdatedTimestampUtc,
    IEnumerable<Pizza> Pizzas,
    Address Address,
    decimal TotalPrice,
    OrderStatus Status,
    IEnumerable<string>? Errors = null)
    : OrderRequest(Pizzas, Address)
{
    public OrderStatus Status { get; private set; } = Status;
    public DateTime? UpdatedTimestampUtc { get; private set; } = UpdatedTimestampUtc;
    public IEnumerable<string>? Errors { get; private set; } = Errors;

    public void UpdateStatus(OrderStatusUpdate orderStatusUpdate)
    {
        Status = orderStatusUpdate.Status;
        Errors = orderStatusUpdate.Errors;
        UpdatedTimestampUtc = orderStatusUpdate.UpdatedTimestampUtc;
    }
}

public record OrderResponse(
    Guid OrderId, 
    decimal TotalPrice);

public record OrderRequest(
    IEnumerable<Pizza> Pizzas, 
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
