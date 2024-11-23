using Dapr.Actors.Runtime;
using DaprizzaInterfaces;

namespace DaprizzaKitchen.Actors;

public class ChefActor(
    ActorHost host,
    ILogger<ChefActor> logger) : Actor(host), IChefActor, IRemindable
{
    public async Task StartCooking()
    {
        await this.RegisterReminderAsync("CheckInWithKitchenManager", null, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(7));
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckInWithKitchenManager")
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("I ({ChefActor}), am looking for pizzas to cook...", Id.ToString());

        return Task.CompletedTask;
    }
}

