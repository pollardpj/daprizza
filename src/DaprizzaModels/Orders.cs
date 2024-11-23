namespace DaprizzaModels;

public class OrderStatusUpdate
{
    public Guid OrderId { get; set; }
    public DateTime UpdatedTimestampUtc { get; set; }
    public OrderStatus Status { get; set; }
    public IEnumerable<string> Errors { get; set; }
}

public class Order : OrderRequest
{
    public Guid Id { get; set; }
    public DateTime CreatedTimestampUtc { get; set; }
    public DateTime? UpdatedTimestampUtc { get; set; }
    public decimal TotalPrice { get; set; }
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
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderRequest
{
    public IEnumerable<Pizza> Pizzas { get; set; }
    public Address Address {get; set; }
}

public class Address
{
    public string HouseNumberOrName { get; set; }
    public string Postcode { get; set; }
}

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
