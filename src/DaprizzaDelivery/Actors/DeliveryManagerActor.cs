using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using DaprizzaInterfaces;
using DaprizzaModels;

namespace DaprizzaDelivery.Actors;

public class DeliveryManagerActor(
    ActorHost host,
    ILogger<DeliveryManagerActor> logger) : Actor(host), IDeliveryManagerActor, IRemindable
{
    private const string driversKey = "drivers";
    private const string ordersInQueue = "ordersInQueue";
    private const string driverActorIdPrefix = "driver";

    public async Task RegisterDrivers(IEnumerable<Driver> drivers)
    {
        await StateManager.SetStateAsync(driversKey, drivers);

        // Get all the drivers working:
        foreach (var driver in drivers)
        {
            var actorId = new ActorId($"{driverActorIdPrefix}-{driver.Name.ToLowerInvariant()}");
            var proxy = ActorProxy.Create<IDriverActor>(actorId, nameof(DriverActor));

            await proxy.StartDelivering(driver);
        }

        await this.RegisterReminderAsync("CheckOnDrivers", null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public async Task<IEnumerable<Driver>> ListDrivers()
    {
        var drivers = await StateManager.TryGetStateAsync<IEnumerable<Driver>>(driversKey);

        return drivers.HasValue ? drivers.Value : new List<Driver>();
    }

    public async Task EnqueueOrder(Order order)
    {
        var ordersTest = await StateManager.TryGetStateAsync<List<Order>>(ordersInQueue);

        var orders = ordersTest.HasValue ? ordersTest.Value : [];

        logger.LogInformation("EnqueueOrder: Orders On Queue BEFORE: {Orders}", orders.Select(o => o.Id).Serialize());

        orders.Add(order);

        await StateManager.SetStateAsync(ordersInQueue, orders);

        logger.LogInformation("EnqueueOrder: Orders On Queue AFTER: {Orders}", orders.Select(o => o.Id).Serialize());

        logger.LogInformation("Order received: {Order}", order.Serialize());
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
        if (reminderName != "CheckOnDrivers")
        {
            return Task.CompletedTask;
        }

        logger.LogInformation("TODO: Check what the drivers are up to...");

        return Task.CompletedTask;
    }
}

