using Dapr.Actors;
using DaprizzaModels;

namespace DaprizzaInterfaces;

public interface IKitchenManagerActor : IActor
{
    Task RegisterChefs(IEnumerable<Chef> chefs);
    Task<IEnumerable<Chef>> ListChefs();
    Task EnqueueOrder(Order order);
    Task<Order> DequeueOrder();
}

