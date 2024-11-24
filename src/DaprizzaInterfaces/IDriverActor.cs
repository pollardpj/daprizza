using Dapr.Actors;

namespace DaprizzaInterfaces;

public interface IDriverActor : IActor
{
    Task StartDelivering();
}

