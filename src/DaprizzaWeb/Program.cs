using Dapr;
using DaprizzaShared;
using DaprizzaWeb.Components;
using DaprizzaWeb.Models;
using DaprizzaWeb.Models.Validators;
using FluentValidation;
using Mdl = DaprizzaModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IValidator<Order>, OrderValidator>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery();

const string daprPubSubName = "orderstatuspubsub";

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orderstatus", [Topic(daprPubSubName, "orderstatus")] async (
    ILogger<Program> logger,
    Mdl.OrderStatusUpdate orderStatusUpdate,
    CancellationToken token) =>
{
    logger.LogInformation("Order Updated On Web: {OrderUpdated}", orderStatusUpdate.Serialize());
});

app.Run();
