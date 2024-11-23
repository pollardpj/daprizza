using Dapr.Actors.Runtime;
using DaprizzaInterfaces;
using DaprizzaModels;

namespace DaprizzaKitchen.Actors;

public class KitchenManagerActor(
    ActorHost host,
    ILogger<KitchenManagerActor> logger) : Actor(host), IKitchenManagerActor, IRemindable
{
    private const string chefsKey = "chefs";
    private const string ordersInProgress = "ordersInProgress";

    public async Task RegisterChefs(IEnumerable<Chef> chefs)
    {
        await StateManager.SetStateAsync(chefsKey, chefs);

        await this.RegisterReminderAsync("CheckOnKitchen", null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public async Task<IEnumerable<Chef>> ListChefs()
    {
        return await StateManager.GetStateAsync<IEnumerable<Chef>>(chefsKey);
    }

    public async Task EnqueueOrder(Order order)
    {
        var orders = await StateManager.GetStateAsync<List<Order>>(ordersInProgress);
        orders.Add(order);

        await StateManager.SetStateAsync(ordersInProgress, orders);

        logger.LogInformation("Order received: {Order}", order);
    }

    public async Task<Order?> DequeueOrder()
    {
        var orders = await StateManager.GetStateAsync<List<Order>>(ordersInProgress);
        Order? dequeuedOrder = default;

        if (orders?.Any() == true)
        {
            dequeuedOrder = orders[0];
            orders.RemoveAt(0);
            await StateManager.SetStateAsync(ordersInProgress, orders);
            logger.LogInformation("Order dequeued: {Order}", dequeuedOrder);
        }

        return dequeuedOrder;
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        return Task.CompletedTask;
    }
}

