using Dapr.Client;
using DaprizzaModels;
using DaprizzaStore.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IValidator<OrderRequest>, OrderRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

const string daprStoreName = "statestore";

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

    var order = new Order(Guid.NewGuid(), DateTime.UtcNow, null, request.Pizzas, request.Address,
        request.Pizzas.Sum(p => 10M * p.Size switch
        {
            PizzaSize.Large => 3,
            PizzaSize.Medium => 2,
            PizzaSize.Small => 1,
            _ => 10
        }),
        OrderStatus.Created);

    var stateClient = new DaprClientBuilder().Build();
    await stateClient.SaveStateAsync(daprStoreName, order.OrderId.ToString(), order, cancellationToken: token);

    var invokeClient = DaprClient.CreateInvokeHttpClient(appId: "daprizza-kitchen");
    var response = await invokeClient.PostAsJsonAsync("/cook", order, token);
    response.EnsureSuccessStatusCode();

    return Results.Ok(new OrderResponse(order.OrderId, order.TotalPrice));
});

app.MapGet("/order/{orderId:guid}", async (Guid orderId) =>
{
    var client = new DaprClientBuilder().Build();

    var order = await client.GetStateAsync<Order>(daprStoreName, orderId.ToString());

    return order == null ? Results.NotFound() : Results.Ok(order);
});

app.Run();

