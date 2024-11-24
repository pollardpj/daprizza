using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Dapr.Client;
using DaprizzaInterfaces;
using DaprizzaModels;

namespace DaprizzaKitchen.Actors;

public class ChefActor(
    ActorHost host,
    ILogger<ChefActor> logger) : Actor(host), IChefActor, IRemindable
{
    private const string cookingOrder = "cookingOrder";

    public async Task StartCooking()
    {
        await RegisterReminderAsync("CheckInWithKitchenManager", null, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(7));
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName != "CheckInWithKitchenManager")
        {
            return;
        }

        var current = await StateManager.TryGetStateAsync<Order>(cookingOrder);

        if (current.HasValue)
        {
            logger.LogInformation("I ({ChefActor}), am cooking {Order}", Id.ToString(), current.Value.Serialize());

            if (new Random().Next(1, 10) > 5)
            {
                var orderStatusUpdate = new OrderStatusUpdate
                {
                    OrderId = current.Value.Id,
                    Status = OrderStatus.ReadyForDelivery,
                    UpdatedTimestampUtc = DateTime.UtcNow
                };

                using var client = new DaprClientBuilder().Build();

                await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);

                await StateManager.TryRemoveStateAsync(cookingOrder);

                logger.LogInformation("I ({ChefActor}), have an order ready to deliver: {Order}", Id.ToString(), current.Value.Serialize());
            }

            return;
        }

        logger.LogInformation("I ({ChefActor}), am looking for pizzas to cook...", Id.ToString());

        var managerActorId = new ActorId(Constants.KitchenManagerActorId);
        var proxy = ActorProxy.Create<IKitchenManagerActor>(managerActorId, nameof(KitchenManagerActor));

        var order = await proxy.DequeueOrder();

        if (order != null)
        {
            await StateManager.SetStateAsync(cookingOrder, order);

            logger.LogInformation("I ({ChefActor}), have started cooking {Order}", Id.ToString(), order.Serialize());

            var orderStatusUpdate = new OrderStatusUpdate
            {
                OrderId = order.Id,
                Status = OrderStatus.CookingInProgress,
                UpdatedTimestampUtc = DateTime.UtcNow
            };

            using var client = new DaprClientBuilder().Build();

            await client.PublishEventAsync(Constants.DaprOrderStatePubSubName, "orderstatus", orderStatusUpdate);
        }
    }
}

