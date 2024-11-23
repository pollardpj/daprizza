using DaprizzaModels;
using DaprizzaStore.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IValidator<OrderRequest>, OrderRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapPost("/order", async (IValidator<OrderRequest> validator, OrderRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var order = new Order(
        Guid.NewGuid(), 
        DateTime.UtcNow, 
        request.Pizzas, 
        request.Address,
        request.Pizzas.Sum(p => 10M * p.Size switch
        {
            PizzaSize.Large => 3,
            PizzaSize.Medium => 2,
            _ => 1
        }));

    return Results.Ok(new OrderResponse(order.OrderId, order.TotalPrice));
});

app.Run();

