using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using DaprizzaInterfaces;
using DaprizzaModels;

namespace DaprizzaKitchen.Actors;

public class KitchenManagerActor(
    ActorHost host,
    ILogger<KitchenManagerActor> logger) : Actor(host), IKitchenManagerActor, IRemindable
{
    private const string chefsKey = "chefs";
    private const string ordersInQueue = "ordersInQueue";
    private const string chefActorIdPrefix = "chef";

    public async Task RegisterChefs(IEnumerable<Chef> chefs)
    {
        await StateManager.SetStateAsync(chefsKey, chefs);

        // Get all the chefs working:
        foreach (var chef in chefs)
        {
            var actorId = new ActorId($"{chefActorIdPrefix}-{chef.Name!.ToLowerInvariant()}");
            var proxy = ActorProxy.Create<IChefActor>(actorId, nameof(ChefActor));

            await proxy.StartCooking();
        }

        await this.RegisterReminderAsync("CheckOnKitchen", null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public async Task<IEnumerable<Chef>> ListChefs()
    {
        var chefs = await StateManager.TryGetStateAsync<IEnumerable<Chef>>(chefsKey);

        return chefs.HasValue ? chefs.Value : new List<Chef>();
    }

    public async Task EnqueueOrder(Order order)
    {
        var ordersTest = await StateManager.TryGetStateAsync<List<Order>>(ordersInQueue);

        var orders = ordersTest.HasValue ? ordersTest.Value : [];

        logger.LogInformation("EnqueueOrder: Orders On Queue BEFORE: {Orders}", orders.Select(o => o.Id).Serialize());

        orders.Add(order);

        await StateManager.SetStateAsync(ordersInQueue, orders);

        logger.LogInformation("EnqueueOrder: Orders On Queue AFTER: {Orders}", orders.Select(o => o.Id).Serialize());

        logger.LogInformation("Order received: {Order}", order);
    }

    public async Task<Order> DequeueOrder()
    {
        var ordersTest = await StateManager.TryGetStateAsync<List<Order>>(ordersInQueue);
        Order dequeuedOrder = default;

        if (ordersTest.HasValue && ordersTest.Value.Any())
        {
            var orders = ordersTest.Value;

            logger.LogInformation("DequeueOrder: Orders On Queue BEFORE: {Orders}", orders.Select(o => o.Id).Serialize());

            dequeuedOrder = orders[0];
            orders.RemoveAt(0);
            await StateManager.SetStateAsync(ordersInQueue, orders);

            logger.LogInformation("DequeueOrder: Orders On Queue AFTER: {Orders}", orders.Select(o => o.Id).Serialize());

            logger.LogInformation("Order dequeued: {Order}", dequeuedOrder.Serialize());
        }

        return dequeuedOrder;
    }

    public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckOnKitchen")
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("TODO: Check what the chefs are up to...");

        return Task.CompletedTask;
    }
}

