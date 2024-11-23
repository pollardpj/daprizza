using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Client;
using DaprizzaInterfaces;
using DaprizzaKitchen.Actors;
using DaprizzaModels;
using DaprizzaModels.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<RegisterChefsRequest>, RegisterChefsRequestValidator>();

builder.Services.AddActors(options =>
{
    // Register actor types and configure actor settings
    options.Actors.RegisterActor<KitchenManagerActor>();

    options.ReentrancyConfig = new ActorReentrancyConfig
    {
        Enabled = true,
        MaxStackDepth = 32
    };
});

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

const string daprPubSubName = "orderstatuspubsub";
const string kitchenManagerActorId = "kitchen-manager";

app.MapPost("/cook", async (Order order, CancellationToken token) =>
{
    using var client = new DaprClientBuilder().Build();

    await Task.Delay(1000);

    var orderStatusUpdate = new OrderStatusUpdate(
        order.OrderId,
        DateTime.UtcNow,
        OrderStatus.CookingInProgress);

    await client.PublishEventAsync(daprPubSubName, "orderstatus", orderStatusUpdate, token);

    await Task.Delay(1000);

    orderStatusUpdate = new OrderStatusUpdate(
        order.OrderId,
        DateTime.UtcNow,
        OrderStatus.ReadyForDelivery);

    await client.PublishEventAsync(daprPubSubName, "orderstatus", orderStatusUpdate, token);

    return Results.Accepted();
});

app.MapMethods("/Chefs/register", ["POST", "PUT"], async (
    IValidator<RegisterChefsRequest> validator, 
    RegisterChefsRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var actorId = new ActorId(kitchenManagerActorId);
    var proxy = ActorProxy.Create<IKitchenManagerActor>(actorId, nameof(KitchenManagerActor));

    await proxy.RegisterChefs(request.Chefs);

    return Results.Ok();
});

app.MapActorsHandlers();

app.Run();
