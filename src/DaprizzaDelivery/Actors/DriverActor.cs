using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Dapr.Client;
using DaprizzaInterfaces;
using DaprizzaModels;
using System.Text.Json;

namespace DaprizzaDelivery.Actors;

public class DriverActor(
    ActorHost host,
    ILogger<DriverActor> logger) : Actor(host), IDriverActor, IRemindable
{
    private const string deliverOrder = "deliverOrder";

    public async Task StartDelivering()
    {
        await RegisterReminderAsync("CheckInWithDeliveryManager", null, TimeSpan.FromSeconds(13), TimeSpan.FromSeconds(13));
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckInWithDeliveryManager")
        {
            return;
        }

        var current = await StateManager.TryGetStateAsync<Order>(deliverOrder);

        if (current.HasValue)
        {
            logger.LogInformation("I ({DriverActor}), am delivering {Order}", 
                Id.ToString(), JsonSerializer.Serialize(current.Value));

            if (new Random().Next(1, 10) > 5)
            {
                var orderStatusUpdate = new OrderStatusUpdate
                {
                    OrderId = current.Value.Id,
                    Status = OrderStatus.Delivered,
                    UpdatedTimestampUtc = DateTime.UtcNow
                };

                using var client = new DaprClientBuilder().Build();

                await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);

                await StateManager.TryRemoveStateAsync(deliverOrder);

                logger.LogInformation("I ({DriverActor}), have delivered {Order}",
                    Id.ToString(), JsonSerializer.Serialize(current.Value));
            }

            return;
        }

        logger.LogInformation("I ({DriverActor}), am looking for pizzas to deliver...", Id.ToString());

        var managerActorId = new ActorId(Constants.DeliveryManagerActorId);
        var proxy = ActorProxy.Create<IDeliveryManagerActor>(managerActorId, nameof(DeliveryManagerActor));

        var order = await proxy.DequeueOrder();

        if (order != null)
        {
            await StateManager.SetStateAsync(deliverOrder, order);

            logger.LogInformation("I ({DriverActor}), have started delivering {Order}", 
                Id.ToString(), JsonSerializer.Serialize(order));

            var orderStatusUpdate = new OrderStatusUpdate
            {
                OrderId = order.Id,
                Status = OrderStatus.InTransit,
                UpdatedTimestampUtc = DateTime.UtcNow
            };

            using var client = new DaprClientBuilder().Build();

            await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);
        }
    }
}

