using Dapr.Actors;
using Dapr.Actors.Client;
using DaprizzaInterfaces;
using DaprizzaKitchen;
using DaprizzaKitchen.Actors;
using DaprizzaModels;
using DaprizzaModels.Validators;
using FluentValidation;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<RegisterChefsRequest>, RegisterChefsRequestValidator>();

builder.Services.AddActors(options =>
{
    // Register actor types and configure actor settings
    options.Actors.RegisterActor<KitchenManagerActor>();
    options.Actors.RegisterActor<ChefActor>();
});

// Add services to the container.

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// Configure the HTTP request pipeline.

app.MapPost("/cook", async (
    ILogger<Program> logger, 
    Order order, 
    CancellationToken token) =>
{
    var actorId = new ActorId(Constants.KitchenManagerActorId);
    var proxy = ActorProxy.Create<IKitchenManagerActor>(actorId, nameof(KitchenManagerActor));

    await proxy.EnqueueOrder(order);

    logger.LogInformation("New Order accepted into the kitchen: {Order}", JsonSerializer.Serialize(order));

    return Results.Accepted();
});

app.MapMethods("/api/chefs/register", ["POST", "PUT"], async (
    ILogger<Program> logger,
    IValidator<RegisterChefsRequest> validator, 
    RegisterChefsRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var actorId = new ActorId(Constants.KitchenManagerActorId);
    var proxy = ActorProxy.Create<IKitchenManagerActor>(actorId, nameof(KitchenManagerActor));

    await proxy.RegisterChefs(request.Chefs);

    logger.LogInformation("Chefs registered and started work: {Chefs}", JsonSerializer.Serialize(request.Chefs));

    return Results.Ok();
});

app.MapActorsHandlers();

app.Run();
