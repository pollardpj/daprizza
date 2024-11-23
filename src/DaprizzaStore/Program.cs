using System.Text.Json;
using Dapr;
using Dapr.Client;
using DaprizzaModels;
using DaprizzaModels.Validators;
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

app.MapPost("/order", async (
    IValidator<OrderRequest> validator, 
    OrderRequest request,
    CancellationToken token) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var order = new Order(
        Guid.NewGuid(), 
        DateTime.UtcNow, 
        null, 
        request.Pizzas, 
        request.Address,
        request.Pizzas.Sum(p => 5M * p.Toppings.Count() * p.Size switch
        {
            PizzaSize.Large => 3,
            PizzaSize.Medium => 2,
            PizzaSize.Small => 1,
            _ => throw new InvalidOperationException($"Pizza with size = {p.Size} doesn't have a price")
        }),
        OrderStatus.Created);

    var stateClient = new DaprClientBuilder().Build();
    await stateClient.SaveStateAsync(daprStoreName, order.Id.ToString(), order, 
        cancellationToken: token);

    var invokeClient = DaprClient.CreateInvokeHttpClient(appId: "daprizza-kitchen");
    await invokeClient.PostAsJsonAsync("/cook", order, token);
    
    return Results.Ok(new OrderResponse(order.Id, order.TotalPrice));
});

app.MapGet("/order/{orderId:guid}", async (
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

    logger.LogInformation("Order Updated: {OrderUpdated}", JsonSerializer.Serialize(orderStatusUpdate));
});

app.Run();

