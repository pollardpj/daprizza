namespace DaprizzaModels;

public class OrderStatusUpdate
{
    public Guid OrderId { get; init; }
    public DateTime UpdatedTimestampUtc { get; init; }
    public OrderStatus Status { get; init; }
    public string UpdatedByActorName { get; set; }
    public IEnumerable<string> Errors { get; init; }
}

public class Order : OrderRequest
{
    public Guid Id { get; init; }
    public DateTime CreatedTimestampUtc { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime? UpdatedTimestampUtc { get; set; }
    public OrderStatus Status { get; set; }
    public IEnumerable<string> Errors { get; set; }

    public void UpdateStatus(OrderStatusUpdate orderStatusUpdate)
    {
        Status = orderStatusUpdate.Status;
        Errors = orderStatusUpdate.Errors;
        UpdatedTimestampUtc = orderStatusUpdate.UpdatedTimestampUtc;
    }
}

public class OrderResponse
{
    public Guid OrderId { get; init; }
    public decimal TotalPrice { get; init; }
}

public class OrderRequest
{
    public IEnumerable<Pizza> Pizzas { get; init; }
    public Address Address {get; init; }
}

public class Address
{
    public string HouseNumberOrName { get; init; }
    public string Postcode { get; init; }
}

public enum OrderStatus
{
    Created = 10,
    CookingInProgress = 20,
    ReadyForDelivery = 30,
    InTransit = 40,
    Delivered = 50,
    InError = 999
}
