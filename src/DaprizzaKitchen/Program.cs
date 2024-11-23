using Dapr.Client;
using DaprizzaModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

const string daprPubSubName = "orderstatuspubsub";

app.MapPost("/cook", async (Order order) =>
{
    using var client = new DaprClientBuilder().Build();

    await Task.Delay(1000);

    var orderStatusUpdate = new OrderStatusUpdate(
        order.OrderId,
        DateTime.UtcNow,
        OrderStatus.CookingInProgress);

    await client.PublishEventAsync(daprPubSubName, "orderstatus", orderStatusUpdate);

    await Task.Delay(1000);

    orderStatusUpdate = new OrderStatusUpdate(
        order.OrderId,
        DateTime.UtcNow,
        OrderStatus.ReadyForDelivery);

    await client.PublishEventAsync(daprPubSubName, "orderstatus", orderStatusUpdate);

    return Results.Accepted();
});

app.Run();
