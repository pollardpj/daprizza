using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Dapr.Client;
using DaprizzaInterfaces;
using DaprizzaModels;
using DaprizzaShared;

namespace DaprizzaDelivery.Actors;

public class DriverActor(
    ActorHost host,
    ILogger<DriverActor> logger) : Actor(host), IDriverActor, IRemindable
{
    private const string deliveringOrder = "deliveringOrder";
    private const string driverStateName = "driver";

    public async Task StartDelivering(Driver driver)
    {
        await StateManager.SetStateAsync(driverStateName, driver);

        await RegisterReminderAsync("CheckInWithDeliveryManager", null, TimeSpan.FromSeconds(13), TimeSpan.FromSeconds(13));
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckInWithDeliveryManager")
        {
            return;
        }

        var current = await StateManager.TryGetStateAsync<Order>(deliveringOrder);

        if (current.HasValue)
        {
            logger.LogInformation("I ({DriverActor}), am delivering {Order}", 
                Id.ToString(), current.Value.Serialize());

            if (new Random().Next(1, 10) > 5)
            {
                var driver = await StateManager.TryGetStateAsync<Driver>(driverStateName);

                var orderStatusUpdate = new OrderStatusUpdate
                {
                    OrderId = current.Value.Id,
                    Status = OrderStatus.Delivered,
                    UpdatedTimestampUtc = DateTime.UtcNow,
                    UpdatedByActorName = driver.HasValue ? driver.Value.Name : null
                };

                using var client = new DaprClientBuilder().Build();

                await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);

                await StateManager.TryRemoveStateAsync(deliveringOrder);

                logger.LogInformation("I ({DriverActor}), have delivered {Order}",
                    Id.ToString(), current.Value.Serialize());
            }

            return;
        }

        logger.LogInformation("I ({DriverActor}), am looking for pizzas to deliver...", Id.ToString());

        var managerActorId = new ActorId(Constants.DeliveryManagerActorId);
        var proxy = ActorProxy.Create<IDeliveryManagerActor>(managerActorId, nameof(DeliveryManagerActor));

        var order = await proxy.DequeueOrder();

        if (order != null)
        {
            await StateManager.SetStateAsync(deliveringOrder, order);

            logger.LogInformation("I ({DriverActor}), have started delivering {Order}", 
                Id.ToString(), order.Serialize());

            var driver = await StateManager.TryGetStateAsync<Driver>(driverStateName);

            var orderStatusUpdate = new OrderStatusUpdate
            {
                OrderId = order.Id,
                Status = OrderStatus.InTransit,
                UpdatedTimestampUtc = DateTime.UtcNow,
                UpdatedByActorName = driver.HasValue ? driver.Value.Name : null
            };

            using var client = new DaprClientBuilder().Build();

            await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);
        }
    }
}

