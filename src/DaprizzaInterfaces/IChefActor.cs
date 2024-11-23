using Dapr.Actors;

namespace DaprizzaInterfaces;

public interface IChefActor : IActor
{
    Task StartCooking();
}

