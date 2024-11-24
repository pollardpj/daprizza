using Dapr.Actors;
using Dapr.Actors.Client;
using DaprizzaDelivery;
using DaprizzaDelivery.Actors;
using DaprizzaInterfaces;
using DaprizzaModels;
using DaprizzaModels.Validators;
using DaprizzaShared;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<RegisterDriversRequest>, RegisterDriversRequestValidator>();

builder.Services.AddActors(options =>
{
    // Register actor types and configure actor settings
    options.Actors.RegisterActor<DeliveryManagerActor>();
    options.Actors.RegisterActor<DriverActor>();

    options.ReentrancyConfig = new ActorReentrancyConfig
    {
        Enabled = false
    };
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

    logger.LogInformation("New Order dispatched to be delivered: {Order}", order.Serialize());

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

    logger.LogInformation("Drivers registered and started work: {Drivers}", request.Drivers.Serialize());

    return Results.Ok();
});

app.MapActorsHandlers();

app.Run();

