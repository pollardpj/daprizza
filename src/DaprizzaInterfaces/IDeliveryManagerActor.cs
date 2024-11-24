using Dapr.Actors;
using DaprizzaModels;

namespace DaprizzaInterfaces;

public interface IDeliveryManagerActor : IActor
{
    Task RegisterDrivers(IEnumerable<Driver> chefs);
    Task<IEnumerable<Driver>> ListDrivers();
    Task EnqueueOrder(Order order);
    Task<Order> DequeueOrder();
}

