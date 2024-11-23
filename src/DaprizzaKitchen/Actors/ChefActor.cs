using System.Text.Json;
using Dapr.Actors.Client;
using Dapr.Actors;
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

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckInWithKitchenManager")
        {
            return;
        }

        logger.LogInformation("I ({ChefActor}), am looking for pizzas to cook...", Id.ToString());

        var managerActorId = new ActorId(Constants.kitchenManagerActorId);
        var proxy = ActorProxy.Create<IKitchenManagerActor>(managerActorId, nameof(KitchenManagerActor));

        var order = await proxy.DequeueOrder();

        if (order != null)
        {
            logger.LogInformation("I am cooking {Id} {OrderDetails}", order.Id, JsonSerializer.Serialize(order));
        }
    }
}

