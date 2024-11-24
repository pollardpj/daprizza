using Dapr.Actors;
using DaprizzaModels;

namespace DaprizzaInterfaces;

public interface IDriverActor : IActor
{
    Task StartDelivering(Driver driver);
}

