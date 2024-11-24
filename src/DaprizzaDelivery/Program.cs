using DaprizzaDelivery.Actors;
using DaprizzaModels.Validators;
using DaprizzaModels;
using FluentValidation;
using Dapr.Actors.Client;
using Dapr.Actors;
using DaprizzaInterfaces;
using System.Text.Json;
using DaprizzaDelivery;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<RegisterDriversRequest>, RegisterDriversRequestValidator>();

builder.Services.AddActors(options =>
{
    // Register actor types and configure actor settings
    options.Actors.RegisterActor<DeliveryManagerActor>();
    options.Actors.RegisterActor<DriverActor>();
});

// Add services to the container.

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// Configure the HTTP request pipeline.

app.MapPost("/deliver", async (
    ILogger<Program> logger,
    Order order,
    CancellationToken token) =>
{
    var actorId = new ActorId(Constants.DeliveryManagerActorId);
    var proxy = ActorProxy.Create<IDeliveryManagerActor>(actorId, nameof(DeliveryManagerActor));

    await proxy.EnqueueOrder(order);

    logger.LogInformation("New Order dispatched to be delivered: {Order}", JsonSerializer.Serialize(order));

    return Results.Accepted();
});

app.MapMethods("/api/drivers/register", ["POST", "PUT"], async (
    ILogger<Program> logger,
    IValidator<RegisterDriversRequest> validator,
    RegisterDriversRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var actorId = new ActorId(Constants.DeliveryManagerActorId);
    var proxy = ActorProxy.Create<IDeliveryManagerActor>(actorId, nameof(DeliveryManagerActor));

    await proxy.RegisterDrivers(request.Drivers);

    logger.LogInformation("Drivers registered and started work: {Drivers}", JsonSerializer.Serialize(request.Drivers));

    return Results.Ok();
});

app.MapActorsHandlers();

app.Run();

