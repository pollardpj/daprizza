﻿namespace DaprizzaModels;

public record OrderStatusUpdate(
    Guid OrderId,
    DateTime UpdatedTimestampUtc,
    OrderStatus Status,
    List<string>? Errors = null);

public record Order(
    Guid OrderId,
    DateTime CreatedTimestampUtc,
    DateTime? UpdatedTimestampUtc,
    List<Pizza> Pizzas,
    Address Address,
    decimal TotalPrice,
    OrderStatus Status,
    List<string>? Errors = null)
    : OrderRequest(Pizzas, Address)
{
    public OrderStatus Status { get; private set; }
    public DateTime? UpdatedTimestampUtc { get; private set; }
    public List<string>? Errors { get; private set; }

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
