using Dapr.Actors.Runtime;
using DaprizzaInterfaces;

namespace DaprizzaKitchen.Actors;

public class ChefActor(ActorHost host) : Actor(host), IChefActor, IRemindable
{
    private const string chefsKey = "chefs";
    private const string ordersInProgress = "ordersInProgress";

    public async Task StartCooking()
    {
        await this.RegisterReminderAsync("CheckInWithKitchenManager", null, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(7));
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        return Task.CompletedTask;
    }
}

