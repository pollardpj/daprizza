using DaprizzaModels;
using DaprizzaShared;

namespace DaprizzaStore;

public static class Extensions
{
    public static decimal GetTotalPrice(this IEnumerable<Pizza> pizzas)
    {
        return pizzas.Sum(p => 5M * p.Size switch
        {
            PizzaSize.Large => 3,
            PizzaSize.Medium => 2,
            PizzaSize.Small => 1,
            _ => throw new InvalidOperationException($"Pizza with size = {p.Size} doesn't have a price")
        } + p.Toppings.Count());
    }
}

