using Dapr;
using Dapr.Client;
using DaprizzaModels;
using DaprizzaModels.Validators;
using DaprizzaStore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IValidator<OrderRequest>, OrderRequestValidator>();

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.

const string daprStoreName = "statestore";
const string daprPubSubName = "orderstatuspubsub";

app.MapPost("/api/order", async (
    IValidator<OrderRequest> validator, 
    OrderRequest request,
    CancellationToken token) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var order = new Order
    {
        Id = Guid.NewGuid(),
        CreatedTimestampUtc = DateTime.UtcNow,
        Pizzas = request.Pizzas,
        Address = request.Address,
        TotalPrice = request.Pizzas.GetTotalPrice(),
        Status = OrderStatus.Created
    };

    var stateClient = new DaprClientBuilder().Build();
    await stateClient.SaveStateAsync(daprStoreName, order.Id.ToString(), order, 
        cancellationToken: token);

    var invokeClient = DaprClient.CreateInvokeHttpClient(appId: "daprizza-kitchen");
    var invokeResponse = await invokeClient.PostAsJsonAsync("/cook", order, token);
    invokeResponse.EnsureSuccessStatusCode();

    return Results.Ok(new OrderResponse
    {
        OrderId = order.Id, 
        TotalPrice = order.TotalPrice
    });
});

app.MapGet("/api/order/{orderId:guid}", async (
    Guid orderId, 
    CancellationToken token) =>
{
    var client = new DaprClientBuilder().Build();

    var order = await client.GetStateAsync<Order>(daprStoreName, orderId.ToString(), 
        cancellationToken: token);

    return order == null ? Results.NotFound() : Results.Ok(order);
});

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orderstatus", [Topic(daprPubSubName, "orderstatus")] async (
    ILogger<Program> logger,
    OrderStatusUpdate orderStatusUpdate, 
    CancellationToken token) => 
{

    var client = new DaprClientBuilder().Build();

    var order = await client.GetStateAsync<Order>(daprStoreName, orderStatusUpdate.OrderId.ToString(),
        cancellationToken: token);

    order.UpdateStatus(orderStatusUpdate);

    await client.SaveStateAsync(daprStoreName, order.Id.ToString(), order, 
        cancellationToken: token);

    if (orderStatusUpdate.Status == OrderStatus.ReadyForDelivery)
    {
        var invokeClient = DaprClient.CreateInvokeHttpClient(appId: "daprizza-delivery");
        var invokeResponse = await invokeClient.PostAsJsonAsync("/deliver", order, token);
        invokeResponse.EnsureSuccessStatusCode();
    }

    logger.LogInformation("Order Updated: {OrderUpdated}", orderStatusUpdate.Serialize());
});

app.Run();

